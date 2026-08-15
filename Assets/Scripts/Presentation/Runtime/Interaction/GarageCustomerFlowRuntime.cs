using System;
using PCShopEmpire3D.Actors;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Presentation.Player;
using PCShopEmpire3D.Retail;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace PCShopEmpire3D.Presentation.Interaction
{
    /// <summary>
    /// Projects one authoritative customer visit onto the Garage graybox NavMesh. The agent may
    /// only report arrival or route failure; customer intent and lifecycle remain domain-owned.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GarageCustomerFlowRuntime : MonoBehaviour
    {
        private const float ArrivalTolerance = 0.10f;
        private const float RouteProgressEpsilon = 0.025f;
        private const long FixedStepMilliseconds = 20L;
        private const long RouteStallMilliseconds = 4_000L;
        private const float SampleRadius = 0.45f;

        [SerializeField] private GarageStockFlowRuntime stockFlow;
        [SerializeField] private FirstPersonMotor playerMotor;
        [SerializeField] private NavMeshSurface navigationSurface;
        [SerializeField] private NavMeshAgent customerAgent;
        [SerializeField] private GameObject customerVisualRoot;
        [SerializeField] private TextMesh customerStatusText;
        [SerializeField] private Transform entranceWaypoint;
        [SerializeField] private Transform browseWaypoint;
        [SerializeField] private Transform checkoutWaypoint;
        [SerializeField] private Transform exitWaypoint;

        private bool _visitStarted;
        private bool _navigationReady;
        private bool _routeAssigned;
        private bool _configurationFailureLogged;
        private CustomerVisitState _routeState;
        private readonly SimulationClock _simulationClock = new SimulationClock();
        private long _retryNotBeforeMilliseconds;
        private float _bestRemainingDistance = float.PositiveInfinity;
        private long _lastRouteProgressMilliseconds;
        private bool _wasPaused;
        private Vector3 _entrancePoint;
        private Vector3 _browsePoint;
        private Vector3 _checkoutPoint;
        private Vector3 _exitPoint;

        public GarageStockFlowRuntime StockFlow => stockFlow;

        public FirstPersonMotor PlayerMotor => playerMotor;

        public NavMeshSurface NavigationSurface => navigationSurface;

        public NavMeshAgent CustomerAgent => customerAgent;

        public GameObject CustomerVisualRoot => customerVisualRoot;

        public TextMesh CustomerStatusText => customerStatusText;

        public Transform EntranceWaypoint => entranceWaypoint;

        public Transform BrowseWaypoint => browseWaypoint;

        public Transform CheckoutWaypoint => checkoutWaypoint;

        public Transform ExitWaypoint => exitWaypoint;

        public bool NavigationReady => _navigationReady;

        public bool CustomerVisible => customerVisualRoot != null && customerVisualRoot.activeSelf;

        public bool VisitStarted => _visitStarted;

        public bool HasAssignedRoute => _routeAssigned;

        public SimulationTimestamp CurrentSimulationTime => _simulationClock.Current;

        public CustomerVisitRecord CurrentVisit
        {
            get
            {
                GarageStockFlowSession session = stockFlow != null
                    ? stockFlow.EnsureInitialized()
                    : null;
                return session != null && session.TryGetPrototypeCustomerVisit(out CustomerVisitRecord visit)
                    ? visit
                    : null;
            }
        }

        public CustomerOfferDecision CurrentOfferDecision
        {
            get
            {
                GarageStockFlowSession session = stockFlow != null
                    ? stockFlow.EnsureInitialized()
                    : null;
                if (session == null)
                {
                    return null;
                }

                OperationResult<CustomerOfferDecision> result =
                    session.EvaluatePrototypeCustomerOffer();
                return result.TryGetValue(out CustomerOfferDecision decision)
                    ? decision
                    : null;
            }
        }

        public string OfferDecisionText
        {
            get
            {
                CustomerOfferDecision decision = CurrentOfferDecision;
                if (decision == null)
                {
                    return string.Empty;
                }

                return decision.DecisionKind == CustomerOfferDecisionKind.Buy
                    ? "KARAR: SATIN AL"
                    : "KARAR: AYRIL";
            }
        }

        public string OfferDecisionReasonCode => CurrentOfferDecision?.ReasonCode ?? string.Empty;

        public string StateText
        {
            get
            {
                CustomerVisitRecord visit = CurrentVisit;
                if (visit == null)
                {
                    return "TEKLİF BEKLİYOR";
                }

                switch (visit.State)
                {
                    case CustomerVisitState.Entering:
                        return "MAĞAZAYA GİRİYOR";
                    case CustomerVisitState.Browsing:
                        string decisionText = OfferDecisionText;
                        return string.IsNullOrEmpty(decisionText)
                            ? "RAF ÜRÜNÜNÜ İNCELİYOR • KARAR HAZIRLANIYOR"
                            : $"RAF ÜRÜNÜNÜ İNCELİYOR • {decisionText}";
                    case CustomerVisitState.NavigatingToCheckout:
                        return "KASAYA İLERLİYOR";
                    case CustomerVisitState.AwaitingCheckout:
                        return "KASADA BEKLİYOR";
                    case CustomerVisitState.Exiting:
                        return visit.ExitReason == CustomerVisitExitReason.Fulfilled
                            ? "SATIŞ TAMAM • ÇIKIYOR"
                            : $"GÜVENLİ ÇIKIŞ • {FormatExitReason(visit.ExitReason)}";
                    case CustomerVisitState.Exited:
                        return $"AYRILDI • {FormatExitReason(visit.ExitReason)}";
                    default:
                        return "DURUM GEÇERSİZ";
                }
            }
        }

        public string StatusText
        {
            get
            {
                CustomerVisitRecord visit = CurrentVisit;
                string route = !_navigationReady
                    ? "NAVMESH HAZIR DEĞİL"
                    : visit != null && IsRouteState(visit.State)
                        ? (_routeAssigned ? "ROTA AKTİF" : "ROTA HAZIRLANIYOR")
                        : "ROTA BEKLEMEDE";
                string reasonCode = OfferDecisionReasonCode;
                return string.IsNullOrEmpty(reasonCode)
                    ? $"MÜŞTERİ AKIŞI: {StateText}\n{route}"
                    : $"MÜŞTERİ AKIŞI: {StateText}\n{route}\n{reasonCode}";
            }
        }

        public void Configure(
            GarageStockFlowRuntime garageStockFlow,
            FirstPersonMotor motor,
            NavMeshSurface surface,
            NavMeshAgent agent,
            GameObject visualRoot,
            TextMesh statusText,
            Transform entrance,
            Transform browse,
            Transform checkout,
            Transform exit)
        {
            stockFlow = garageStockFlow != null
                ? garageStockFlow
                : throw new ArgumentNullException(nameof(garageStockFlow));
            playerMotor = motor != null
                ? motor
                : throw new ArgumentNullException(nameof(motor));
            navigationSurface = surface != null
                ? surface
                : throw new ArgumentNullException(nameof(surface));
            customerAgent = agent != null
                ? agent
                : throw new ArgumentNullException(nameof(agent));
            customerVisualRoot = visualRoot != null
                ? visualRoot
                : throw new ArgumentNullException(nameof(visualRoot));
            customerStatusText = statusText;
            entranceWaypoint = entrance != null
                ? entrance
                : throw new ArgumentNullException(nameof(entrance));
            browseWaypoint = browse != null
                ? browse
                : throw new ArgumentNullException(nameof(browse));
            checkoutWaypoint = checkout != null
                ? checkout
                : throw new ArgumentNullException(nameof(checkout));
            exitWaypoint = exit != null
                ? exit
                : throw new ArgumentNullException(nameof(exit));
            customerVisualRoot.SetActive(false);
            RefreshPresentation();
        }

        public bool EnsureNavigationBuilt()
        {
            if (_navigationReady)
            {
                return true;
            }

            if (!HasCompleteConfiguration())
            {
                LogConfigurationFailureOnce("customer-flow.configuration-missing");
                return false;
            }

            try
            {
                if (navigationSurface.navMeshData == null)
                {
                    navigationSurface.BuildNavMesh();
                }

                bool sampled = TrySamplePoint(entranceWaypoint.position, out _entrancePoint) &&
                               TrySamplePoint(browseWaypoint.position, out _browsePoint) &&
                               TrySamplePoint(checkoutWaypoint.position, out _checkoutPoint) &&
                               TrySamplePoint(exitWaypoint.position, out _exitPoint);
                _navigationReady = sampled &&
                                   HasCompletePath(_entrancePoint, _browsePoint) &&
                                   HasCompletePath(_browsePoint, _checkoutPoint) &&
                                   HasCompletePath(_checkoutPoint, _exitPoint);
                if (!_navigationReady)
                {
                    LogConfigurationFailureOnce("customer-flow.route-contract-unavailable");
                }
            }
            catch (Exception exception)
            {
                _navigationReady = false;
                LogConfigurationFailureOnce(
                    $"customer-flow.navmesh-build-failed type={exception.GetType().Name}");
            }

            RefreshPresentation();
            return _navigationReady;
        }

        public void RefreshPresentation()
        {
            if (customerStatusText != null)
            {
                customerStatusText.text = StatusText;
            }
        }

        private void Awake()
        {
            stockFlow?.EnsureInitialized();
            if (customerVisualRoot != null)
            {
                customerVisualRoot.SetActive(false);
            }

            EnsureNavigationBuilt();
            RefreshPresentation();
        }

        private void FixedUpdate()
        {
            bool paused = playerMotor != null && playerMotor.IsPaused;
            if (paused)
            {
                _simulationClock.Pause();
                SetAgentPaused(true);
                _wasPaused = true;
                return;
            }

            if (_wasPaused)
            {
                _simulationClock.Resume();
                SetAgentPaused(false);
                _lastRouteProgressMilliseconds =
                    CurrentSimulationTime.ElapsedMilliseconds;
                _wasPaused = false;
            }

            OperationResult<SimulationTimestamp> clockAdvance = _simulationClock.Advance(
                SimulationDuration.FromMilliseconds(FixedStepMilliseconds));
            if (clockAdvance.IsFailure)
            {
                LogConfigurationFailureOnce(
                    $"customer-flow.clock-advance-failed code={clockAdvance.Error.Code}");
                return;
            }

            GarageStockFlowSession session = stockFlow != null
                ? stockFlow.EnsureInitialized()
                : null;
            if (session == null)
            {
                LogConfigurationFailureOnce("customer-flow.stock-session-missing");
                return;
            }

            if (!_visitStarted)
            {
                if (session.TryGetShelfOffer(out _))
                {
                    StartVisit(session);
                }

                RefreshPresentation();
                return;
            }

            if (!session.TryGetPrototypeCustomerVisit(out CustomerVisitRecord visit))
            {
                LogConfigurationFailureOnce("customer-flow.visit-missing");
                return;
            }

            DriveVisit(session, visit);
            OperationResult advance = session.AdvanceCustomerTime(CurrentSimulationTime);
            if (advance.IsFailure)
            {
                LogConfigurationFailureOnce($"customer-flow.advance-failed code={advance.Error.Code}");
                return;
            }

            if (session.TryGetPrototypeCustomerVisit(out CustomerVisitRecord advancedVisit) &&
                advancedVisit.State == CustomerVisitState.Exited)
            {
                HideTerminalCustomer();
            }

            RefreshPresentation();
        }

        private void StartVisit(GarageStockFlowSession session)
        {
            OperationResult start = session.StartPrototypeCustomerVisit(CurrentSimulationTime);
            if (start.IsFailure)
            {
                LogConfigurationFailureOnce($"customer-flow.start-failed code={start.Error.Code}");
                return;
            }

            _visitStarted = true;
            if (!EnsureNavigationBuilt() || !ActivateCustomerAtEntrance())
            {
                _retryNotBeforeMilliseconds =
                    CurrentSimulationTime.ElapsedMilliseconds + FixedStepMilliseconds;
                return;
            }

            _routeAssigned = false;
        }

        private void DriveVisit(
            GarageStockFlowSession session,
            CustomerVisitRecord visit)
        {
            switch (visit.State)
            {
                case CustomerVisitState.Entering:
                    DriveRoute(
                        session,
                        visit,
                        _browsePoint,
                        () => session.MarkPrototypeCustomerBrowseArrival(CurrentSimulationTime));
                    break;
                case CustomerVisitState.Browsing:
                    StopAgent();
                    if (session.TryGetPrototypeBasketLine(out _))
                    {
                        ApplyTransition(
                            session.BeginPrototypeCustomerCheckoutNavigation(CurrentSimulationTime));
                    }
                    break;
                case CustomerVisitState.NavigatingToCheckout:
                    DriveRoute(
                        session,
                        visit,
                        _checkoutPoint,
                        () => session.MarkPrototypeCustomerCheckoutArrival(CurrentSimulationTime));
                    break;
                case CustomerVisitState.AwaitingCheckout:
                    StopAgent();
                    if (session.TryGetPrototypeCheckoutCompletion(out _))
                    {
                        ApplyTransition(session.BeginPrototypeCustomerExit(
                            CustomerVisitExitReason.Fulfilled,
                            CurrentSimulationTime));
                    }
                    break;
                case CustomerVisitState.Exiting:
                    DriveRoute(
                        session,
                        visit,
                        _exitPoint,
                        () => session.MarkPrototypeCustomerExitArrival(CurrentSimulationTime));
                    break;
                case CustomerVisitState.Exited:
                    HideTerminalCustomer();
                    break;
            }
        }

        private void DriveRoute(
            GarageStockFlowSession session,
            CustomerVisitRecord visit,
            Vector3 destination,
            Func<OperationResult> arrive)
        {
            if (!CustomerVisible)
            {
                if (!ActivateCustomerAtEntrance())
                {
                    ReportRouteFailure(session);
                }

                return;
            }

            if (_routeState != visit.State)
            {
                _routeState = visit.State;
                _routeAssigned = false;
            }

            if (!_routeAssigned)
            {
                if (CurrentSimulationTime.ElapsedMilliseconds < _retryNotBeforeMilliseconds)
                {
                    return;
                }

                if (!TryAssignRoute(destination))
                {
                    ReportRouteFailure(session);
                }

                return;
            }

            if (HasArrived())
            {
                OperationResult arrival = arrive();
                ApplyTransition(arrival);
                if (arrival.IsSuccess &&
                    session.TryGetPrototypeCustomerVisit(out CustomerVisitRecord arrivedVisit) &&
                    arrivedVisit.State == CustomerVisitState.Exited)
                {
                    HideTerminalCustomer();
                }

                return;
            }

            if (RouteHasFailedOrStalled())
            {
                ReportRouteFailure(session);
            }
        }

        private bool ActivateCustomerAtEntrance()
        {
            if (!_navigationReady || customerVisualRoot == null || customerAgent == null)
            {
                return false;
            }

            customerVisualRoot.SetActive(true);
            customerAgent.enabled = true;
            bool warped = customerAgent.Warp(_entrancePoint);
            if (!warped || !customerAgent.isOnNavMesh)
            {
                customerVisualRoot.SetActive(false);
                return false;
            }

            _routeAssigned = false;
            return true;
        }

        private bool TryAssignRoute(Vector3 destination)
        {
            if (!_navigationReady || customerAgent == null ||
                !customerAgent.isActiveAndEnabled || !customerAgent.isOnNavMesh)
            {
                return false;
            }

            var path = new NavMeshPath();
            bool calculated = customerAgent.CalculatePath(destination, path);
            if (!calculated || path.status != NavMeshPathStatus.PathComplete || path.corners.Length < 1)
            {
                return false;
            }

            if (!customerAgent.SetPath(path))
            {
                return false;
            }

            _routeAssigned = true;
            _bestRemainingDistance = CalculatePathLength(path);
            _lastRouteProgressMilliseconds = CurrentSimulationTime.ElapsedMilliseconds;
            return true;
        }

        private bool HasArrived()
        {
            return customerAgent != null &&
                   customerAgent.isActiveAndEnabled &&
                   customerAgent.isOnNavMesh &&
                   !customerAgent.pathPending &&
                   customerAgent.pathStatus == NavMeshPathStatus.PathComplete &&
                   customerAgent.remainingDistance <= customerAgent.stoppingDistance + ArrivalTolerance;
        }

        private bool RouteHasFailedOrStalled()
        {
            if (customerAgent == null || !customerAgent.isActiveAndEnabled ||
                !customerAgent.isOnNavMesh)
            {
                return true;
            }

            if (customerAgent.pathPending)
            {
                return CurrentSimulationTime.ElapsedMilliseconds - _lastRouteProgressMilliseconds >=
                       RouteStallMilliseconds;
            }

            if (customerAgent.pathStatus != NavMeshPathStatus.PathComplete)
            {
                return true;
            }

            float remaining = customerAgent.remainingDistance;
            if (float.IsInfinity(remaining) || float.IsNaN(remaining))
            {
                return CurrentSimulationTime.ElapsedMilliseconds - _lastRouteProgressMilliseconds >=
                       RouteStallMilliseconds;
            }

            if (remaining + RouteProgressEpsilon < _bestRemainingDistance)
            {
                _bestRemainingDistance = remaining;
                _lastRouteProgressMilliseconds = CurrentSimulationTime.ElapsedMilliseconds;
            }

            return CurrentSimulationTime.ElapsedMilliseconds - _lastRouteProgressMilliseconds >=
                   RouteStallMilliseconds;
        }

        private void ReportRouteFailure(GarageStockFlowSession session)
        {
            StopAgent();
            OperationResult failure = session.ReportPrototypeCustomerRouteFailure(
                CurrentSimulationTime);
            if (failure.IsFailure)
            {
                LogConfigurationFailureOnce($"customer-flow.route-report-failed code={failure.Error.Code}");
                return;
            }

            _retryNotBeforeMilliseconds =
                CurrentSimulationTime.ElapsedMilliseconds + FixedStepMilliseconds;
            if (session.TryGetPrototypeCustomerVisit(out CustomerVisitRecord visit) &&
                visit.State == CustomerVisitState.Exited)
            {
                HideTerminalCustomer();
            }
        }

        private void HideTerminalCustomer()
        {
            StopAgent();
            if (customerVisualRoot != null)
            {
                customerVisualRoot.SetActive(false);
            }
        }

        private void ApplyTransition(OperationResult result)
        {
            if (result.IsFailure)
            {
                LogConfigurationFailureOnce($"customer-flow.transition-failed code={result.Error.Code}");
                return;
            }

            StopAgent();
            _routeAssigned = false;
        }

        private void StopAgent()
        {
            if (customerAgent != null && customerAgent.isActiveAndEnabled && customerAgent.isOnNavMesh)
            {
                customerAgent.ResetPath();
            }

            _routeAssigned = false;
            _bestRemainingDistance = float.PositiveInfinity;
        }

        private bool HasCompleteConfiguration()
        {
            return stockFlow != null &&
                   playerMotor != null &&
                   navigationSurface != null &&
                   customerAgent != null &&
                   customerVisualRoot != null &&
                   entranceWaypoint != null &&
                   browseWaypoint != null &&
                   checkoutWaypoint != null &&
                   exitWaypoint != null;
        }

        private static bool TrySamplePoint(Vector3 position, out Vector3 sampled)
        {
            if (NavMesh.SamplePosition(position, out NavMeshHit hit, SampleRadius, NavMesh.AllAreas))
            {
                sampled = hit.position;
                return true;
            }

            sampled = default;
            return false;
        }

        private static bool HasCompletePath(Vector3 start, Vector3 end)
        {
            var path = new NavMeshPath();
            return NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path) &&
                   path.status == NavMeshPathStatus.PathComplete;
        }

        private static float CalculatePathLength(NavMeshPath path)
        {
            float distance = 0f;
            Vector3[] corners = path.corners;
            for (int index = 1; index < corners.Length; index++)
            {
                distance += Vector3.Distance(corners[index - 1], corners[index]);
            }

            return distance;
        }

        private void SetAgentPaused(bool paused)
        {
            if (customerAgent == null || !customerAgent.isActiveAndEnabled ||
                !customerAgent.isOnNavMesh)
            {
                return;
            }

            customerAgent.isStopped = paused;
            if (paused)
            {
                customerAgent.velocity = Vector3.zero;
            }
        }

        private void LogConfigurationFailureOnce(string code)
        {
            if (_configurationFailureLogged)
            {
                return;
            }

            _configurationFailureLogged = true;
            Debug.LogError($"GARAGE_CUSTOMER_FLOW_FAILED code={code}");
        }

        private static bool IsRouteState(CustomerVisitState state)
        {
            return state == CustomerVisitState.Entering ||
                   state == CustomerVisitState.NavigatingToCheckout ||
                   state == CustomerVisitState.Exiting;
        }

        private static string FormatExitReason(CustomerVisitExitReason reason)
        {
            switch (reason)
            {
                case CustomerVisitExitReason.Fulfilled:
                    return "SATIŞ TAMAMLANDI";
                case CustomerVisitExitReason.PatienceExpired:
                    return "SABIR SÜRESİ DOLDU";
                case CustomerVisitExitReason.RouteUnavailable:
                    return "ROTA BULUNAMADI";
                default:
                    return "SONUÇ BEKLENİYOR";
            }
        }
    }
}
