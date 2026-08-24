using System;
using PCShopEmpire3D.Actors;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Presentation.Input;
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
    [DefaultExecutionOrder(50)]
    public sealed class GarageCustomerFlowRuntime : MonoBehaviour
    {
        private const float ArrivalTolerance = 0.10f;
        private const float RouteProgressEpsilon = 0.025f;
        private const long FixedStepMilliseconds = 20L;
        private const long RouteStallMilliseconds = 4_000L;
        private const float SampleRadius = 0.45f;
        private const float ConsultationRange = 2.75f;
        private const float ConsultationFocusDegrees = 24f;
        private const float CustomerFocusHeight = 1.35f;

        [SerializeField] private GarageStockFlowRuntime stockFlow;
        [SerializeField] private FirstPersonMotor playerMotor;
        [SerializeField] private PlayerInputAdapter playerInput;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private NavMeshSurface navigationSurface;
        [SerializeField] private NavMeshAgent customerAgent;
        [SerializeField] private GameObject customerVisualRoot;
        [SerializeField] private TextMesh customerStatusText;
        [SerializeField] private TextMesh customerSpeechText;
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
        private CustomerOfferDecision _displayedOfferDecision;
        private StableId<CustomerVisitIdScope> _displayedDecisionVisitId;
        private string _lastOfferActionFailureCode = string.Empty;
        private StableId<CustomerVisitIdScope> _lastOfferActionVisitId;
        private string _lastConsultationFailureCode = string.Empty;
        private StableId<CustomerVisitIdScope> _lastConsultationVisitId;
        private string _lastCustomPcFailureCode = string.Empty;
        private StableId<CustomerVisitIdScope> _lastCustomPcVisitId;
        private bool _hasDeferredBrowsingDeadline;
        private StableId<CustomerVisitIdScope> _deferredDeadlineVisitId;
        private CustomerVisitState _deferredDeadlineState;
        private SimulationTimestamp _deferredStateDeadline;
        private int _deferredAtRenderedFrame;

        public GarageStockFlowRuntime StockFlow => stockFlow;

        public FirstPersonMotor PlayerMotor => playerMotor;

        public PlayerInputAdapter PlayerInput => playerInput;

        public Camera PlayerCamera => playerCamera;

        public NavMeshSurface NavigationSurface => navigationSurface;

        public NavMeshAgent CustomerAgent => customerAgent;

        public GameObject CustomerVisualRoot => customerVisualRoot;

        public TextMesh CustomerStatusText => customerStatusText;

        public TextMesh CustomerSpeechText => customerSpeechText;

        public Transform EntranceWaypoint => entranceWaypoint;

        public Transform BrowseWaypoint => browseWaypoint;

        public Transform CheckoutWaypoint => checkoutWaypoint;

        public Transform ExitWaypoint => exitWaypoint;

        public bool NavigationReady => _navigationReady;

        public bool CustomerVisible => customerVisualRoot != null && customerVisualRoot.activeSelf;

        public bool VisitStarted => _visitStarted;

        public bool HasAssignedRoute => _routeAssigned;

        public SimulationTimestamp CurrentSimulationTime => _simulationClock.Current;

        public SimulationTimestamp CurrentOfferActionTime
        {
            get => ResolveCurrentCommandTime(requireStrictlyAfterVisit: true);
        }

        public SimulationTimestamp CurrentConsultationTime =>
            ResolveCurrentCommandTime(requireStrictlyAfterVisit: false);

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

        public CustomerConsultationRecord CurrentConsultation
        {
            get
            {
                GarageStockFlowSession session = stockFlow != null
                    ? stockFlow.EnsureInitialized()
                    : null;
                return session != null &&
                       session.TryGetPrototypeCustomerConsultation(
                           out CustomerConsultationRecord consultation)
                    ? consultation
                    : null;
            }
        }

        public bool ConsultationCompleted => CurrentConsultation != null;

        public CustomPcRequestRecord CurrentCustomPcRequest
        {
            get
            {
                GarageStockFlowSession session = stockFlow != null
                    ? stockFlow.EnsureInitialized()
                    : null;
                return session != null &&
                       session.TryGetPrototypeCustomPcRequest(
                           out CustomPcRequestRecord request)
                    ? request
                    : null;
            }
        }

        public CustomPcQuoteRecord CurrentCustomPcQuote
        {
            get
            {
                GarageStockFlowSession session = stockFlow != null
                    ? stockFlow.EnsureInitialized()
                    : null;
                return session != null &&
                       session.TryGetPrototypeCustomPcQuote(
                           out CustomPcQuoteRecord quote)
                    ? quote
                    : null;
            }
        }

        public bool CustomPcRequestAccepted => CurrentCustomPcRequest != null;

        public bool CustomPcQuoteReady => CurrentCustomPcQuote != null;

        public bool CanConsultCurrentCustomer
        {
            get
            {
                CustomerVisitRecord visit = CurrentVisit;
                return visit != null &&
                       visit.State == CustomerVisitState.Browsing &&
                       !ConsultationCompleted &&
                       CustomerVisible &&
                       playerMotor != null &&
                       !playerMotor.IsPaused &&
                       HasCustomerFocus();
            }
        }

        public bool CanProgressCurrentCustomPc
        {
            get
            {
                CustomerVisitRecord visit = CurrentVisit;
                return visit != null &&
                       visit.State == CustomerVisitState.Browsing &&
                       ConsultationCompleted &&
                       !CustomPcQuoteReady &&
                       CustomerVisible &&
                       playerMotor != null &&
                       !playerMotor.IsPaused &&
                       HasCustomerFocus();
            }
        }

        public string LastConsultationFailureCode
        {
            get
            {
                CustomerVisitRecord visit = CurrentVisit;
                return visit != null &&
                       visit.State == CustomerVisitState.Browsing &&
                       visit.Id == _lastConsultationVisitId
                    ? _lastConsultationFailureCode
                    : string.Empty;
            }
        }

        public string LastCustomPcFailureCode
        {
            get
            {
                CustomerVisitRecord visit = CurrentVisit;
                return visit != null &&
                       visit.State == CustomerVisitState.Browsing &&
                       visit.Id == _lastCustomPcVisitId
                    ? _lastCustomPcFailureCode
                    : string.Empty;
            }
        }

        public string ContextualPromptText
        {
            get
            {
                CustomerVisitRecord visit = CurrentVisit;
                if (visit == null || visit.State != CustomerVisitState.Browsing)
                {
                    return string.Empty;
                }

                string failure = LastConsultationFailureCode;
                if (!string.IsNullOrEmpty(failure))
                {
                    return $"GÖRÜŞME ENGELLİ • {failure}";
                }

                if (!ConsultationCompleted)
                {
                    return CanConsultCurrentCustomer
                        ? $"{(playerInput != null ? playerInput.InteractBindingPrompt : "E / A")}: " +
                          "müşterinin ihtiyacını sor"
                        : string.Empty;
                }

                string customPcFailure = LastCustomPcFailureCode;
                if (!string.IsNullOrEmpty(customPcFailure))
                {
                    string failureText =
                        $"ÖZEL PC İŞLEMİ ENGELLİ • {customPcFailure}";
                    return CanProgressCurrentCustomPc
                        ? $"{failureText} • " +
                          $"{(playerInput != null ? playerInput.InteractBindingPrompt : "E / A")}: " +
                          "tekrar dene"
                        : failureText;
                }

                if (CustomPcQuoteReady || !CanProgressCurrentCustomPc)
                {
                    return string.Empty;
                }

                string action = CustomPcRequestAccepted
                    ? "10 parçayı ayır ve teklifi hazırla"
                    : "özel oyun PC'si talebini kabul et";
                return $"{(playerInput != null ? playerInput.InteractBindingPrompt : "E / A")}: " +
                       action;
            }
        }

        public string CustomerSpeechTextValue
        {
            get
            {
                CustomerVisitRecord visit = CurrentVisit;
                if (visit == null)
                {
                    return "MÜŞTERİ 001";
                }

                if (visit.State != CustomerVisitState.Browsing)
                {
                    return "MÜŞTERİ 001";
                }

                if (ConsultationCompleted)
                {
                    CustomPcQuoteRecord quote = CurrentCustomPcQuote;
                    if (quote != null)
                    {
                        return "MÜŞTERİ 001\nÖZEL OYUN PC'Sİ TEKLİFİ HAZIR\n" +
                               $"{quote.ReservedSerializedItemCount} PARÇA • " +
                               GarageStockFlowRuntime.FormatPrice(quote.TotalPrice);
                    }

                    return CustomPcRequestAccepted
                        ? "MÜŞTERİ 001\n\"EKRAN KARTIMI YÜKSELTMEK İSTİYORUM\nVE TAM BİR OYUN PC'Sİ TOPLAYALIM\"\nTALEP ALINDI • TEKLİF BEKLİYOR"
                        : "MÜŞTERİ 001\n\"EKRAN KARTIMI YÜKSELTMEK İSTİYORUM\nVE TAM BİR OYUN PC'Sİ TOPLAYALIM\"";
                }

                return CanConsultCurrentCustomer
                    ? $"MÜŞTERİ 001\n{(playerInput != null ? playerInput.InteractBindingPrompt : "E / A")}: İHTİYACI SOR"
                    : "MÜŞTERİ 001\nYARDIM BEKLİYOR";
            }
        }

        public CustomerOfferDecision CurrentOfferDecision
        {
            get
            {
                GarageStockFlowSession session = stockFlow != null
                    ? stockFlow.EnsureInitialized()
                    : null;
                if (session == null ||
                    !session.TryGetPrototypeCustomerVisit(out CustomerVisitRecord visit) ||
                    visit.State != CustomerVisitState.Browsing)
                {
                    _displayedOfferDecision = null;
                    _displayedDecisionVisitId = default;
                    return null;
                }

                if (CustomPcRequestAccepted)
                {
                    _displayedOfferDecision = null;
                    _displayedDecisionVisitId = default;
                    return null;
                }

                if (_displayedOfferDecision != null &&
                    _displayedDecisionVisitId == visit.Id)
                {
                    return _displayedOfferDecision;
                }

                OperationResult<CustomerOfferDecision> result =
                    session.EvaluatePrototypeCustomerOffer();
                if (!result.TryGetValue(out CustomerOfferDecision decision))
                {
                    _displayedOfferDecision = null;
                    _displayedDecisionVisitId = default;
                    return null;
                }

                _displayedOfferDecision = decision;
                _displayedDecisionVisitId = visit.Id;
                return _displayedOfferDecision;
            }
        }

        public string LastOfferActionFailureCode
        {
            get
            {
                CustomerVisitRecord visit = CurrentVisit;
                return visit != null &&
                       visit.State == CustomerVisitState.Browsing &&
                       visit.Id == _lastOfferActionVisitId
                    ? _lastOfferActionFailureCode
                    : string.Empty;
            }
        }

        public bool BuyActionSucceeded
        {
            get
            {
                GarageStockFlowSession session = stockFlow != null
                    ? stockFlow.EnsureInitialized()
                    : null;
                return session != null &&
                       session.TryGetPrototypeCustomerBuyAction(out _);
            }
        }

        public bool LeaveActionSucceeded
        {
            get
            {
                GarageStockFlowSession session = stockFlow != null
                    ? stockFlow.EnsureInitialized()
                    : null;
                return session != null &&
                       session.TryGetPrototypeCustomerLeaveAction(out _);
            }
        }

        public string OfferActionStatusText
        {
            get
            {
                string failureCode = LastOfferActionFailureCode;
                if (!string.IsNullOrEmpty(failureCode))
                {
                    return CurrentOfferDecision?.DecisionKind ==
                           CustomerOfferDecisionKind.Leave
                        ? $"AYRILMA ENGELLİ • {failureCode}"
                        : $"SATIN ALMA ENGELLİ • {failureCode}";
                }

                if (BuyActionSucceeded)
                {
                    return "SATIN ALMA ONAYLANDI • REZERVASYON KİLİTLİ";
                }

                return LeaveActionSucceeded
                    ? "AYRILMA ONAYLANDI • TEKLİF REDDEDİLDİ"
                    : string.Empty;
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
                        if (!ConsultationCompleted)
                        {
                            return CanConsultCurrentCustomer
                                ? "RAF A'DA YARDIM BEKLİYOR • KONUŞMAYA HAZIR"
                                : "RAF A'DA YARDIM BEKLİYOR";
                        }

                        string decisionText = OfferDecisionText;
                        string baseState = string.IsNullOrEmpty(decisionText)
                            ? "İHTİYAÇ KAYDEDİLDİ • KARAR HAZIRLANIYOR"
                            : $"İHTİYAÇ: EKRAN KARTI YÜKSELTMESİ • {decisionText}";
                        CustomPcQuoteRecord quote = CurrentCustomPcQuote;
                        if (quote != null)
                        {
                            return $"{baseState} • ÖZEL PC TEKLİFİ HAZIR • " +
                                   $"{quote.ReservedSerializedItemCount} PARÇA AYRILDI • " +
                                   GarageStockFlowRuntime.FormatPrice(quote.TotalPrice);
                        }

                        return CustomPcRequestAccepted
                            ? $"{baseState} • ÖZEL PC TALEBİ KAYITLI • TEKLİF BEKLİYOR"
                            : $"{baseState} • ÖZEL PC TALEBİ BEKLİYOR";
                    case CustomerVisitState.NavigatingToCheckout:
                        return "KASAYA İLERLİYOR";
                    case CustomerVisitState.AwaitingCheckout:
                        return "KASADA BEKLİYOR";
                    case CustomerVisitState.Exiting:
                        if (visit.ExitReason == CustomerVisitExitReason.Fulfilled)
                        {
                            return "SATIŞ TAMAM • ÇIKIYOR";
                        }

                        return visit.ExitReason == CustomerVisitExitReason.OfferDeclined
                            ? "TEKLİF REDDEDİLDİ • ÇIKIYOR"
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
                string actionStatus = OfferActionStatusText;
                string consultationFailure = LastConsultationFailureCode;
                string customPcFailure = LastCustomPcFailureCode;
                string status = string.IsNullOrEmpty(reasonCode)
                    ? $"MÜŞTERİ AKIŞI: {StateText}\n{route}"
                    : $"MÜŞTERİ AKIŞI: {StateText}\n{route}\n{reasonCode}";
                if (!string.IsNullOrEmpty(consultationFailure))
                {
                    status = $"{status}\nGÖRÜŞME ENGELLİ • {consultationFailure}";
                }

                if (!string.IsNullOrEmpty(customPcFailure))
                {
                    status = $"{status}\nÖZEL PC İŞLEMİ ENGELLİ • {customPcFailure}";
                }

                return string.IsNullOrEmpty(actionStatus)
                    ? status
                    : $"{status}\n{actionStatus}";
            }
        }

        public OperationResult TryConsultCurrentCustomer()
        {
            CustomerVisitRecord visit = CurrentVisit;
            if (visit == null || visit.State != CustomerVisitState.Browsing ||
                ConsultationCompleted || !CanConsultCurrentCustomer)
            {
                return RecordConsultationResult(
                    OperationResult.Fail(
                        GarageCustomerConsultationFailures.FocusRequired));
            }

            GarageStockFlowSession session = stockFlow != null
                ? stockFlow.EnsureInitialized()
                : null;
            OperationResult result = session != null
                ? session.ConsultPrototypeCustomer(CurrentConsultationTime)
                : OperationResult.Fail(CustomerConsultationFailures.InputInvalid);
            return RecordConsultationResult(result);
        }

        public OperationResult TryProgressCurrentCustomPc()
        {
            CustomerVisitRecord visit = CurrentVisit;
            if (visit == null ||
                visit.State != CustomerVisitState.Browsing ||
                !ConsultationCompleted ||
                CustomPcQuoteReady ||
                !CanProgressCurrentCustomPc)
            {
                return RecordCustomPcResult(
                    OperationResult.Fail(
                        GarageCustomerCustomPcFailures.FocusRequired));
            }

            GarageStockFlowSession session = stockFlow != null
                ? stockFlow.EnsureInitialized()
                : null;
            OperationResult result = session == null
                ? OperationResult.Fail(CustomPcQuoteFailures.InputInvalid)
                : CustomPcRequestAccepted
                    ? session.CreatePrototypeCustomPcQuote(CurrentConsultationTime)
                    : session.AcceptPrototypeCustomPcRequest(CurrentConsultationTime);
            return RecordCustomPcResult(result);
        }

        public void ProcessInputFrame()
        {
            if (playerInput == null || playerMotor == null || playerMotor.IsPaused ||
                playerInput.PausePressedThisFrame ||
                (!CanConsultCurrentCustomer && !CanProgressCurrentCustomPc) ||
                !playerInput.TryConsumeInteractPressThisFrame())
            {
                return;
            }

            if (CanConsultCurrentCustomer)
            {
                TryConsultCurrentCustomer();
                return;
            }

            TryProgressCurrentCustomPc();
        }

        public void RecordOfferActionResult(OperationResult result)
        {
            CustomerVisitRecord visit = CurrentVisit;
            bool showFailure = result.IsFailure &&
                               visit != null &&
                               visit.State == CustomerVisitState.Browsing;
            _lastOfferActionFailureCode = showFailure ? result.Error.Code : string.Empty;
            _lastOfferActionVisitId = showFailure ? visit.Id : default;
            RefreshPresentation();
        }

        public void Configure(
            GarageStockFlowRuntime garageStockFlow,
            FirstPersonMotor motor,
            PlayerInputAdapter inputAdapter,
            Camera camera,
            NavMeshSurface surface,
            NavMeshAgent agent,
            GameObject visualRoot,
            TextMesh statusText,
            TextMesh speechText,
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
            playerInput = inputAdapter != null
                ? inputAdapter
                : throw new ArgumentNullException(nameof(inputAdapter));
            playerCamera = camera != null
                ? camera
                : throw new ArgumentNullException(nameof(camera));
            navigationSurface = surface != null
                ? surface
                : throw new ArgumentNullException(nameof(surface));
            customerAgent = agent != null
                ? agent
                : throw new ArgumentNullException(nameof(agent));
            customerVisualRoot = visualRoot != null
                ? visualRoot
                : throw new ArgumentNullException(nameof(visualRoot));
            customerStatusText = statusText != null
                ? statusText
                : throw new ArgumentNullException(nameof(statusText));
            customerSpeechText = speechText != null
                ? speechText
                : throw new ArgumentNullException(nameof(speechText));
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
                                   HasCompletePath(_browsePoint, _exitPoint) &&
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

            if (customerSpeechText != null)
            {
                customerSpeechText.text = CustomerSpeechTextValue;
                FaceSpeechToPlayer();
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

        private void Update()
        {
            ProcessInputFrame();
            RefreshPresentation();
        }

        private void FixedUpdate()
        {
            ProcessFixedStep(Time.frameCount);
        }

        internal void ProcessFixedStep(int renderedFrame)
        {
            bool paused = playerMotor != null && playerMotor.IsPaused;
            if (paused)
            {
                ClearDeferredBrowsingDeadline();
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
            if (!session.TryGetPrototypeCustomerVisit(out CustomerVisitRecord currentVisit))
            {
                LogConfigurationFailureOnce("customer-flow.visit-missing-after-drive");
                return;
            }

            bool customPcServiceOwnsVisit =
                currentVisit.State == CustomerVisitState.Browsing &&
                CustomPcRequestAccepted;
            if (customPcServiceOwnsVisit)
            {
                ClearDeferredBrowsingDeadline();
                RefreshPresentation();
                return;
            }

            if (ShouldDeferBrowsingDeadline(
                    currentVisit,
                    CurrentSimulationTime,
                    renderedFrame))
            {
                RefreshPresentation();
                return;
            }

            ClearDeferredBrowsingDeadline();
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

        private bool ShouldDeferBrowsingDeadline(
            CustomerVisitRecord visit,
            SimulationTimestamp now,
            int renderedFrame)
        {
            if (visit == null ||
                visit.State != CustomerVisitState.Browsing ||
                !now.IsAtOrAfter(visit.StateDeadline))
            {
                ClearDeferredBrowsingDeadline();
                return false;
            }

            bool exactDeadline = _hasDeferredBrowsingDeadline &&
                                 _deferredDeadlineVisitId == visit.Id &&
                                 _deferredDeadlineState == visit.State &&
                                 _deferredStateDeadline == visit.StateDeadline;
            if (!exactDeadline)
            {
                _hasDeferredBrowsingDeadline = true;
                _deferredDeadlineVisitId = visit.Id;
                _deferredDeadlineState = visit.State;
                _deferredStateDeadline = visit.StateDeadline;
                _deferredAtRenderedFrame = renderedFrame;
                return true;
            }

            return _deferredAtRenderedFrame == renderedFrame;
        }

        private void ClearDeferredBrowsingDeadline()
        {
            _hasDeferredBrowsingDeadline = false;
            _deferredDeadlineVisitId = default;
            _deferredDeadlineState = default;
            _deferredStateDeadline = default;
            _deferredAtRenderedFrame = 0;
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
                    if (session.TryGetPrototypeCheckoutSettlement(out _))
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
                   playerInput != null &&
                   playerCamera != null &&
                   navigationSurface != null &&
                   customerAgent != null &&
                   customerVisualRoot != null &&
                   customerStatusText != null &&
                   customerSpeechText != null &&
                   entranceWaypoint != null &&
                   browseWaypoint != null &&
                   checkoutWaypoint != null &&
                   exitWaypoint != null;
        }

        private void FaceSpeechToPlayer()
        {
            if (customerSpeechText == null || playerCamera == null)
            {
                return;
            }

            Vector3 towardCamera =
                playerCamera.transform.position - customerSpeechText.transform.position;
            towardCamera.y = 0f;
            if (towardCamera.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            customerSpeechText.transform.rotation = Quaternion.LookRotation(
                -towardCamera.normalized,
                Vector3.up);
        }

        private OperationResult RecordConsultationResult(OperationResult result)
        {
            CustomerVisitRecord visit = CurrentVisit;
            bool showFailure = result.IsFailure &&
                               visit != null &&
                               visit.State == CustomerVisitState.Browsing;
            _lastConsultationFailureCode = showFailure
                ? result.Error.Code
                : string.Empty;
            _lastConsultationVisitId = showFailure ? visit.Id : default;
            if (result.IsSuccess)
            {
                _displayedOfferDecision = null;
                _displayedDecisionVisitId = default;
            }

            stockFlow?.RefreshPresentation();
            RefreshPresentation();
            return result;
        }

        private OperationResult RecordCustomPcResult(OperationResult result)
        {
            CustomerVisitRecord visit = CurrentVisit;
            bool showFailure = result.IsFailure &&
                               visit != null &&
                               visit.State == CustomerVisitState.Browsing;
            _lastCustomPcFailureCode = showFailure
                ? result.Error.Code
                : string.Empty;
            _lastCustomPcVisitId = showFailure ? visit.Id : default;
            stockFlow?.RefreshPresentation();
            RefreshPresentation();
            return result;
        }

        private bool HasCustomerFocus()
        {
            if (playerCamera == null || customerVisualRoot == null || !CustomerVisible)
            {
                return false;
            }

            Vector3 target = customerVisualRoot.transform.position +
                             (Vector3.up * CustomerFocusHeight);
            Vector3 toTarget = target - playerCamera.transform.position;
            float distance = toTarget.magnitude;
            if (distance <= Mathf.Epsilon || distance > ConsultationRange)
            {
                return false;
            }

            Vector3 direction = toTarget / distance;
            float minimumFocusDot = Mathf.Cos(ConsultationFocusDegrees * Mathf.Deg2Rad);
            if (Vector3.Dot(playerCamera.transform.forward, direction) < minimumFocusDot)
            {
                return false;
            }

            if (!Physics.Raycast(
                    playerCamera.transform.position,
                    direction,
                    out RaycastHit hit,
                    distance + 0.15f,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Collide))
            {
                return false;
            }

            Transform hitTransform = hit.transform;
            Transform customerTransform = customerVisualRoot.transform;
            return hitTransform == customerTransform ||
                   hitTransform.IsChildOf(customerTransform);
        }

        private SimulationTimestamp ResolveCurrentCommandTime(
            bool requireStrictlyAfterVisit)
        {
            SimulationTimestamp current = CurrentSimulationTime;
            CustomerVisitRecord visit = CurrentVisit;
            if (visit == null)
            {
                return current;
            }

            bool currentIsValid = current.IsAtOrAfter(visit.LastUpdatedAt) &&
                                  (!requireStrictlyAfterVisit ||
                                   current != visit.LastUpdatedAt);
            if (currentIsValid)
            {
                return current;
            }

            SimulationTimestamp baseline = visit.LastUpdatedAt;
            if (baseline.Tick == long.MaxValue ||
                baseline.ElapsedMilliseconds > long.MaxValue - FixedStepMilliseconds)
            {
                return current;
            }

            return SimulationTimestamp.Create(
                baseline.Tick + 1,
                baseline.ElapsedMilliseconds + FixedStepMilliseconds);
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
                case CustomerVisitExitReason.OfferDeclined:
                    return "TEKLİF REDDEDİLDİ";
                default:
                    return "SONUÇ BEKLENİYOR";
            }
        }
    }

    public static class GarageCustomerConsultationFailures
    {
        public static readonly Failure FocusRequired =
            Failure.FromCode("presentation.customer-consultation.focus-required");
    }

    public static class GarageCustomerCustomPcFailures
    {
        public static readonly Failure FocusRequired =
            Failure.FromCode("presentation.customer-custom-pc.focus-required");
    }
}
