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

        public string ProjectionIdValue => PrototypeProjectionIdValue;

        public GarageStockFlowRuntime Runtime => stockFlow;

        public TextMesh StatusText => statusText;

        public Renderer StatusIndicator => statusIndicator;

        public Material ReadyMaterial => readyMaterial;

        public Material BlockedMaterial => blockedMaterial;

        public bool IsReady { get; private set; }

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
            if (!IsConfigured)
            {
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
                ApplyBlockedPresentation(
                    "AUTHORITY HENÜZ HAZIR DEĞİL",
                    ElectricalReadinessWorkbenchFailures.RuntimeNotReady);
                return OperationResult.Fail(
                    ElectricalReadinessWorkbenchFailures.RuntimeNotReady);
            }

            OperationResult<ElectricalReadinessSnapshot> readiness =
                session.AssemblyBuild.EvaluateElectricalReadiness();
            CaptureAuthorityState(session);
            if (readiness.IsFailure)
            {
                ApplyBlockedPresentation(
                    ResolveBlockerMessage(readiness.Error),
                    readiness.Error);
                return OperationResult.Fail(readiness.Error);
            }

            IsReady = true;
            CurrentFailureCode = string.Empty;
            statusText.text =
                "ELEKTRİK HAZIR\n" +
                "10/10 PARÇA • 3/3 KABLO\n" +
                "GÜÇ TESTİ BEKLİYOR";
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
                       session.AssemblyBuild.PcieGpuPowerCableRevision;
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
            _hasObservedAuthorityState = true;
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

            return failure == ElectricalReadinessFailures.InvariantInvalid
                ? "MONTAJ SOYUNU DOĞRULA"
                : $"KONTROL GEREKİYOR • {failure.Code}";
        }
    }
}
