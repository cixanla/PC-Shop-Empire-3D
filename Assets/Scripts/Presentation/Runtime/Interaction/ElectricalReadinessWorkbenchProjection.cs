using System;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public static class ElectricalReadinessWorkbenchFailures
    {
        public static readonly Failure ConfigurationMissing = Failure.FromCode(
            "electrical-readiness-workbench.configuration-missing");
        public static readonly Failure RuntimeNotReady = Failure.FromCode(
            "electrical-readiness-workbench.runtime-not-ready");
    }

    /// <summary>
    /// Presentation-only view of the canonical assembly authority's electrical-readiness
    /// decision. It owns no input, power state, receipt, inventory or assembly mutation.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(70)]
    public sealed class ElectricalReadinessWorkbenchProjection : MonoBehaviour
    {
        public const string PrototypeProjectionIdValue =
            "world.assembly-workbench.electrical-readiness.garage-001";

        [SerializeField] private GarageStockFlowRuntime stockFlow;
        [SerializeField] private TextMesh statusText;
        [SerializeField] private Renderer statusIndicator;
        [SerializeField] private Material readyMaterial;
        [SerializeField] private Material blockedMaterial;

        private bool _hasObservedAuthorityState;
        private long _observedInventoryRevision;
        private long _observedAssemblyRevision;
        private long _observedAtx24PowerCableRevision;
        private long _observedEps12vPowerCableRevision;
        private long _observedPcieGpuPowerCableRevision;
        private long _observedPowerTestAttemptRevision;
        private long _observedPowerStateRevision = -1;
        private long _observedPostStartupRevision = -1;

        public string ProjectionIdValue => PrototypeProjectionIdValue;

        public GarageStockFlowRuntime Runtime => stockFlow;

        public TextMesh StatusText => statusText;

        public Renderer StatusIndicator => statusIndicator;

        public Material ReadyMaterial => readyMaterial;

        public Material BlockedMaterial => blockedMaterial;

        public bool IsReady { get; private set; }

        public bool HasPowerBudgetAssessment { get; private set; }

        public int SystemPowerDrawWatts { get; private set; }

        public int MinimumRecommendedPsuWatts { get; private set; }

        public int InstalledPsuWatts { get; private set; }

        public int CapacityMarginWatts { get; private set; }

        public bool HasAcceptedPreflight { get; private set; }

        public bool HasCurrentAcceptedPreflight { get; private set; }

        public bool HasCurrentPostStartupPass { get; private set; }

        public PcPowerState PowerState { get; private set; }

        public bool IsEnergized => PowerState == PcPowerState.Energized;

        public string CurrentFailureCode { get; private set; } =
            ElectricalReadinessWorkbenchFailures.ConfigurationMissing.Code;

        public bool IsConfigured => stockFlow != null &&
                                    statusText != null &&
                                    statusIndicator != null &&
                                    readyMaterial != null &&
                                    blockedMaterial != null;

        public void Configure(
            GarageStockFlowRuntime garageStockFlow,
            TextMesh worldStatusText,
            Renderer indicator,
            Material electricalReadyMaterial,
            Material electricalBlockedMaterial)
        {
            stockFlow = garageStockFlow != null
                ? garageStockFlow
                : throw new ArgumentNullException(nameof(garageStockFlow));
            statusText = worldStatusText != null
                ? worldStatusText
                : throw new ArgumentNullException(nameof(worldStatusText));
            statusIndicator = indicator != null
                ? indicator
                : throw new ArgumentNullException(nameof(indicator));
            readyMaterial = electricalReadyMaterial != null
                ? electricalReadyMaterial
                : throw new ArgumentNullException(nameof(electricalReadyMaterial));
            blockedMaterial = electricalBlockedMaterial != null
                ? electricalBlockedMaterial
                : throw new ArgumentNullException(nameof(electricalBlockedMaterial));
            RefreshPresentation();
        }

        public OperationResult RefreshPresentation()
        {
            ClearPowerBudgetAssessment();
            if (!IsConfigured)
            {
                PowerState = PcPowerState.Off;
                ApplyBlockedPresentation(
                    "AUTHORITY BAĞLANTISI EKSİK",
                    ElectricalReadinessWorkbenchFailures.ConfigurationMissing);
                return OperationResult.Fail(
                    ElectricalReadinessWorkbenchFailures.ConfigurationMissing);
            }

            if (!stockFlow.TryGetInitializedSession(
                    out GarageStockFlowSession session))
            {
                _hasObservedAuthorityState = false;
                PowerState = PcPowerState.Off;
                ApplyBlockedPresentation(
                    "AUTHORITY HENÜZ HAZIR DEĞİL",
                    ElectricalReadinessWorkbenchFailures.RuntimeNotReady);
                return OperationResult.Fail(
                    ElectricalReadinessWorkbenchFailures.RuntimeNotReady);
            }

            ObservePowerState(session);

            if (session.PowerBudget == null)
            {
                CaptureAuthorityState(session);
                ApplyBlockedPresentation(
                    "GÜÇ BÜTÇESİ BAĞLANTISI EKSİK",
                    PcPowerBudgetFailures.ConfigurationMissing);
                return OperationResult.Fail(
                    PcPowerBudgetFailures.ConfigurationMissing);
            }

            OperationResult<PcPowerBudgetSnapshot> assessment =
                session.PowerBudget.AssessPowerBudget();
            CaptureAuthorityState(session);
            if (assessment.IsFailure)
            {
                ApplyBlockedPresentation(
                    ResolveBlockerMessage(assessment.Error),
                    assessment.Error);
                return OperationResult.Fail(assessment.Error);
            }

            CapturePowerBudgetAssessment(assessment.Value);
            if (!assessment.Value.IsSufficient)
            {
                ApplyBlockedPresentation(
                    $"PSU YETERSİZ • {assessment.Value.InstalledPsuWatts}W / " +
                    $"EN AZ {assessment.Value.MinimumRecommendedPsuWatts}W",
                    assessment.Value.Blocker);
                return OperationResult.Fail(assessment.Value.Blocker);
            }

            if (!session.TryGetPowerTestAttempts(
                    out PowerTestAttemptAuthority attempts))
            {
                IsReady = true;
                CurrentFailureCode = string.Empty;
                statusText.text =
                    "GÜÇ BÜTÇESİ UYGUN\n" +
                    $"{assessment.Value.SystemPowerDrawWatts}W / " +
                    $"EN AZ {assessment.Value.MinimumRecommendedPsuWatts}W / " +
                    $"PSU {assessment.Value.InstalledPsuWatts}W\n" +
                    "GÜÇ TESTİ BEKLİYOR";
                statusText.color = new Color(0.68f, 1f, 0.76f);
                statusIndicator.sharedMaterial = readyMaterial;
                CaptureAuthorityState(session);
                return OperationResult.Success();
            }

            IsReady = true;
            HasAcceptedPreflight = attempts.HasCompletedPreflight;
            if (!HasAcceptedPreflight)
            {
                CurrentFailureCode = string.Empty;
                statusText.text =
                    "GÜÇ BÜTÇESİ UYGUN\n" +
                    $"{assessment.Value.SystemPowerDrawWatts}W / " +
                    $"EN AZ {assessment.Value.MinimumRecommendedPsuWatts}W / " +
                    $"PSU {assessment.Value.InstalledPsuWatts}W\n" +
                    "GÜÇ TESTİ BEKLİYOR";
                statusText.color = new Color(0.68f, 1f, 0.76f);
                statusIndicator.sharedMaterial = readyMaterial;
                return OperationResult.Success();
            }

            OperationResult<PowerTestAttemptReceipt> currentReceipt =
                attempts.EvaluateCurrentReceipt();
            if (currentReceipt.IsFailure)
            {
                HasCurrentAcceptedPreflight = false;
                CurrentFailureCode = currentReceipt.Error.Code;
                statusText.text =
                    "ÖN KONTROL GEÇERSİZ\n" +
                    ResolveBlockerMessage(currentReceipt.Error) + "\n" +
                    "POWER-ON BEKLİYOR";
                statusText.color = new Color(1f, 0.78f, 0.50f);
                statusIndicator.sharedMaterial = blockedMaterial;
                return OperationResult.Fail(currentReceipt.Error);
            }

            HasCurrentAcceptedPreflight = true;
            CurrentFailureCode = string.Empty;
            PcPowerStateAuthority powerState =
                session.TryGetPowerState(out PcPowerStateAuthority existing)
                    ? existing
                    : null;
            if (powerState != null &&
                powerState.ValidateReceiptHistory().IsFailure)
            {
                ApplyBlockedPresentation(
                    "POWER AUTHORITY DOĞRULANAMADI",
                    PcPowerStateFailures.ReceiptHistoryInvalid);
                return OperationResult.Fail(
                    PcPowerStateFailures.ReceiptHistoryInvalid);
            }

            PowerState = powerState?.State ?? PcPowerState.Off;
            CaptureAuthorityState(session);
            if (powerState?.IsEnergized == true)
            {
                OperationResult<PcPostStartupReceipt> postStartup =
                    powerState.EvaluateCurrentStartupSelfTest();
                HasCurrentPostStartupPass = postStartup.IsSuccess;
                statusText.text = HasCurrentPostStartupPass
                    ? "GÜÇ AÇIK • POST GEÇTİ\n" +
                      $"{assessment.Value.SystemPowerDrawWatts}W / " +
                      $"EN AZ {assessment.Value.MinimumRecommendedPsuWatts}W / " +
                      $"PSU {assessment.Value.InstalledPsuWatts}W\n" +
                      "FIRMWARE BEKLİYOR • BAKIM KİLİDİ AKTİF"
                    : "GÜÇ AÇIK • POST BEKLİYOR\n" +
                      $"{assessment.Value.SystemPowerDrawWatts}W / " +
                      $"EN AZ {assessment.Value.MinimumRecommendedPsuWatts}W / " +
                      $"PSU {assessment.Value.InstalledPsuWatts}W\n" +
                      "BAKIM KİLİDİ AKTİF";
                statusText.color = new Color(1f, 0.90f, 0.42f);
                statusIndicator.sharedMaterial = readyMaterial;
                return OperationResult.Success();
            }

            statusText.text =
                "ÖN KONTROL GEÇTİ\n" +
                $"{assessment.Value.SystemPowerDrawWatts}W / " +
                $"EN AZ {assessment.Value.MinimumRecommendedPsuWatts}W / " +
                $"PSU {assessment.Value.InstalledPsuWatts}W\n" +
                "POWER-ON BEKLİYOR";
            statusText.color = new Color(0.68f, 1f, 0.76f);
            statusIndicator.sharedMaterial = readyMaterial;
            return OperationResult.Success();
        }

        private void Awake()
        {
            RefreshPresentation();
        }

        private void Update()
        {
            if (!IsConfigured)
            {
                if (CurrentFailureCode !=
                    ElectricalReadinessWorkbenchFailures.ConfigurationMissing.Code)
                {
                    RefreshPresentation();
                }

                return;
            }

            if (!stockFlow.TryGetInitializedSession(
                    out GarageStockFlowSession session))
            {
                if (_hasObservedAuthorityState ||
                    CurrentFailureCode !=
                    ElectricalReadinessWorkbenchFailures.RuntimeNotReady.Code)
                {
                    RefreshPresentation();
                }

                return;
            }

            if (AuthorityStateChanged(session))
            {
                RefreshPresentation();
            }
        }

        private bool AuthorityStateChanged(GarageStockFlowSession session)
        {
            return !_hasObservedAuthorityState ||
                   _observedInventoryRevision != session.Inventory.Revision ||
                   _observedAssemblyRevision != session.AssemblyBuild.Revision ||
                   _observedAtx24PowerCableRevision !=
                       session.AssemblyBuild.Atx24PowerCableRevision ||
                   _observedEps12vPowerCableRevision !=
                       session.AssemblyBuild.Eps12vPowerCableRevision ||
                   _observedPcieGpuPowerCableRevision !=
                       session.AssemblyBuild.PcieGpuPowerCableRevision ||
                   _observedPowerTestAttemptRevision !=
                       (session.TryGetPowerTestAttempts(
                           out PowerTestAttemptAuthority attempts)
                               ? attempts.Revision
                               : -1L) ||
                   _observedPowerStateRevision != ResolvePowerStateRevision(session) ||
                   _observedPostStartupRevision !=
                       ResolvePostStartupRevision(session);
        }

        private void CaptureAuthorityState(GarageStockFlowSession session)
        {
            _observedInventoryRevision = session.Inventory.Revision;
            _observedAssemblyRevision = session.AssemblyBuild.Revision;
            _observedAtx24PowerCableRevision =
                session.AssemblyBuild.Atx24PowerCableRevision;
            _observedEps12vPowerCableRevision =
                session.AssemblyBuild.Eps12vPowerCableRevision;
            _observedPcieGpuPowerCableRevision =
                session.AssemblyBuild.PcieGpuPowerCableRevision;
            _observedPowerTestAttemptRevision =
                session.TryGetPowerTestAttempts(
                    out PowerTestAttemptAuthority attempts)
                        ? attempts.Revision
                        : -1L;
            _observedPowerStateRevision = ResolvePowerStateRevision(session);
            _observedPostStartupRevision = ResolvePostStartupRevision(session);
            _hasObservedAuthorityState = true;
        }

        private void ClearPowerBudgetAssessment()
        {
            HasPowerBudgetAssessment = false;
            SystemPowerDrawWatts = 0;
            MinimumRecommendedPsuWatts = 0;
            InstalledPsuWatts = 0;
            CapacityMarginWatts = 0;
            HasAcceptedPreflight = false;
            HasCurrentAcceptedPreflight = false;
            HasCurrentPostStartupPass = false;
        }

        private void ObservePowerState(GarageStockFlowSession session)
        {
            PowerState = session != null &&
                         session.TryGetPowerState(
                             out PcPowerStateAuthority powerState)
                ? powerState.State
                : PcPowerState.Off;
        }

        private void CapturePowerBudgetAssessment(PcPowerBudgetSnapshot assessment)
        {
            HasPowerBudgetAssessment = true;
            SystemPowerDrawWatts = assessment.SystemPowerDrawWatts;
            MinimumRecommendedPsuWatts = assessment.MinimumRecommendedPsuWatts;
            InstalledPsuWatts = assessment.InstalledPsuWatts;
            CapacityMarginWatts = assessment.CapacityMarginWatts;
        }

        private void ApplyBlockedPresentation(string blocker, Failure failure)
        {
            IsReady = false;
            CurrentFailureCode = failure.Code;
            if (statusText != null)
            {
                statusText.text =
                    "ELEKTRİK KONTROLÜ\n" +
                    blocker + "\n" +
                    "GÜÇ HAZIR DEĞİL";
                statusText.color = new Color(1f, 0.78f, 0.50f);
            }

            if (statusIndicator != null && blockedMaterial != null)
            {
                statusIndicator.sharedMaterial = blockedMaterial;
            }
        }

        private static string ResolveBlockerMessage(Failure failure)
        {
            if (failure == AssemblyFailures.MotherboardMissing)
            {
                return "ANAKART EKSİK";
            }

            if (failure == AssemblyFailures.MotherboardUnsecured)
            {
                return "ANAKARTI SABİTLE";
            }

            if (failure == ElectricalReadinessFailures.ConfigurationUnsupported)
            {
                return "KASA TOPOLOJİSİ UYUMSUZ";
            }

            if (failure == AssemblyFailures.ProcessorMissing)
            {
                return "İŞLEMCİ EKSİK";
            }

            if (failure == AssemblyFailures.ProcessorUnretained)
            {
                return "İŞLEMCİYİ KİLİTLE";
            }

            if (failure == AssemblyFailures.MemoryMissing)
            {
                return "DDR5 BELLEK EKSİK";
            }

            if (failure == AssemblyFailures.MemoryUnretained)
            {
                return "DDR5 MANDALLARINI KİLİTLE";
            }

            if (failure == AssemblyFailures.StorageMissing)
            {
                return "M.2 DEPOLAMA EKSİK";
            }

            if (failure == AssemblyFailures.StorageUnsecured)
            {
                return "M.2 VİDASINI SABİTLE";
            }

            if (failure == AssemblyFailures.ProcessorCoolerMissing)
            {
                return "CPU SOĞUTUCU EKSİK";
            }

            if (failure == AssemblyFailures.ProcessorCoolerUnretained)
            {
                return "TERMAL MACUN / SOĞUTUCUYU TAMAMLA";
            }

            if (failure == AssemblyFailures.GraphicsCardMissing)
            {
                return "EKRAN KARTI EKSİK";
            }

            if (failure == AssemblyFailures.GraphicsCardUnretained)
            {
                return "EKRAN KARTINI SABİTLE";
            }

            if (failure == AssemblyFailures.PowerSupplyMissing)
            {
                return "GÜÇ KAYNAĞI EKSİK";
            }

            if (failure == AssemblyFailures.PowerSupplyUnretained)
            {
                return "GÜÇ KAYNAĞINI SABİTLE";
            }

            if (failure == ElectricalReadinessFailures.Atx24PowerCableMissing)
            {
                return "ATX24 KABLOSUNU BAĞLA";
            }

            if (failure == ElectricalReadinessFailures.Eps12vPowerCableMissing)
            {
                return "EPS12V KABLOSUNU BAĞLA";
            }

            if (failure == ElectricalReadinessFailures.PcieGpuPowerCableMissing)
            {
                return "PCIe GPU 6+2 KABLOSUNU BAĞLA";
            }

            if (failure == PcPowerBudgetFailures.ConfigurationMissing)
            {
                return "GÜÇ BÜTÇESİ BAĞLANTISI EKSİK";
            }

            if (failure == PcPowerBudgetFailures.CatalogMismatch)
            {
                return "GÜÇ KATALOĞU UYUMSUZ";
            }

            if (failure == PcPowerBudgetFailures.ElectricalProfileMissing)
            {
                return "PARÇA GÜÇ PROFİLİ EKSİK";
            }

            if (failure == PcPowerBudgetFailures.ElectricalProfileKindMismatch)
            {
                return "PARÇA GÜÇ PROFİLİ UYUMSUZ";
            }

            if (failure == PcPowerBudgetFailures.PolicyInvalid ||
                failure == PcPowerBudgetFailures.SystemPowerDrawInvalid ||
                failure == PcPowerBudgetFailures.ArithmeticOverflow)
            {
                return "GÜÇ HESABI DOĞRULANAMADI";
            }

            if (failure == PowerTestAttemptFailures.ContextStale)
            {
                return "YAPILANDIRMA DEĞİŞTİ";
            }

            return failure == ElectricalReadinessFailures.InvariantInvalid
                ? "MONTAJ SOYUNU DOĞRULA"
                : $"KONTROL GEREKİYOR • {failure.Code}";
        }

        private static long ResolvePowerStateRevision(
            GarageStockFlowSession session)
        {
            return session != null &&
                   session.TryGetPowerState(out PcPowerStateAuthority powerState)
                ? powerState.Revision
                : -1L;
        }

        private static long ResolvePostStartupRevision(
            GarageStockFlowSession session)
        {
            return session != null &&
                   session.TryGetPowerState(out PcPowerStateAuthority powerState)
                ? powerState.PostStartupRevision
                : -1L;
        }
    }
}
