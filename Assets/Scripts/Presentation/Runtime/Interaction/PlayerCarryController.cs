using System;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Presentation.Input;
using PCShopEmpire3D.Presentation.Player;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(75)]
    public sealed partial class PlayerCarryController : MonoBehaviour
    {
        private static readonly Vector3 MotherboardSeatPreviewSize =
            new Vector3(0.244f, 0.244f, 0.012f);
        private static readonly Vector3 ProcessorSeatPreviewSize =
            new Vector3(0.045f, 0.0375f, 0.004f);
        private static readonly Vector3 DimmSeatPreviewSize =
            new Vector3(0.132f, 0.032f, 0.008f);

        [SerializeField] private PlayerInputAdapter input;
        [SerializeField] private FirstPersonMotor motor;
        [SerializeField] private PhysicalInteractionResolver resolver;
        [SerializeField] private Transform carryAnchor;
        [SerializeField] private VisibleHandsPresenter hands;
        [SerializeField] private PlacementPreview placementPreview;
        [SerializeField] private MotherboardSeatProjection motherboardSeat;
        [SerializeField] private MotherboardFastenerProjection motherboardFastener;
        [SerializeField] private MotherboardAssemblyItemBinding motherboardAssemblyBinding;
        [SerializeField] private ProcessorSocketProjection processorSocket;
        [SerializeField] private ProcessorAssemblyItemBinding processorAssemblyBinding;
        [SerializeField] private DimmSlotProjection dimmSlot;
        [SerializeField] private DimmAssemblyItemBinding dimmAssemblyBinding;
        [SerializeField] private LayerMask supportMask;
        [SerializeField] private LayerMask stackSupportMask;
        [SerializeField] private LayerMask obstructionMask;
        [SerializeField] private int heldItemLayer;

        private bool _applicationQuitting;
        private string _heldItemId = string.Empty;
        private string _activeCartId = string.Empty;
        private int _placementRotationQuarterTurns;

        public PhysicalItemProjection FocusedItem { get; private set; }

        public PhysicalItemProjection HeldItem { get; private set; }

        public TransportCartProjection FocusedCart { get; private set; }

        public TransportCartProjection ActiveCart { get; private set; }

        public string LastFailureCode { get; private set; } = string.Empty;

        public bool IsCarrying => HeldItem != null;

        public bool IsDrivingCart => ActiveCart != null && ActiveCart.IsDriven;

        public bool IsPlacementMode { get; private set; }

        public bool IsMotherboardSeatMode { get; private set; }

        public bool IsProcessorSeatMode { get; private set; }

        public bool IsDimmSeatMode { get; private set; }

        public bool PlacementValid { get; private set; }

        public PlacementStatus CurrentPlacementStatus { get; private set; } = PlacementStatus.ContextMissing;

        public MotherboardSeatStatus CurrentMotherboardSeatStatus { get; private set; } =
            MotherboardSeatStatus.ContextMissing;

        public MotherboardFastenerStatus CurrentMotherboardFastenerStatus { get; private set; } =
            MotherboardFastenerStatus.ContextMissing;

        public ProcessorSocketStatus CurrentProcessorSocketStatus { get; private set; } =
            ProcessorSocketStatus.ContextMissing;

        public DimmSlotStatus CurrentDimmSlotStatus { get; private set; } =
            DimmSlotStatus.ContextMissing;

        public bool IsMotherboardFastenerFocused { get; private set; }

        public bool HasMotherboardFastenerContext { get; private set; }

        public bool IsProcessorSocketFocused { get; private set; }

        public bool HasProcessorSocketContext { get; private set; }

        public bool IsDimmSlotFocused { get; private set; }

        public bool HasDimmSlotContext { get; private set; }

        public bool HasAssemblyPromptOwnership =>
            IsAtx24PowerCableRouteMode ||
            IsMotherboardSeatMode ||
            HasMotherboardFastenerContext ||
            IsProcessorSeatMode ||
            HasProcessorSocketContext ||
            IsDimmSeatMode ||
            HasDimmSlotContext ||
            IsPowerSupplySeatMode ||
            HasPowerSupplyBayContext ||
            IsGraphicsCardSeatMode ||
            HasGraphicsCardSlotContext ||
            IsProcessorCoolerSeatMode ||
            HasProcessorCoolerSlotContext ||
            IsM2StorageSeatMode ||
            HasM2StorageSlotContext ||
            (HeldItem != null &&
             (GetAtx24PowerCableBinding(HeldItem) != null ||
              GetMotherboardBinding(HeldItem) != null ||
              GetProcessorBinding(HeldItem) != null ||
              GetDimmBinding(HeldItem) != null ||
              GetPowerSupplyBinding(HeldItem) != null ||
              GetGraphicsCardBinding(HeldItem) != null ||
              GetProcessorCoolerBinding(HeldItem) != null ||
              GetM2StorageBinding(HeldItem) != null));

        public PlacementPreview PlacementPreview => placementPreview;

        public PhysicalItemProjection CurrentStackSupport { get; private set; }

        public int PlacementRotationQuarterTurns => _placementRotationQuarterTurns;

        public float PlacementRotationDegrees => _placementRotationQuarterTurns * 90f;

        public string PromptText
        {
            get
            {
                if (HeldItem != null)
                {
                    string stockState = GetStockStateSuffix(HeldItem);
                    string placement = input != null
                        ? input.PrimaryBindingPrompt
                        : "Mouse Left / RT";
                    string drop = input != null ? input.DropBindingPrompt : "G / B";
                    string rotate = input != null
                        ? input.RotatePlacementBindingPrompt
                        : "R / Right Shoulder";
                    MotherboardAssemblyItemBinding motherboardBinding =
                        GetMotherboardBinding(HeldItem);
                    ProcessorAssemblyItemBinding processorBinding =
                        GetProcessorBinding(HeldItem);
                    DimmAssemblyItemBinding dimmBinding =
                        GetDimmBinding(HeldItem);
                    PowerSupplyAssemblyItemBinding powerSupplyAssemblyBinding =
                        GetPowerSupplyBinding(HeldItem);
                    GraphicsCardAssemblyItemBinding graphicsCardAssemblyBinding =
                        GetGraphicsCardBinding(HeldItem);
                    ProcessorCoolerAssemblyItemBinding coolerBinding =
                        GetProcessorCoolerBinding(HeldItem);
                    M2StorageAssemblyItemBinding storageBinding =
                        GetM2StorageBinding(HeldItem);
                    Atx24PowerCableAssemblyItemBinding cableBinding =
                        GetAtx24PowerCableBinding(HeldItem);
                    if (cableBinding != null)
                    {
                        return GetHeldAtx24PowerCablePrompt(
                            cableBinding,
                            placement,
                            drop,
                            rotate);
                    }
                    if (powerSupplyAssemblyBinding != null)
                    {
                        return GetHeldPowerSupplyPrompt(
                            powerSupplyAssemblyBinding,
                            placement,
                            drop,
                            rotate);
                    }
                    if (graphicsCardAssemblyBinding != null)
                    {
                        return GetHeldGraphicsCardPrompt(
                            graphicsCardAssemblyBinding,
                            placement,
                            drop,
                            rotate);
                    }
                    if (coolerBinding != null)
                    {
                        return GetHeldProcessorCoolerPrompt(
                            coolerBinding, placement, drop, rotate);
                    }
                    if (storageBinding != null)
                    {
                        return GetHeldM2StoragePrompt(
                            storageBinding,
                            placement,
                            drop,
                            rotate);
                    }

                    if (dimmBinding != null)
                    {
                        if (!IsDimmSeatMode)
                        {
                            return $"{placement}: A2 bellek slotuna hizala • " +
                                   $"{drop}: güvenli bırak • ANAHTARLI DDR5";
                        }

                        string state = GetDimmSlotStatusLabel(CurrentDimmSlotStatus);
                        return PlacementValid
                            ? $"[OK] DDR5 ÇENTİĞİ HİZALI • {drop}: oturt • " +
                              $"{rotate}: döndür • {placement}: çık"
                            : $"[X] {state} • {rotate}: döndür • {placement}: çık";
                    }

                    if (processorBinding != null)
                    {
                        if (!IsProcessorSeatMode)
                        {
                            return $"{placement}: işlemci soketine hizala • " +
                                   $"{drop}: güvenli bırak • HASSAS PARÇA";
                        }

                        string state = GetProcessorSocketStatusLabel(
                            CurrentProcessorSocketStatus);
                        return PlacementValid
                            ? $"[OK] CPU ANAHTARI HİZALI • {drop}: oturt • " +
                              $"{rotate}: döndür • {placement}: çık"
                            : $"[X] {state} • {rotate}: döndür • {placement}: çık";
                    }

                    if (motherboardBinding != null)
                    {
                        if (!IsMotherboardSeatMode)
                        {
                            return $"{placement}: kasaya hizala • " +
                                   $"{drop}: güvenli bırak • HASSAS PARÇA";
                        }

                        string state = GetMotherboardSeatStatusLabel(
                            CurrentMotherboardSeatStatus);
                        return PlacementValid
                            ? $"[OK] HİZALI • {drop}: oturt • {rotate}: döndür • " +
                              $"{placement}: çık"
                            : $"[X] {state} • {rotate}: döndür • {placement}: çık";
                    }

                    if (HeldItem.CarryProfile == PhysicalCarryProfile.LargeBox)
                    {
                        string load = FocusedCart != null && FocusedCart.CanLoad(HeldItem)
                            ? $"{(input != null ? input.InteractBindingPrompt : "E / A")}: " +
                              $"{FocusedCart.DisplayName} üzerine yükle   |   "
                            : string.Empty;
                        string blocked = LastFailureCode.StartsWith(
                            "drop.",
                            StringComparison.Ordinal)
                            ? "   |   BIRAKMA ENGELLİ"
                            : string.Empty;
                        return load +
                               $"{drop}: {HeldItem.DisplayName} güvenli bırak   |   " +
                               $"AĞIR YÜK — sprint kapalı{blocked}{stockState}";
                    }

                    return IsPlacementMode
                        ? $"{drop}: yerleştir   |   {rotate}: 90° döndür " +
                          $"[{PlacementRotationDegrees:0}°]   |   {placement}: iptal   |   " +
                          (PlacementValid
                              ? (CurrentStackSupport != null ? "İSTİF GEÇERLİ" : "GEÇERLİ")
                              : "ENGELLİ") + stockState
                        : $"{placement}: yerleştirme önizlemesi   |   " +
                          $"{drop}: güvenli bırak{stockState}";
                }

                if (ActiveCart != null)
                {
                    string cargo = ActiveCart.HasCargo
                        ? $"YÜKLÜ: {ActiveCart.Cargo.DisplayName}"
                        : "BOŞ";
                    return $"{(input != null ? input.PrimaryBindingPrompt : "Mouse Left / RT")}: " +
                           $"arabayı bırak   |   {cargo}   |   sprint kapalı";
                }

                if (FocusedCart != null)
                {
                    string unload = FocusedCart.HasCargo
                        ? $"{(input != null ? input.InteractBindingPrompt : "E / A")}: " +
                          $"{FocusedCart.Cargo.DisplayName} yükünü al   |   "
                        : string.Empty;
                    string blocked = LastFailureCode.StartsWith(
                        "cart.",
                        StringComparison.Ordinal)
                        ? "   |   ARABA ENGELLİ"
                        : string.Empty;
                    return unload +
                           $"{(input != null ? input.PrimaryBindingPrompt : "Mouse Left / RT")}: " +
                           $"{FocusedCart.DisplayName} tut{blocked}";
                }

                if (HasPowerSupplyBayContext && powerSupplyBinding != null)
                {
                    return GetPowerSupplyBayPrompt();
                }

                if (HasGraphicsCardSlotContext && graphicsCardBinding != null)
                {
                    return GetGraphicsCardSlotPrompt();
                }

                if (HasProcessorCoolerSlotContext && processorCoolerBinding != null)
                {
                    return GetProcessorCoolerSlotPrompt();
                }

                if (HasM2StorageSlotContext && m2StorageAssemblyBinding != null)
                {
                    return GetM2StorageSlotPrompt();
                }

                if (HasDimmSlotContext &&
                    !IsDimmSlotFocused &&
                    dimmAssemblyBinding != null)
                {
                    return CurrentDimmSlotStatus switch
                    {
                        DimmSlotStatus.LineOfSightBlocked =>
                            "[X] A2 BELLEK SLOTU ENGELLİ • görüş hattını aç",
                        DimmSlotStatus.Obstructed =>
                            "[X] A2 BELLEK SLOTU ENGELLİ • önünü aç",
                        _ => "[X] A2 BELLEK SLOTU KULLANILAMIYOR"
                    };
                }

                if (IsDimmSlotFocused && dimmAssemblyBinding != null)
                {
                    string primary = input != null
                        ? input.PrimaryBindingPrompt
                        : "Mouse Left / RT";
                    string interact = input != null
                        ? input.InteractBindingPrompt
                        : "E / A";
                    if (CurrentDimmSlotStatus ==
                        DimmSlotStatus.ValidSeatedOpenRetentionBlocked)
                    {
                        return $"[AÇIK] DDR5 OTURDU • ANAKART SABİT DEĞİL • " +
                               $"{interact}: belleği çıkar";
                    }

                    return dimmAssemblyBinding.IsRetained
                        ? $"[KİLİTLİ] ÇİFT MANDAL KAPALI • {primary}: mandalları aç • " +
                          $"{interact}: çıkarma kilitli"
                        : $"[AÇIK] DDR5 OTURDU • {primary}: çift mandalı kapat • " +
                          $"{interact}: belleği çıkar";
                }

                if (HasProcessorSocketContext &&
                    !IsProcessorSocketFocused &&
                    processorAssemblyBinding != null)
                {
                    return CurrentProcessorSocketStatus switch
                    {
                        ProcessorSocketStatus.LineOfSightBlocked =>
                            "[X] CPU SOKETİ ENGELLİ • görüş hattını aç",
                        ProcessorSocketStatus.Obstructed =>
                            "[X] CPU SOKETİ ENGELLİ • önünü aç",
                        _ => "[X] CPU SOKETİ KULLANILAMIYOR"
                    };
                }

                if (IsProcessorSocketFocused && processorAssemblyBinding != null)
                {
                    string primary = input != null
                        ? input.PrimaryBindingPrompt
                        : "Mouse Left / RT";
                    string interact = input != null
                        ? input.InteractBindingPrompt
                        : "E / A";
                    if (CurrentProcessorSocketStatus ==
                        ProcessorSocketStatus.ValidSeatedOpenRetentionBlocked)
                    {
                        return $"[AÇIK] CPU OTURDU • ANAKART SABİT DEĞİL • " +
                               $"{interact}: işlemciyi çıkar";
                    }

                    return processorAssemblyBinding.IsRetained
                        ? $"[KİLİTLİ] RETENTION KAPALI • {primary}: kolu aç • " +
                          $"{interact}: çıkarma kilitli"
                        : $"[AÇIK] CPU OTURDU • {primary}: kolu kapat • " +
                          $"{interact}: işlemciyi çıkar";
                }

                if (HasMotherboardFastenerContext &&
                    !IsMotherboardFastenerFocused &&
                    motherboardAssemblyBinding != null)
                {
                    return CurrentMotherboardFastenerStatus switch
                    {
                        MotherboardFastenerStatus.LineOfSightBlocked =>
                            "[X] VİDA ENGELLİ • görüş hattını aç",
                        MotherboardFastenerStatus.Obstructed =>
                            "[X] VİDA ENGELLİ • önünü aç",
                        _ => "[X] VİDA KULLANILAMIYOR"
                    };
                }

                if (FocusedItem == null)
                {
                    if (!IsMotherboardFastenerFocused || motherboardAssemblyBinding == null)
                    {
                        return string.Empty;
                    }
                }

                if (IsMotherboardFastenerFocused && motherboardAssemblyBinding != null)
                {
                    string primary = input != null
                        ? input.PrimaryBindingPrompt
                        : "Mouse Left / RT";
                    string interact = input != null
                        ? input.InteractBindingPrompt
                        : "E / A";
                    return motherboardAssemblyBinding.IsSecured
                        ? $"[OK] VIDA SIKILI • {primary}: gevşet • " +
                          $"{interact}: sökme kilitli"
                        : $"[O] VIDA GEVŞEK • {primary}: sık • " +
                          $"{interact}: anakartı sök";
                }

                MotherboardAssemblyItemBinding focusedMotherboard =
                    GetMotherboardBinding(FocusedItem);
                ProcessorAssemblyItemBinding focusedProcessor =
                    GetProcessorBinding(FocusedItem);
                DimmAssemblyItemBinding focusedDimm = GetDimmBinding(FocusedItem);
                PowerSupplyAssemblyItemBinding focusedPowerSupply =
                    GetPowerSupplyBinding(FocusedItem);
                GraphicsCardAssemblyItemBinding focusedGraphicsCard =
                    GetGraphicsCardBinding(FocusedItem);
                ProcessorCoolerAssemblyItemBinding focusedCooler =
                    GetProcessorCoolerBinding(FocusedItem);
                M2StorageAssemblyItemBinding focusedStorage =
                    GetM2StorageBinding(FocusedItem);
                Atx24PowerCableAssemblyItemBinding focusedCable =
                    GetAtx24PowerCableBinding(FocusedItem);
                if (focusedCable != null)
                {
                    return GetFocusedAtx24PowerCablePrompt(focusedCable);
                }
                if (focusedPowerSupply != null)
                {
                    return focusedPowerSupply.IsRetained
                        ? "PSU ARKA PLAKA + 4 VİDA KİLİTLİ • sökme kilitli"
                        : focusedPowerSupply.IsSeated
                            ? $"{(input != null ? input.InteractBindingPrompt : "E / A")}: PSU'yu çıkar • 4 VİDA GEVŞEK"
                            : $"{(input != null ? input.InteractBindingPrompt : "E / A")}: {FocusedItem.DisplayName} al • ATX PS/2";
                }
                if (focusedGraphicsCard != null)
                {
                    return focusedGraphicsCard.IsRetained
                        ? "PCIe MANDALI + ARKA BRAKET KİLİTLİ • sökme kilitli"
                        : focusedGraphicsCard.IsSeated
                            ? $"{(input != null ? input.InteractBindingPrompt : "E / A")}: ekran kartını çıkar • BRAKET GEVŞEK"
                            : $"{(input != null ? input.InteractBindingPrompt : "E / A")}: {FocusedItem.DisplayName} al • PCIe x16";
                }
                if (focusedCooler != null)
                {
                    return focusedCooler.IsRetained
                        ? "SOĞUTUCU BRAKETİ KİLİTLİ • sökme kilitli"
                        : focusedCooler.IsSeated
                            ? $"{(input != null ? input.InteractBindingPrompt : "E / A")}: soğutucuyu çıkar • 4 NOKTA GEVŞEK"
                            : $"{(input != null ? input.InteractBindingPrompt : "E / A")}: {FocusedItem.DisplayName} al • TOP-DOWN AIR";
                }
                if (focusedStorage != null)
                {
                    string interact = input != null
                        ? input.InteractBindingPrompt
                        : "E / A";
                    return focusedStorage.IsSecured
                        ? "M.2 VİDASI SIKILI • yuvayı hedefleyip gevşet"
                        : focusedStorage.IsSeated
                            ? $"{interact}: SSD'yi çıkar • M.2 VİDASI GEVŞEK"
                            : $"{interact}: {FocusedItem.DisplayName} al • M-KEY NVMe";
                }

                if (focusedDimm != null)
                {
                    string interact = input != null
                        ? input.InteractBindingPrompt
                        : "E / A";
                    return focusedDimm.IsRetained
                        ? "ÇİFT MANDAL KAPALI • slotu hedefleyip aç"
                        : focusedDimm.IsSeated
                            ? $"{interact}: belleği çıkar • ÇİFT MANDAL AÇIK"
                            : $"{interact}: {FocusedItem.DisplayName} al • DDR5";
                }

                if (focusedProcessor != null)
                {
                    string interact = input != null
                        ? input.InteractBindingPrompt
                        : "E / A";
                    return focusedProcessor.IsRetained
                        ? "RETENTION KAPALI • kolu hedefleyip aç"
                        : focusedProcessor.IsSeated
                            ? $"{interact}: işlemciyi çıkar • RETENTION AÇIK"
                            : $"{interact}: {FocusedItem.DisplayName} al • HASSAS PARÇA";
                }

                if (focusedMotherboard != null)
                {
                    string interact = input != null
                        ? input.InteractBindingPrompt
                        : "E / A";
                    return focusedMotherboard.IsSecured
                        ? $"VIDA SIKILI • vidayı hedefle ve " +
                          $"{(input != null ? input.PrimaryBindingPrompt : "Mouse Left / RT")}: gevşet"
                        : focusedMotherboard.IsSeated
                            ? $"{interact}: anakartı sök • OTURDU, VİDALANMADI"
                        : $"{interact}: {FocusedItem.DisplayName} al • HASSAS PARÇA";
                }

                InventoryItemWorldBinding binding = GetInventoryBinding(FocusedItem);
                if (binding != null && binding.RequiresAcceptance)
                {
                    return $"{(input != null ? input.InteractBindingPrompt : "E / A")}: " +
                           $"{FocusedItem.DisplayName} teslimatını kabul et   |   " +
                           binding.LocationLabel;
                }

                if (binding != null && binding.RequiresUnpacking)
                {
                    return $"{(input != null ? input.InteractBindingPrompt : "E / A")}: " +
                           $"{FocusedItem.DisplayName} kolisini aç   |   " +
                           binding.LocationLabel;
                }

                if (binding != null && binding.RequiresShelfOffer)
                {
                    return $"{(input != null ? input.InteractBindingPrompt : "E / A")}: " +
                           $"RAF A fiyatını yayınla • " +
                           $"{GarageStockFlowRuntime.PrototypePriceText}   |   " +
                           binding.LocationLabel;
                }

                if (binding != null && binding.IsCustomerReserved)
                {
                    if (binding.RequiresCheckoutCompletion)
                    {
                        return "KASA İSTASYONUNA GİT   |   " +
                               $"KASA: {binding.Runtime.CheckoutStatusText}   |   " +
                               "REZERVASYON KİLİTLİ";
                    }

                    if (binding.IsCustomerReservationActionOwned)
                    {
                        return "KASA İSTASYONUNA GİT   |   SATIN ALMA ONAYLANDI   |   " +
                               "REZERVASYON KİLİTLİ";
                    }

                    return $"{(input != null ? input.DropBindingPrompt : "G / B")}: " +
                           "müşteri rezervasyonunu kaldır   |   MÜŞTERİ İÇİN AYRILDI";
                }

                if (binding != null && binding.RequiresCustomerReservation)
                {
                    string actionFailure = LastFailureCode.StartsWith(
                        "retail.offer-action.",
                        StringComparison.Ordinal)
                        ? $"   |   SATIN ALMA ENGELLİ: {LastFailureCode}"
                        : string.Empty;
                    return $"{(input != null ? input.InteractBindingPrompt : "E / A")}: " +
                           $"{FocusedItem.DisplayName} al   |   " +
                           $"{(input != null ? input.DropBindingPrompt : "G / B")}: " +
                           "müşterinin satın almasını onayla   |   " +
                           binding.LocationLabel + actionFailure;
                }

                if (binding != null && binding.RequiresCustomerDeparture)
                {
                    string actionFailure = LastFailureCode.StartsWith(
                        "retail.offer-action.",
                        StringComparison.Ordinal)
                        ? $"   |   AYRILMA ENGELLİ: {LastFailureCode}"
                        : string.Empty;
                    return $"{(input != null ? input.InteractBindingPrompt : "E / A")}: " +
                           $"{FocusedItem.DisplayName} al   |   " +
                           $"{(input != null ? input.DropBindingPrompt : "G / B")}: " +
                           "müşteriyi teklifi reddederek uğurla   |   " +
                           binding.LocationLabel + actionFailure;
                }

                return FocusedItem.HasStackedItem
                    ? $"{FocusedItem.DisplayName}: önce üst kutuyu al"
                    : $"{(input != null ? input.InteractBindingPrompt : "E / A")}: " +
                      $"{FocusedItem.DisplayName} al{GetStockStateSuffix(FocusedItem)}";
            }
        }

        public void Configure(
            PlayerInputAdapter inputAdapter,
            FirstPersonMotor playerMotor,
            PhysicalInteractionResolver interactionResolver,
            Transform itemCarryAnchor,
            VisibleHandsPresenter handsPresenter,
            PlacementPreview preview,
            LayerMask groundLayers,
            LayerMask stackingLayers,
            LayerMask blockingLayers,
            int heldLayer)
        {
            input = inputAdapter != null ? inputAdapter : throw new ArgumentNullException(nameof(inputAdapter));
            motor = playerMotor != null ? playerMotor : throw new ArgumentNullException(nameof(playerMotor));
            resolver = interactionResolver != null
                ? interactionResolver
                : throw new ArgumentNullException(nameof(interactionResolver));
            carryAnchor = itemCarryAnchor != null
                ? itemCarryAnchor
                : throw new ArgumentNullException(nameof(itemCarryAnchor));
            hands = handsPresenter != null
                ? handsPresenter
                : throw new ArgumentNullException(nameof(handsPresenter));
            placementPreview = preview != null ? preview : throw new ArgumentNullException(nameof(preview));
            supportMask = groundLayers;
            stackSupportMask = stackingLayers;
            obstructionMask = blockingLayers;
            heldItemLayer = heldLayer;
        }

        public void ConfigureMotherboardSeat(MotherboardSeatProjection seatProjection)
        {
            motherboardSeat = seatProjection != null
                ? seatProjection
                : throw new ArgumentNullException(nameof(seatProjection));
        }

        public void ConfigureMotherboardFastener(
            MotherboardFastenerProjection fastenerProjection,
            MotherboardAssemblyItemBinding assemblyBinding)
        {
            motherboardFastener = fastenerProjection != null
                ? fastenerProjection
                : throw new ArgumentNullException(nameof(fastenerProjection));
            motherboardAssemblyBinding = assemblyBinding != null
                ? assemblyBinding
                : throw new ArgumentNullException(nameof(assemblyBinding));
        }

        public void ConfigureProcessorSocket(
            ProcessorSocketProjection socketProjection,
            ProcessorAssemblyItemBinding assemblyBinding)
        {
            processorSocket = socketProjection != null
                ? socketProjection
                : throw new ArgumentNullException(nameof(socketProjection));
            processorAssemblyBinding = assemblyBinding != null
                ? assemblyBinding
                : throw new ArgumentNullException(nameof(assemblyBinding));
            processorAssemblyBinding.SyncProjectionToAuthority();
        }

        public void ConfigureDimmSlot(
            DimmSlotProjection slotProjection,
            DimmAssemblyItemBinding assemblyBinding)
        {
            if (slotProjection == null)
            {
                throw new ArgumentNullException(nameof(slotProjection));
            }

            if (assemblyBinding == null)
            {
                throw new ArgumentNullException(nameof(assemblyBinding));
            }

            if (assemblyBinding.Slot != slotProjection)
            {
                throw new ArgumentException(
                    "The DIMM binding must own the configured slot.",
                    nameof(assemblyBinding));
            }

            dimmSlot = slotProjection;
            dimmAssemblyBinding = assemblyBinding;
            dimmAssemblyBinding.SyncProjectionToAuthority();
        }

        public bool MatchesDimmConfiguration(
            DimmSlotProjection slotProjection,
            DimmAssemblyItemBinding assemblyBinding)
        {
            return slotProjection != null &&
                   assemblyBinding != null &&
                   dimmSlot == slotProjection &&
                   dimmAssemblyBinding == assemblyBinding &&
                   assemblyBinding.Slot == slotProjection;
        }

        public OperationResult TryOperateMotherboardFastener()
        {
            if (motherboardFastener == null || motherboardAssemblyBinding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-fastener.context-missing")));
            }

            return TryOperateMotherboardFastener(EvaluateMotherboardFastener());
        }

        private OperationResult TryOperateMotherboardFastener(
            MotherboardFastenerEvaluation evaluation)
        {
            ApplyMotherboardFastenerEvaluation(evaluation);
            if (!evaluation.CanOperate)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(evaluation.FailureCode)));
            }

            OperationResult result = motherboardAssemblyBinding.TryOperateFastener();
            if (result.IsSuccess)
            {
                bool isSecured = motherboardAssemblyBinding.IsSecured;
                ApplyMotherboardFastenerEvaluation(
                    new MotherboardFastenerEvaluation(
                        isSecured
                            ? MotherboardFastenerStatus.ValidSecured
                            : MotherboardFastenerStatus.ValidUnsecured,
                        isSecured));
                processorAssemblyBinding?.SyncProjectionToAuthority();
                dimmAssemblyBinding?.SyncProjectionToAuthority();
                m2StorageAssemblyBinding?.SyncProjectionToAuthority();
                processorCoolerBinding?.SyncProjectionToAuthority();
                graphicsCardBinding?.SyncProjectionToAuthority();
                powerSupplyBinding?.SyncProjectionToAuthority();
            }

            return Remember(result);
        }

        public OperationResult TryOperateProcessorRetention()
        {
            if (processorSocket == null || processorAssemblyBinding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-processor.context-missing")));
            }

            return TryOperateProcessorRetention(EvaluateProcessorSocketInteraction());
        }

        private OperationResult TryOperateProcessorRetention(
            ProcessorSocketEvaluation evaluation)
        {
            ApplyProcessorSocketEvaluation(evaluation);
            if (!evaluation.CanOperateRetention)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(evaluation.FailureCode)));
            }

            OperationResult result = processorAssemblyBinding.TryOperateRetention();
            if (result.IsSuccess)
            {
                GarageStockFlowSession session = processorAssemblyBinding.Session;
                ProcessorSocketEvaluation authoritativeEvaluation =
                    processorSocket.ApplyAuthoritativeInteractionFeedback(
                        session.AssemblyBuild.MotherboardSeatState,
                        session.AssemblyBuild.ProcessorSocketState);
                ApplyProcessorSocketEvaluation(authoritativeEvaluation);
            }

            return Remember(result);
        }

        public OperationResult TryOperateDimmRetention()
        {
            if (dimmSlot == null || dimmAssemblyBinding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-memory.context-missing")));
            }

            return TryOperateDimmRetention(EvaluateDimmSlotInteraction());
        }

        private OperationResult TryOperateDimmRetention(DimmSlotEvaluation evaluation)
        {
            ApplyDimmSlotEvaluation(evaluation);
            if (!evaluation.CanOperateRetention)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(evaluation.FailureCode)));
            }

            OperationResult result = dimmAssemblyBinding.TryOperateRetention();
            if (result.IsSuccess)
            {
                GarageStockFlowSession session = dimmAssemblyBinding.Session;
                DimmSlotEvaluation authoritativeEvaluation =
                    dimmSlot.ApplyAuthoritativeInteractionFeedback(
                        session.AssemblyBuild.MotherboardSeatState,
                        session.AssemblyBuild.MemorySlotState);
                ApplyDimmSlotEvaluation(authoritativeEvaluation);
            }

            return Remember(result);
        }

        public OperationResult TrySetMotherboardSeatMode(bool enabled)
        {
            MotherboardAssemblyItemBinding binding = GetMotherboardBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-seat.nothing-held")));
            }

            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-seat.paused")));
            }

            SetMotherboardSeatMode(enabled);
            if (enabled)
            {
                UpdateMotherboardSeatPreview(binding);
            }

            return Remember(OperationResult.Success());
        }

        public OperationResult TrySetProcessorSeatMode(bool enabled)
        {
            ProcessorAssemblyItemBinding binding = GetProcessorBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-processor.nothing-held")));
            }

            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-processor.paused")));
            }

            SetProcessorSeatMode(enabled);
            if (enabled)
            {
                UpdateProcessorSeatPreview(binding);
            }

            return Remember(OperationResult.Success());
        }

        public OperationResult TrySetDimmSeatMode(bool enabled)
        {
            DimmAssemblyItemBinding binding = GetDimmBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-memory.nothing-held")));
            }

            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-memory.paused")));
            }

            SetDimmSeatMode(enabled);
            if (enabled)
            {
                UpdateDimmSeatPreview(binding);
            }

            return Remember(OperationResult.Success());
        }

        public OperationResult TryRotateProcessorSeatPreviewClockwise()
        {
            ProcessorAssemblyItemBinding binding = GetProcessorBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-processor.nothing-held")));
            }

            if (!IsProcessorSeatMode)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-processor.mode-inactive")));
            }

            _placementRotationQuarterTurns = (_placementRotationQuarterTurns + 1) % 4;
            LastFailureCode = string.Empty;
            UpdateProcessorSeatPreview(binding);
            return OperationResult.Success();
        }

        public OperationResult TryRotateDimmSeatPreviewClockwise()
        {
            DimmAssemblyItemBinding binding = GetDimmBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-memory.nothing-held")));
            }

            if (!IsDimmSeatMode)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-memory.mode-inactive")));
            }

            _placementRotationQuarterTurns = (_placementRotationQuarterTurns + 2) % 4;
            LastFailureCode = string.Empty;
            UpdateDimmSeatPreview(binding);
            return OperationResult.Success();
        }

        public OperationResult TryPickup(PhysicalItemProjection item)
        {
            if (item == null)
            {
                return Remember(OperationResult.Fail(Failure.FromCode("pickup.no-target")));
            }

            if (HeldItem != null)
            {
                return Remember(OperationResult.Fail(Failure.FromCode("pickup.slot-occupied")));
            }

            MotherboardAssemblyItemBinding motherboardBinding =
                GetMotherboardBinding(item);
            ProcessorAssemblyItemBinding processorBinding =
                GetProcessorBinding(item);
            DimmAssemblyItemBinding dimmBinding = GetDimmBinding(item);
            PowerSupplyAssemblyItemBinding powerSupplyAssemblyBinding =
                GetPowerSupplyBinding(item);
            GraphicsCardAssemblyItemBinding graphicsCardAssemblyBinding =
                GetGraphicsCardBinding(item);
            ProcessorCoolerAssemblyItemBinding coolerBinding = GetProcessorCoolerBinding(item);
            M2StorageAssemblyItemBinding storageBinding = GetM2StorageBinding(item);
            Atx24PowerCableAssemblyItemBinding cableBinding =
                GetAtx24PowerCableBinding(item);
            if (cableBinding != null)
            {
                return TryPickupAtx24PowerCable(item, cableBinding);
            }
            if (powerSupplyAssemblyBinding != null)
            {
                return TryPickupPowerSupply(item, powerSupplyAssemblyBinding);
            }
            if (graphicsCardAssemblyBinding != null)
            {
                return TryPickupGraphicsCard(item, graphicsCardAssemblyBinding);
            }
            if (coolerBinding != null)
            {
                return TryPickupProcessorCooler(item, coolerBinding);
            }
            if (storageBinding != null)
            {
                return TryPickupM2Storage(item, storageBinding);
            }

            if (dimmBinding != null)
            {
                return TryPickupDimm(item, dimmBinding);
            }

            if (processorBinding != null)
            {
                return TryPickupProcessor(item, processorBinding);
            }

            if (motherboardBinding != null)
            {
                return TryPickupMotherboard(item, motherboardBinding);
            }

            InventoryItemWorldBinding binding = GetInventoryBinding(item);
            if (binding != null && binding.RequiresAcceptance)
            {
                OperationResult acceptance = binding.TryAcceptDelivery();
                if (acceptance.IsSuccess)
                {
                    LastFailureCode = string.Empty;
                    SetHandsState(VisibleHandsState.TargetFocused);
                }

                return Remember(acceptance);
            }

            if (binding != null && binding.RequiresUnpacking)
            {
                OperationResult unpack = binding.TryOpenParcel();
                if (unpack.IsSuccess)
                {
                    LastFailureCode = string.Empty;
                    SetHandsState(VisibleHandsState.TargetFocused);
                }

                return Remember(unpack);
            }

            if (binding != null && binding.RequiresShelfOffer)
            {
                OperationResult publish = binding.TryPublishShelfOffer();
                if (publish.IsSuccess)
                {
                    LastFailureCode = string.Empty;
                    SetHandsState(VisibleHandsState.TargetFocused);
                }

                return Remember(publish);
            }

            OperationResult authorityTransfer = binding != null
                ? binding.TryPreparePickupTransfer()
                : OperationResult.Success();
            if (authorityTransfer.IsFailure)
            {
                SetHandsState(VisibleHandsState.TargetFocused);
                return Remember(authorityTransfer);
            }

            OperationResult result = item.BeginCarry(carryAnchor, heldItemLayer);
            if (result.IsFailure)
            {
                RollbackAuthorityTransfer(binding);
                return Remember(result);
            }

            if (binding != null)
            {
                OperationResult commit = binding.CommitPreparedTransfer(targetIsWorld: false);
                if (commit.IsFailure)
                {
                    item.RecoverToLastSafePose();
                    RollbackAuthorityTransfer(binding);
                    return Remember(commit);
                }
            }

            HeldItem = item;
            _heldItemId = item.ItemIdValue;
            FocusedItem = null;
            ResetPlacementState();
            LastFailureCode = string.Empty;
            motor.ApplyCarryProfile(item.CarryProfile);
            SetCarryHandsState(blocked: false);
            return result;
        }

        public OperationResult TryLoadHeldItem(TransportCartProjection cart)
        {
            if (HeldItem == null)
            {
                return Remember(OperationResult.Fail(Failure.FromCode("cart.load-nothing-held")));
            }

            if (cart == null)
            {
                return Remember(OperationResult.Fail(Failure.FromCode("cart.load-no-target")));
            }

            PhysicalItemProjection item = HeldItem;
            OperationResult result = cart.TryLoad(item, heldItemLayer);
            if (result.IsFailure)
            {
                SetCarryHandsState(blocked: true);
                return Remember(result);
            }

            HeldItem = null;
            _heldItemId = string.Empty;
            FocusedItem = null;
            FocusedCart = cart;
            ResetPlacementState();
            motor?.ClearCarryProfile();
            LastFailureCode = string.Empty;
            SetHandsState(VisibleHandsState.TargetFocused);
            return result;
        }

        public OperationResult TryUnloadCart(TransportCartProjection cart)
        {
            if (HeldItem != null)
            {
                return Remember(OperationResult.Fail(Failure.FromCode("pickup.slot-occupied")));
            }

            if (cart == null)
            {
                return Remember(OperationResult.Fail(Failure.FromCode("cart.unload-no-target")));
            }

            OperationResult<PhysicalItemProjection> result = cart.TryUnload(
                carryAnchor,
                heldItemLayer);
            if (result.IsFailure)
            {
                return Remember(OperationResult.Fail(result.Error));
            }

            HeldItem = result.Value;
            _heldItemId = HeldItem.ItemIdValue;
            FocusedItem = null;
            FocusedCart = null;
            ResetPlacementState();
            motor?.ApplyCarryProfile(HeldItem.CarryProfile);
            LastFailureCode = string.Empty;
            SetCarryHandsState(blocked: false);
            return OperationResult.Success();
        }

        public OperationResult TryBeginCartDrive(TransportCartProjection cart)
        {
            if (HeldItem != null)
            {
                return Remember(OperationResult.Fail(Failure.FromCode("cart.driver-hands-occupied")));
            }

            if (ActiveCart != null)
            {
                return Remember(OperationResult.Fail(Failure.FromCode("cart.driver-slot-occupied")));
            }

            if (cart == null)
            {
                return Remember(OperationResult.Fail(Failure.FromCode("cart.driver-no-target")));
            }

            OperationResult result = cart.BeginDrive(transform);
            if (result.IsFailure)
            {
                return Remember(result);
            }

            ActiveCart = cart;
            _activeCartId = cart.CartIdValue;
            FocusedCart = cart;
            FocusedItem = null;
            motor?.ApplyTransportCartDriveProfile(cart.MovementSpeedMultiplier);
            LastFailureCode = string.Empty;
            SetHandsState(VisibleHandsState.DrivingTransportCart);
            return result;
        }

        public OperationResult TryEndCartDrive()
        {
            if (ActiveCart == null)
            {
                return Remember(OperationResult.Fail(Failure.FromCode("cart.driver-inactive")));
            }

            TransportCartProjection cart = ActiveCart;
            OperationResult result = cart.EndDrive();
            if (result.IsFailure)
            {
                return Remember(result);
            }

            ActiveCart = null;
            _activeCartId = string.Empty;
            FocusedCart = cart;
            motor?.ClearTransportCartDriveProfile();
            LastFailureCode = string.Empty;
            SetHandsState(VisibleHandsState.TargetFocused);
            return result;
        }

        public OperationResult TryDrop()
        {
            if (HeldItem == null)
            {
                return Remember(OperationResult.Fail(Failure.FromCode("drop.nothing-held")));
            }

            OperationResult<Pose> pose = SafeDropSolver.FindPose(
                transform,
                HeldItem,
                supportMask,
                obstructionMask);
            if (pose.IsFailure)
            {
                SetCarryHandsState(blocked: true);
                return Remember(OperationResult.Fail(pose.Error));
            }

            MotherboardAssemblyItemBinding motherboardBinding =
                GetMotherboardBinding(HeldItem);
            ProcessorAssemblyItemBinding processorBinding =
                GetProcessorBinding(HeldItem);
            DimmAssemblyItemBinding dimmBinding = GetDimmBinding(HeldItem);
            PowerSupplyAssemblyItemBinding powerSupplyAssemblyBinding =
                GetPowerSupplyBinding(HeldItem);
            GraphicsCardAssemblyItemBinding graphicsCardAssemblyBinding =
                GetGraphicsCardBinding(HeldItem);
            ProcessorCoolerAssemblyItemBinding coolerBinding = GetProcessorCoolerBinding(HeldItem);
            M2StorageAssemblyItemBinding storageBinding =
                GetM2StorageBinding(HeldItem);
            Atx24PowerCableAssemblyItemBinding cableBinding =
                GetAtx24PowerCableBinding(HeldItem);
            if (cableBinding != null)
            {
                if (motor != null && motor.IsPaused)
                {
                    return Remember(OperationResult.Fail(
                        Failure.FromCode("assembly-power-cable.paused")));
                }

                OperationResult drop = cableBinding.TryDropToWorld(pose.Value);
                if (drop.IsSuccess)
                {
                    CompleteHeldItemRelease();
                }
                else
                {
                    SetCarryHandsState(blocked: true);
                }

                return Remember(drop);
            }
            if (powerSupplyAssemblyBinding != null)
            {
                if (motor != null && motor.IsPaused)
                {
                    return Remember(OperationResult.Fail(
                        Failure.FromCode("assembly-power-supply.paused")));
                }

                OperationResult drop =
                    powerSupplyAssemblyBinding.TryDropToWorld(pose.Value);
                if (drop.IsSuccess)
                {
                    CompleteHeldItemRelease();
                }
                else
                {
                    SetCarryHandsState(blocked: true);
                }

                return Remember(drop);
            }
            if (graphicsCardAssemblyBinding != null)
            {
                if (motor != null && motor.IsPaused)
                {
                    return Remember(OperationResult.Fail(
                        Failure.FromCode("assembly-graphics-card.paused")));
                }

                OperationResult drop =
                    graphicsCardAssemblyBinding.TryDropToWorld(pose.Value);
                if (drop.IsSuccess)
                {
                    CompleteHeldItemRelease();
                }
                else
                {
                    SetCarryHandsState(blocked: true);
                }

                return Remember(drop);
            }
            if (coolerBinding != null)
            {
                if (motor != null && motor.IsPaused)
                {
                    return Remember(OperationResult.Fail(Failure.FromCode("assembly-cooler.paused")));
                }

                OperationResult drop = coolerBinding.TryDropToWorld(pose.Value);
                if (drop.IsSuccess) CompleteHeldItemRelease(); else SetCarryHandsState(blocked: true);
                return Remember(drop);
            }
            if (storageBinding != null)
            {
                if (motor != null && motor.IsPaused)
                {
                    return Remember(OperationResult.Fail(
                        Failure.FromCode("assembly-storage.paused")));
                }

                OperationResult drop = storageBinding.TryDropToWorld(pose.Value);
                if (drop.IsSuccess)
                {
                    CompleteHeldItemRelease();
                }
                else
                {
                    SetCarryHandsState(blocked: true);
                }

                return Remember(drop);
            }

            if (dimmBinding != null)
            {
                if (motor != null && motor.IsPaused)
                {
                    return Remember(OperationResult.Fail(
                        Failure.FromCode("assembly-memory.paused")));
                }

                OperationResult drop = dimmBinding.TryDropToWorld(pose.Value);
                if (drop.IsSuccess)
                {
                    CompleteHeldItemRelease();
                }
                else
                {
                    SetCarryHandsState(blocked: true);
                }

                return Remember(drop);
            }

            if (processorBinding != null)
            {
                if (motor != null && motor.IsPaused)
                {
                    return Remember(OperationResult.Fail(
                        Failure.FromCode("assembly-processor.paused")));
                }

                OperationResult drop = processorBinding.TryDropToWorld(pose.Value);
                if (drop.IsSuccess)
                {
                    CompleteHeldItemRelease();
                }
                else
                {
                    SetCarryHandsState(blocked: true);
                }

                return Remember(drop);
            }

            if (motherboardBinding != null)
            {
                if (motor != null && motor.IsPaused)
                {
                    return Remember(OperationResult.Fail(
                        Failure.FromCode("assembly-seat.paused")));
                }

                OperationResult drop = motherboardBinding.TryDropToWorld(pose.Value);
                if (drop.IsSuccess)
                {
                    CompleteHeldItemRelease();
                }
                else
                {
                    SetCarryHandsState(blocked: true);
                }

                return Remember(drop);
            }

            return ReleaseHeldItem(pose.Value, stabilizePlacement: false, placementSurface: null);
        }

        public OperationResult TryConfirmMotherboardSeat()
        {
            MotherboardAssemblyItemBinding binding = GetMotherboardBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-seat.nothing-held")));
            }

            if (!IsMotherboardSeatMode)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-seat.mode-inactive")));
            }

            MotherboardSeatEvaluation evaluation = EvaluateMotherboardSeat(binding);
            ApplyMotherboardSeatEvaluation(evaluation);
            if (!evaluation.IsValid)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(evaluation.FailureCode)));
            }

            OperationResult attach = binding.TryAttachAt(evaluation.Pose);
            if (attach.IsSuccess)
            {
                CompleteHeldItemRelease();
            }
            else
            {
                SetCarryHandsState(blocked: true);
            }

            return Remember(attach);
        }

        public OperationResult TryConfirmProcessorSeat()
        {
            ProcessorAssemblyItemBinding binding = GetProcessorBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-processor.nothing-held")));
            }

            if (!IsProcessorSeatMode)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-processor.mode-inactive")));
            }

            ProcessorSocketEvaluation evaluation = EvaluateProcessorSeat(binding);
            return TryConfirmProcessorSeat(binding, evaluation);
        }

        private OperationResult TryConfirmProcessorSeat(
            ProcessorAssemblyItemBinding binding,
            ProcessorSocketEvaluation evaluation)
        {
            ApplyProcessorSeatEvaluation(evaluation);
            if (!evaluation.CanSeat)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(evaluation.FailureCode)));
            }

            OperationResult attach = binding.TryAttachAt(evaluation.Pose);
            if (attach.IsSuccess)
            {
                CompleteHeldItemRelease();
                binding.SyncProjectionToAuthority();
            }
            else
            {
                SetCarryHandsState(blocked: true);
            }

            return Remember(attach);
        }

        public OperationResult TryConfirmDimmSeat()
        {
            DimmAssemblyItemBinding binding = GetDimmBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-memory.nothing-held")));
            }

            if (!IsDimmSeatMode)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-memory.mode-inactive")));
            }

            DimmSlotEvaluation evaluation = EvaluateDimmSeat(binding);
            return TryConfirmDimmSeat(binding, evaluation);
        }

        private OperationResult TryConfirmDimmSeat(
            DimmAssemblyItemBinding binding,
            DimmSlotEvaluation evaluation)
        {
            ApplyDimmSeatEvaluation(evaluation);
            if (!evaluation.CanSeat)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(evaluation.FailureCode)));
            }

            OperationResult attach = binding.TryAttachAt(
                evaluation.Pose,
                evaluation.Orientation);
            if (attach.IsSuccess)
            {
                CompleteHeldItemRelease();
                binding.SyncProjectionToAuthority();
            }
            else
            {
                SetCarryHandsState(blocked: true);
            }

            return Remember(attach);
        }

        public OperationResult TryConfirmPlacement()
        {
            if (HeldItem == null)
            {
                return Remember(OperationResult.Fail(Failure.FromCode("placement.nothing-held")));
            }

            if (!HeldItem.SupportsPlacement)
            {
                return Remember(OperationResult.Fail(Failure.FromCode("placement.profile-unsupported")));
            }

            if (!IsPlacementMode)
            {
                return Remember(OperationResult.Fail(Failure.FromCode("placement.mode-inactive")));
            }

            PlacementEvaluation evaluation = PlacementSolver.Evaluate(
                transform,
                HeldItem,
                supportMask,
                obstructionMask,
                _placementRotationQuarterTurns,
                stackSupportMask);
            ApplyPlacementEvaluation(evaluation);
            if (!evaluation.IsValid)
            {
                return Remember(OperationResult.Fail(Failure.FromCode(evaluation.FailureCode)));
            }

            return ReleaseHeldItem(
                evaluation.Pose,
                stabilizePlacement: true,
                evaluation.StackSupport,
                evaluation.Surface);
        }

        public void ProcessInputFrame()
        {
            if (HeldItem == null && !string.IsNullOrEmpty(_heldItemId))
            {
                LastFailureCode = "carry.projection-missing";
                _heldItemId = string.Empty;
                ResetPlacementState();
                motor?.ClearCarryProfile();
                SetHandsState(VisibleHandsState.Recovering);
                Debug.LogError("CARRY_RECOVERY_FAILED code=carry.projection-missing");
                return;
            }

            if (ActiveCart == null && !string.IsNullOrEmpty(_activeCartId))
            {
                LastFailureCode = "cart.projection-missing";
                _activeCartId = string.Empty;
                motor?.ClearTransportCartDriveProfile();
                SetHandsState(VisibleHandsState.Recovering);
                Debug.LogError("CART_RECOVERY_FAILED code=cart.projection-missing");
                return;
            }

            if (ActiveCart != null && (!ActiveCart.isActiveAndEnabled || !ActiveCart.IsDriven))
            {
                ActiveCart = null;
                _activeCartId = string.Empty;
                FocusedCart = null;
                motor?.ClearTransportCartDriveProfile();
                LastFailureCode = "cart.driver-interrupted";
                SetHandsState(VisibleHandsState.Recovering);
                return;
            }

            if (HeldItem != null && (!HeldItem.isActiveAndEnabled || !HeldItem.IsCarried))
            {
                TryRecoverHeldItem();
                return;
            }

            if (input == null || motor == null || resolver == null)
            {
                placementPreview?.Hide();
                motherboardSeat?.ResetFeedback();
                ResetAtx24PowerCableState();
                ResetPowerSupplyBayFocus();
                ResetGraphicsCardSlotFocus();
                ResetProcessorCoolerSlotFocus();
                ResetM2StorageSlotFocus();
                ResetDimmSlotFocus();
                ResetProcessorSocketFocus();
                ResetMotherboardFastenerFocus();
                return;
            }

            if (motor.IsPaused || input.PausePressedThisFrame)
            {
                input.DrainGameplayPressesThisFrame();
                placementPreview?.Hide();
                motherboardSeat?.ResetFeedback();
                ResetAtx24PowerCableState();
                ResetPowerSupplyBayFocus();
                ResetGraphicsCardSlotFocus();
                ResetProcessorCoolerSlotFocus();
                ResetM2StorageSlotFocus();
                ResetDimmSlotFocus();
                ResetProcessorSocketFocus();
                ResetMotherboardFastenerFocus();
                return;
            }

            if (HeldItem != null)
            {
                ResetPowerSupplyBayFocus();
                ResetGraphicsCardSlotFocus();
                ResetProcessorCoolerSlotFocus();
                ResetM2StorageSlotFocus();
                ResetDimmSlotFocus();
                ResetProcessorSocketFocus();
                ResetMotherboardFastenerFocus();
                FocusedCart = null;
                if (ProcessHeldAtx24PowerCableInput())
                {
                    return;
                }
                if (ProcessHeldPowerSupplyInput())
                {
                    return;
                }
                if (ProcessHeldGraphicsCardInput())
                {
                    return;
                }
                if (ProcessHeldProcessorCoolerInput())
                {
                    return;
                }
                if (ProcessHeldM2StorageInput())
                {
                    return;
                }

                DimmAssemblyItemBinding dimmBinding = GetDimmBinding(HeldItem);
                if (dimmBinding != null)
                {
                    if (input.TryConsumePrimaryActionPressThisFrame())
                    {
                        input.TryConsumeRotatePlacementPressThisFrame();
                        input.TryConsumeInteractPressThisFrame();
                        input.TryConsumeDropPressThisFrame();
                        TrySetDimmSeatMode(!IsDimmSeatMode);
                        return;
                    }

                    if (IsDimmSeatMode &&
                        input.TryConsumeRotatePlacementPressThisFrame())
                    {
                        input.TryConsumeInteractPressThisFrame();
                        input.TryConsumeDropPressThisFrame();
                        TryRotateDimmSeatPreviewClockwise();
                        return;
                    }

                    if (!IsDimmSeatMode)
                    {
                        UpdateDimmSeatPreview(dimmBinding);
                        if (input.TryConsumeDropPressThisFrame())
                        {
                            input.TryConsumePrimaryActionPressThisFrame();
                            input.TryConsumeRotatePlacementPressThisFrame();
                            input.TryConsumeInteractPressThisFrame();
                            TryDrop();
                        }

                        return;
                    }

                    DimmSlotEvaluation dimmSeatEvaluation = EvaluateDimmSeat(dimmBinding);
                    ApplyDimmSeatEvaluation(dimmSeatEvaluation);
                    if (input.TryConsumeDropPressThisFrame())
                    {
                        input.TryConsumePrimaryActionPressThisFrame();
                        input.TryConsumeInteractPressThisFrame();
                        TryConfirmDimmSeat(dimmBinding, dimmSeatEvaluation);
                    }

                    return;
                }

                ProcessorAssemblyItemBinding processorBinding =
                    GetProcessorBinding(HeldItem);
                if (processorBinding != null)
                {
                    if (input.TryConsumePrimaryActionPressThisFrame())
                    {
                        input.TryConsumeRotatePlacementPressThisFrame();
                        input.TryConsumeInteractPressThisFrame();
                        input.TryConsumeDropPressThisFrame();
                        TrySetProcessorSeatMode(!IsProcessorSeatMode);
                        return;
                    }

                    if (IsProcessorSeatMode &&
                        input.TryConsumeRotatePlacementPressThisFrame())
                    {
                        input.TryConsumeInteractPressThisFrame();
                        input.TryConsumeDropPressThisFrame();
                        TryRotateProcessorSeatPreviewClockwise();
                        return;
                    }

                    if (!IsProcessorSeatMode)
                    {
                        UpdateProcessorSeatPreview(processorBinding);
                        if (input.TryConsumeDropPressThisFrame())
                        {
                            input.TryConsumePrimaryActionPressThisFrame();
                            input.TryConsumeRotatePlacementPressThisFrame();
                            input.TryConsumeInteractPressThisFrame();
                            TryDrop();
                        }

                        return;
                    }

                    ProcessorSocketEvaluation processorSeatEvaluation =
                        EvaluateProcessorSeat(processorBinding);
                    ApplyProcessorSeatEvaluation(processorSeatEvaluation);
                    if (input.TryConsumeDropPressThisFrame())
                    {
                        input.TryConsumePrimaryActionPressThisFrame();
                        input.TryConsumeInteractPressThisFrame();
                        TryConfirmProcessorSeat(
                            processorBinding,
                            processorSeatEvaluation);
                    }

                    return;
                }

                MotherboardAssemblyItemBinding motherboardBinding =
                    GetMotherboardBinding(HeldItem);
                if (motherboardBinding != null)
                {
                    if (input.TryConsumePrimaryActionPressThisFrame())
                    {
                        TrySetMotherboardSeatMode(!IsMotherboardSeatMode);
                        input.TryConsumeDropPressThisFrame();
                        UpdateMotherboardSeatPreview(motherboardBinding);
                        return;
                    }

                    if (IsMotherboardSeatMode &&
                        input.TryConsumeRotatePlacementPressThisFrame())
                    {
                        _placementRotationQuarterTurns =
                            (_placementRotationQuarterTurns + 1) % 4;
                        LastFailureCode = string.Empty;
                    }

                    UpdateMotherboardSeatPreview(motherboardBinding);
                    if (input.TryConsumeDropPressThisFrame())
                    {
                        if (IsMotherboardSeatMode)
                        {
                            TryConfirmMotherboardSeat();
                        }
                        else
                        {
                            TryDrop();
                        }
                    }

                    return;
                }

                if (HeldItem.CarryProfile == PhysicalCarryProfile.LargeBox)
                {
                    OperationResult<TransportCartProjection> cartTarget =
                        resolver.ResolveTransportCart();
                    FocusedCart = cartTarget.IsSuccess ? cartTarget.Value : null;
                    if (FocusedCart != null && input.TryConsumeInteractPressThisFrame())
                    {
                        TryLoadHeldItem(FocusedCart);
                        return;
                    }
                }

                if (HeldItem.SupportsPlacement &&
                    input.TryConsumePrimaryActionPressThisFrame())
                {
                    SetPlacementMode(!IsPlacementMode);
                }

                if (IsPlacementMode &&
                    input.TryConsumeRotatePlacementPressThisFrame())
                {
                    _placementRotationQuarterTurns = (_placementRotationQuarterTurns + 1) % 4;
                    LastFailureCode = string.Empty;
                }

                UpdatePlacementPreview();
                if (input.TryConsumeDropPressThisFrame())
                {
                    if (IsPlacementMode)
                    {
                        TryConfirmPlacement();
                    }
                    else
                    {
                        TryDrop();
                    }
                }

                return;
            }

            if (ActiveCart != null)
            {
                ResetAtx24PowerCableState();
                ResetPowerSupplyBayFocus();
                ResetGraphicsCardSlotFocus();
                ResetProcessorCoolerSlotFocus();
                ResetM2StorageSlotFocus();
                ResetDimmSlotFocus();
                ResetProcessorSocketFocus();
                ResetMotherboardFastenerFocus();
                FocusedItem = null;
                FocusedCart = ActiveCart;
                SetHandsState(VisibleHandsState.DrivingTransportCart);
                if (input.TryConsumePrimaryActionPressThisFrame())
                {
                    TryEndCartDrive();
                    return;
                }

                OperationResult motion = ActiveCart.TryFollowDriver(supportMask, obstructionMask);
                if (motion.IsFailure)
                {
                    string failureCode = motion.Error.Code;
                    Debug.LogWarning($"TRANSPORT_CART_DRIVE_STOPPED code={failureCode}");
                    TransportCartProjection blockedCart = ActiveCart;
                    if (blockedCart.IsDriven)
                    {
                        blockedCart.EndDrive();
                    }

                    ActiveCart = null;
                    _activeCartId = string.Empty;
                    FocusedCart = blockedCart;
                    motor.ClearTransportCartDriveProfile();
                    Remember(OperationResult.Fail(Failure.FromCode(failureCode)));
                    SetHandsState(VisibleHandsState.TargetFocused);
                }

                return;
            }

            if (ProcessAtx24PowerCableWorldInput())
            {
                ResetPowerSupplyBayFocus();
                ResetGraphicsCardSlotFocus();
                ResetProcessorCoolerSlotFocus();
                ResetM2StorageSlotFocus();
                ResetDimmSlotFocus();
                ResetProcessorSocketFocus();
                ResetMotherboardFastenerFocus();
                return;
            }

            if (ProcessLoosePowerSupplyPickupInput())
            {
                ResetGraphicsCardSlotFocus();
                ResetProcessorCoolerSlotFocus();
                ResetM2StorageSlotFocus();
                ResetDimmSlotFocus();
                ResetProcessorSocketFocus();
                ResetMotherboardFastenerFocus();
                return;
            }

            UpdatePowerSupplyBayFocus();
            if (ProcessPowerSupplyBayInput())
            {
                ResetGraphicsCardSlotFocus();
                ResetProcessorCoolerSlotFocus();
                ResetM2StorageSlotFocus();
                ResetDimmSlotFocus();
                ResetProcessorSocketFocus();
                ResetMotherboardFastenerFocus();
                return;
            }

            if (ProcessLooseGraphicsCardPickupInput())
            {
                ResetPowerSupplyBayFocus();
                ResetProcessorCoolerSlotFocus();
                ResetM2StorageSlotFocus();
                ResetDimmSlotFocus();
                ResetProcessorSocketFocus();
                ResetMotherboardFastenerFocus();
                return;
            }

            UpdateGraphicsCardSlotFocus();
            if (ProcessGraphicsCardSlotInput())
            {
                ResetPowerSupplyBayFocus();
                ResetProcessorCoolerSlotFocus();
                ResetM2StorageSlotFocus();
                ResetDimmSlotFocus();
                ResetProcessorSocketFocus();
                ResetMotherboardFastenerFocus();
                return;
            }

            if (ProcessLooseProcessorCoolerPickupInput())
            {
                ResetPowerSupplyBayFocus();
                ResetGraphicsCardSlotFocus();
                ResetM2StorageSlotFocus();
                ResetDimmSlotFocus();
                ResetProcessorSocketFocus();
                ResetMotherboardFastenerFocus();
                return;
            }

            UpdateProcessorCoolerSlotFocus();
            if (ProcessProcessorCoolerSlotInput())
            {
                ResetPowerSupplyBayFocus();
                ResetGraphicsCardSlotFocus();
                ResetM2StorageSlotFocus();
                ResetDimmSlotFocus();
                ResetProcessorSocketFocus();
                ResetMotherboardFastenerFocus();
                return;
            }

            UpdateM2StorageSlotFocus();
            if (ProcessM2StorageSlotInput())
            {
                ResetPowerSupplyBayFocus();
                ResetGraphicsCardSlotFocus();
                ResetDimmSlotFocus();
                ResetProcessorSocketFocus();
                ResetMotherboardFastenerFocus();
                return;
            }

            UpdateDimmSlotFocus();
            UpdateProcessorSocketFocus();
            UpdateMotherboardFastenerFocus();
            SelectAssemblyInteractionTarget();
            if (IsDimmSlotFocused)
            {
                FocusedCart = null;
                FocusedItem = dimmAssemblyBinding.PhysicalItem;
                SetHandsState(VisibleHandsState.TargetFocused);
                if (input.TryConsumePrimaryActionPressThisFrame())
                {
                    input.TryConsumeRotatePlacementPressThisFrame();
                    input.TryConsumeInteractPressThisFrame();
                    input.TryConsumeDropPressThisFrame();
                    TryOperateDimmRetention(dimmSlot.LastEvaluation);
                    return;
                }

                if (input.TryConsumeInteractPressThisFrame())
                {
                    input.TryConsumeRotatePlacementPressThisFrame();
                    input.TryConsumeDropPressThisFrame();
                    TryPickup(dimmAssemblyBinding.PhysicalItem);
                }

                return;
            }

            if (HasDimmSlotContext)
            {
                FocusedCart = null;
                FocusedItem = dimmAssemblyBinding.PhysicalItem;
                SetHandsState(VisibleHandsState.TargetFocused);
                bool primaryPressed = input.TryConsumePrimaryActionPressThisFrame();
                input.TryConsumeRotatePlacementPressThisFrame();
                input.TryConsumeInteractPressThisFrame();
                input.TryConsumeDropPressThisFrame();
                if (primaryPressed)
                {
                    TryOperateDimmRetention(dimmSlot.LastEvaluation);
                }

                return;
            }

            if (IsProcessorSocketFocused)
            {
                FocusedCart = null;
                FocusedItem = processorAssemblyBinding.PhysicalItem;
                SetHandsState(VisibleHandsState.TargetFocused);
                if (input.TryConsumePrimaryActionPressThisFrame())
                {
                    input.TryConsumeRotatePlacementPressThisFrame();
                    input.TryConsumeInteractPressThisFrame();
                    input.TryConsumeDropPressThisFrame();
                    TryOperateProcessorRetention(processorSocket.LastEvaluation);
                    return;
                }

                if (input.TryConsumeInteractPressThisFrame())
                {
                    input.TryConsumeRotatePlacementPressThisFrame();
                    input.TryConsumeDropPressThisFrame();
                    TryPickup(processorAssemblyBinding.PhysicalItem);
                }

                return;
            }

            if (HasProcessorSocketContext)
            {
                FocusedCart = null;
                FocusedItem = processorAssemblyBinding.PhysicalItem;
                SetHandsState(VisibleHandsState.TargetFocused);
                bool primaryPressed = input.TryConsumePrimaryActionPressThisFrame();
                input.TryConsumeRotatePlacementPressThisFrame();
                input.TryConsumeInteractPressThisFrame();
                input.TryConsumeDropPressThisFrame();
                if (primaryPressed)
                {
                    TryOperateProcessorRetention(processorSocket.LastEvaluation);
                }

                return;
            }

            if (IsMotherboardFastenerFocused)
            {
                FocusedCart = null;
                FocusedItem = motherboardAssemblyBinding.PhysicalItem;
                SetHandsState(VisibleHandsState.TargetFocused);
                if (input.TryConsumePrimaryActionPressThisFrame())
                {
                    input.TryConsumeInteractPressThisFrame();
                    input.TryConsumeDropPressThisFrame();
                    TryOperateMotherboardFastener(motherboardFastener.LastEvaluation);
                    return;
                }

                if (input.TryConsumeInteractPressThisFrame())
                {
                    TryPickup(motherboardAssemblyBinding.PhysicalItem);
                }

                return;
            }

            if (HasMotherboardFastenerContext)
            {
                FocusedCart = null;
                FocusedItem = motherboardAssemblyBinding.PhysicalItem;
                SetHandsState(VisibleHandsState.TargetFocused);
                bool primaryPressed = input.TryConsumePrimaryActionPressThisFrame();
                input.TryConsumeInteractPressThisFrame();
                input.TryConsumeDropPressThisFrame();
                if (primaryPressed)
                {
                    TryOperateMotherboardFastener(motherboardFastener.LastEvaluation);
                }

                return;
            }

            OperationResult<TransportCartProjection> resolvedCart = resolver.ResolveTransportCart();
            FocusedCart = resolvedCart.IsSuccess ? resolvedCart.Value : null;
            if (FocusedCart != null)
            {
                FocusedItem = null;
                SetHandsState(VisibleHandsState.TargetFocused);
                if (input.TryConsumePrimaryActionPressThisFrame())
                {
                    TryBeginCartDrive(FocusedCart);
                }
                else if (FocusedCart.HasCargo && input.TryConsumeInteractPressThisFrame())
                {
                    TryUnloadCart(FocusedCart);
                }

                return;
            }

            OperationResult<PhysicalItemProjection> target = resolver.Resolve();
            FocusedItem = target.IsSuccess ? target.Value : null;
            SetHandsState(FocusedItem != null
                ? VisibleHandsState.TargetFocused
                : VisibleHandsState.Empty);

            InventoryItemWorldBinding focusedBinding = GetInventoryBinding(FocusedItem);

            if (focusedBinding != null && input.TryConsumeDropPressThisFrame())
            {
                if (focusedBinding.IsCustomerReserved)
                {
                    Remember(focusedBinding.TryReleaseCustomerReservation());
                    return;
                }

                if (focusedBinding.RequiresCustomerDecisionAction)
                {
                    Remember(focusedBinding.TryApplyCurrentCustomerDecision());
                    return;
                }
            }

            if (FocusedItem != null && input.TryConsumeInteractPressThisFrame())
            {
                TryPickup(FocusedItem);
            }
        }

        public OperationResult TryRecoverHeldItem()
        {
            if (HeldItem == null)
            {
                return Remember(OperationResult.Fail(Failure.FromCode("carry.nothing-held")));
            }

            PhysicalItemProjection item = HeldItem;
            SetHandsState(VisibleHandsState.Recovering);
            if (!item.gameObject.activeSelf)
            {
                item.gameObject.SetActive(true);
            }

            if (!item.enabled)
            {
                item.enabled = true;
            }

            MotherboardAssemblyItemBinding motherboardBinding =
                GetMotherboardBinding(item);
            ProcessorAssemblyItemBinding processorBinding =
                GetProcessorBinding(item);
            DimmAssemblyItemBinding dimmBinding = GetDimmBinding(item);
            PowerSupplyAssemblyItemBinding powerSupplyAssemblyBinding =
                GetPowerSupplyBinding(item);
            GraphicsCardAssemblyItemBinding graphicsCardAssemblyBinding =
                GetGraphicsCardBinding(item);
            ProcessorCoolerAssemblyItemBinding coolerBinding =
                GetProcessorCoolerBinding(item);
            M2StorageAssemblyItemBinding storageBinding = GetM2StorageBinding(item);
            Atx24PowerCableAssemblyItemBinding cableBinding =
                GetAtx24PowerCableBinding(item);
            if (cableBinding != null)
            {
                OperationResult recovery = cableBinding.TryRecoverHeld(
                    carryAnchor,
                    heldItemLayer);
                if (recovery.IsFailure)
                {
                    return Remember(recovery);
                }

                CompleteHeldItemRelease();
                cableBinding.SyncProjectionToAuthority();
                return Remember(recovery);
            }
            if (powerSupplyAssemblyBinding != null)
            {
                OperationResult recovery =
                    powerSupplyAssemblyBinding.TryRecoverHeld(
                        carryAnchor,
                        heldItemLayer);
                if (recovery.IsFailure)
                {
                    return Remember(recovery);
                }

                CompleteHeldItemRelease();
                powerSupplyAssemblyBinding.SyncProjectionToAuthority();
                return Remember(recovery);
            }

            if (graphicsCardAssemblyBinding != null)
            {
                OperationResult recovery =
                    graphicsCardAssemblyBinding.TryRecoverHeld(
                        carryAnchor,
                        heldItemLayer);
                if (recovery.IsFailure)
                {
                    return Remember(recovery);
                }

                CompleteHeldItemRelease();
                graphicsCardAssemblyBinding.SyncProjectionToAuthority();
                return Remember(recovery);
            }

            if (coolerBinding != null)
            {
                OperationResult recovery = coolerBinding.TryRecoverHeld(
                    carryAnchor,
                    heldItemLayer);
                if (recovery.IsFailure)
                {
                    return Remember(recovery);
                }

                CompleteHeldItemRelease();
                coolerBinding.SyncProjectionToAuthority();
                return Remember(recovery);
            }

            if (storageBinding != null)
            {
                OperationResult recovery = storageBinding.TryRecoverHeld(
                    carryAnchor,
                    heldItemLayer);
                if (recovery.IsFailure)
                {
                    return Remember(recovery);
                }

                CompleteHeldItemRelease();
                storageBinding.SyncProjectionToAuthority();
                return Remember(recovery);
            }

            if (dimmBinding != null)
            {
                OperationResult recovery = dimmBinding.TryRecoverHeld(
                    carryAnchor,
                    heldItemLayer);
                if (recovery.IsFailure)
                {
                    return Remember(recovery);
                }

                CompleteHeldItemRelease();
                dimmBinding.SyncProjectionToAuthority();
                return Remember(recovery);
            }

            if (processorBinding != null)
            {
                OperationResult recovery = processorBinding.TryRecoverHeld(
                    carryAnchor,
                    heldItemLayer);
                if (recovery.IsFailure)
                {
                    return Remember(recovery);
                }

                CompleteHeldItemRelease();
                processorBinding.SyncProjectionToAuthority();
                return Remember(recovery);
            }

            if (motherboardBinding != null)
            {
                OperationResult recovery = motherboardBinding.TryRecoverHeld(
                    carryAnchor,
                    heldItemLayer);
                if (recovery.IsFailure)
                {
                    return Remember(recovery);
                }

                CompleteHeldItemRelease();
                processorAssemblyBinding?.SyncProjectionToAuthority();
                return Remember(recovery);
            }

            InventoryItemWorldBinding binding = GetInventoryBinding(item);
            OperationResult authorityTransfer = binding != null
                ? binding.TryPrepareRecoveryTransfer()
                : OperationResult.Success();
            if (authorityTransfer.IsFailure)
            {
                return Remember(authorityTransfer);
            }

            OperationResult result = item.RecoverToLastSafePose();
            if (result.IsFailure)
            {
                RollbackAuthorityTransfer(binding);
                return Remember(result);
            }

            if (binding != null)
            {
                OperationResult commit = binding.CommitPreparedTransfer(targetIsWorld: true);
                if (commit.IsFailure)
                {
                    RollbackAuthorityTransfer(binding);
                    return Remember(commit);
                }
            }

            HeldItem = null;
            _heldItemId = string.Empty;
            ResetPlacementState();
            motor?.ClearCarryProfile();
            LastFailureCode = string.Empty;
            SetHandsState(VisibleHandsState.Empty);
            return result;
        }

        private void Update()
        {
            ProcessInputFrame();
        }

        private OperationResult ReleaseHeldItem(
            Pose pose,
            bool stabilizePlacement,
            PhysicalItemProjection stackSupport = null,
            PlacementSurface placementSurface = null)
        {
            PhysicalItemProjection releasedItem = HeldItem;
            InventoryItemWorldBinding binding = GetInventoryBinding(releasedItem);
            OperationResult authorityTransfer = binding == null
                ? OperationResult.Success()
                : stabilizePlacement
                    ? binding.TryPreparePlacementTransfer(placementSurface)
                    : binding.TryPrepareDropTransfer();
            if (authorityTransfer.IsFailure)
            {
                SetCarryHandsState(blocked: true);
                return Remember(authorityTransfer);
            }

            OperationResult result = stabilizePlacement
                ? releasedItem.PlaceAt(pose, stackSupport)
                : releasedItem.ReleaseTo(pose);
            if (result.IsFailure)
            {
                RollbackAuthorityTransfer(binding);
                return Remember(result);
            }

            if (binding != null)
            {
                OperationResult commit = binding.CommitPreparedTransfer(targetIsWorld: true);
                if (commit.IsFailure)
                {
                    RollbackAuthorityTransfer(binding);
                    return Remember(commit);
                }
            }

            Physics.SyncTransforms();
            CompleteHeldItemRelease();
            return result;
        }

        private OperationResult TryPickupMotherboard(
            PhysicalItemProjection item,
            MotherboardAssemblyItemBinding binding)
        {
            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-seat.paused")));
            }

            if (binding.IsSecured)
            {
                return Remember(OperationResult.Fail(AssemblyFailures.ComponentSecured));
            }

            if (binding.Session != null &&
                binding.Session.AssemblyBuild.HasProcessorSocket &&
                binding.Session.AssemblyBuild.ProcessorSocketState !=
                    ProcessorSocketState.EmptyOpen)
            {
                return Remember(OperationResult.Fail(AssemblyFailures.ProcessorInstalled));
            }

            if (binding.Session != null &&
                binding.Session.AssemblyBuild.HasMemorySlot &&
                binding.Session.AssemblyBuild.MemorySlotState != MemorySlotState.EmptyOpen)
            {
                return Remember(OperationResult.Fail(AssemblyFailures.MemoryModuleInstalled));
            }

            if (binding.Session != null &&
                binding.Session.AssemblyBuild.HasStorageSlot &&
                binding.Session.AssemblyBuild.StorageSlotState != StorageSlotState.EmptyOpen)
            {
                return Remember(OperationResult.Fail(AssemblyFailures.StorageDeviceInstalled));
            }

            bool wasSeated = binding.IsSeated;
            OperationResult physicalPickup = item.BeginCarry(carryAnchor, heldItemLayer);
            if (physicalPickup.IsFailure)
            {
                return Remember(physicalPickup);
            }

            OperationResult authority = wasSeated
                ? binding.TryCommitSeatedDetach()
                : binding.TryCommitLoosePickup();
            if (authority.IsFailure)
            {
                OperationResult rollback = item.RecoverToLastSafePose();
                if (rollback.IsFailure)
                {
                    Debug.LogError(
                        $"ASSEMBLY_PROJECTION_ROLLBACK_FAILED code={rollback.Error.Code}");
                }

                return Remember(authority);
            }

            HeldItem = item;
            _heldItemId = item.ItemIdValue;
            FocusedItem = null;
            ResetPlacementState();
            LastFailureCode = string.Empty;
            motor?.ApplyCarryProfile(item.CarryProfile);
            processorAssemblyBinding?.SyncProjectionToAuthority();
            dimmAssemblyBinding?.SyncProjectionToAuthority();
            m2StorageAssemblyBinding?.SyncProjectionToAuthority();
            SetCarryHandsState(blocked: false);
            return physicalPickup;
        }

        private OperationResult TryPickupProcessor(
            PhysicalItemProjection item,
            ProcessorAssemblyItemBinding binding)
        {
            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-processor.paused")));
            }

            if (binding.IsRetained)
            {
                return Remember(OperationResult.Fail(AssemblyFailures.ProcessorRetained));
            }

            bool wasSeated = binding.IsSeated;
            OperationResult physicalPickup = item.BeginCarry(carryAnchor, heldItemLayer);
            if (physicalPickup.IsFailure)
            {
                return Remember(physicalPickup);
            }

            OperationResult authority = wasSeated
                ? binding.TryCommitSeatedDetach()
                : binding.TryCommitLoosePickup();
            if (authority.IsFailure)
            {
                OperationResult rollback = item.RecoverToLastSafePose();
                if (rollback.IsFailure)
                {
                    Debug.LogError(
                        $"PROCESSOR_PROJECTION_ROLLBACK_FAILED code={rollback.Error.Code}");
                }

                binding.SyncProjectionToAuthority();
                return Remember(authority);
            }

            HeldItem = item;
            _heldItemId = item.ItemIdValue;
            FocusedItem = null;
            ResetPlacementState();
            LastFailureCode = string.Empty;
            motor?.ApplyCarryProfile(item.CarryProfile);
            binding.SyncProjectionToAuthority();
            SetCarryHandsState(blocked: false);
            return physicalPickup;
        }

        private OperationResult TryPickupDimm(
            PhysicalItemProjection item,
            DimmAssemblyItemBinding binding)
        {
            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-memory.paused")));
            }

            if (binding.IsRetained)
            {
                return Remember(OperationResult.Fail(AssemblyFailures.MemoryModuleRetained));
            }

            bool wasSeated = binding.IsSeated;
            OperationResult physicalPickup = item.BeginCarry(carryAnchor, heldItemLayer);
            if (physicalPickup.IsFailure)
            {
                return Remember(physicalPickup);
            }

            OperationResult authority = wasSeated
                ? binding.TryCommitSeatedDetach()
                : binding.TryCommitLoosePickup();
            if (authority.IsFailure)
            {
                OperationResult rollback = item.RecoverToLastSafePose();
                if (rollback.IsFailure)
                {
                    Debug.LogError(
                        $"DIMM_PROJECTION_ROLLBACK_FAILED code={rollback.Error.Code}");
                }

                binding.SyncProjectionToAuthority();
                return Remember(authority);
            }

            HeldItem = item;
            _heldItemId = item.ItemIdValue;
            FocusedItem = null;
            ResetPlacementState();
            LastFailureCode = string.Empty;
            motor?.ApplyCarryProfile(item.CarryProfile);
            binding.SyncProjectionToAuthority();
            SetCarryHandsState(blocked: false);
            return physicalPickup;
        }

        private void SetMotherboardSeatMode(bool enabled)
        {
            IsMotherboardSeatMode = enabled &&
                                    HeldItem != null &&
                                    GetMotherboardBinding(HeldItem) != null;
            IsProcessorSeatMode = false;
            IsDimmSeatMode = false;
            IsPowerSupplySeatMode = false;
            IsGraphicsCardSeatMode = false;
            IsProcessorCoolerSeatMode = false;
            IsM2StorageSeatMode = false;
            IsPlacementMode = false;
            PlacementValid = false;
            CurrentStackSupport = null;
            CurrentPlacementStatus = PlacementStatus.ContextMissing;
            CurrentMotherboardSeatStatus = MotherboardSeatStatus.ContextMissing;
            LastFailureCode = string.Empty;
            if (!IsMotherboardSeatMode)
            {
                _placementRotationQuarterTurns = 0;
                placementPreview?.Hide();
                motherboardSeat?.ResetFeedback();
                SetCarryHandsState(blocked: false);
            }
        }

        private void SetProcessorSeatMode(bool enabled)
        {
            IsProcessorSeatMode = enabled &&
                                  HeldItem != null &&
                                  GetProcessorBinding(HeldItem) != null;
            IsMotherboardSeatMode = false;
            IsDimmSeatMode = false;
            IsPowerSupplySeatMode = false;
            IsGraphicsCardSeatMode = false;
            IsProcessorCoolerSeatMode = false;
            IsM2StorageSeatMode = false;
            IsPlacementMode = false;
            PlacementValid = false;
            CurrentStackSupport = null;
            CurrentPlacementStatus = PlacementStatus.ContextMissing;
            CurrentProcessorSocketStatus = ProcessorSocketStatus.ContextMissing;
            LastFailureCode = string.Empty;
            if (!IsProcessorSeatMode)
            {
                _placementRotationQuarterTurns = 0;
                placementPreview?.Hide();
                processorSocket?.ResetFeedback();
                SetCarryHandsState(blocked: false);
            }
        }

        private void SetDimmSeatMode(bool enabled)
        {
            IsDimmSeatMode = enabled &&
                             HeldItem != null &&
                             GetDimmBinding(HeldItem) != null;
            IsMotherboardSeatMode = false;
            IsProcessorSeatMode = false;
            IsPowerSupplySeatMode = false;
            IsGraphicsCardSeatMode = false;
            IsProcessorCoolerSeatMode = false;
            IsM2StorageSeatMode = false;
            IsPlacementMode = false;
            PlacementValid = false;
            CurrentStackSupport = null;
            CurrentPlacementStatus = PlacementStatus.ContextMissing;
            CurrentDimmSlotStatus = DimmSlotStatus.ContextMissing;
            LastFailureCode = string.Empty;
            if (!IsDimmSeatMode)
            {
                _placementRotationQuarterTurns = 0;
                placementPreview?.Hide();
                dimmSlot?.ResetFeedback();
                SetCarryHandsState(blocked: false);
            }
        }

        private void UpdateDimmSeatPreview(DimmAssemblyItemBinding binding)
        {
            if (!IsDimmSeatMode || HeldItem == null)
            {
                PlacementValid = false;
                CurrentPlacementStatus = PlacementStatus.ContextMissing;
                CurrentDimmSlotStatus = DimmSlotStatus.ContextMissing;
                CurrentStackSupport = null;
                placementPreview?.Hide();
                dimmSlot?.ResetFeedback();
                SetCarryHandsState(blocked: false);
                return;
            }

            ApplyDimmSeatEvaluation(EvaluateDimmSeat(binding));
        }

        private DimmSlotEvaluation EvaluateDimmSeat(DimmAssemblyItemBinding binding)
        {
            DimmSlotProjection slotProjection = binding?.Slot ?? dimmSlot;
            if (slotProjection == null)
            {
                return new DimmSlotEvaluation(
                    DimmSlotStatus.ContextMissing,
                    default,
                    false,
                    default);
            }

            binding?.SyncProjectionToAuthority();
            bool hostSecured = binding != null &&
                               binding.Session != null &&
                               binding.Session.AssemblyBuild.MotherboardSeatState ==
                                   AssemblySeatState.SeatedSecured;
            return slotProjection.EvaluateSeat(
                resolver != null ? resolver.Origin : null,
                transform,
                HeldItem,
                obstructionMask,
                _placementRotationQuarterTurns,
                motor == null || motor.IsPaused,
                binding != null &&
                    binding.IsAuthorityInHands &&
                    !binding.IsSeated &&
                    hostSecured);
        }

        private void ApplyDimmSeatEvaluation(DimmSlotEvaluation evaluation)
        {
            CurrentDimmSlotStatus = evaluation.Status;
            PlacementValid = evaluation.CanSeat;
            CurrentPlacementStatus = evaluation.CanSeat
                ? PlacementStatus.Valid
                : PlacementStatus.Blocked;
            CurrentStackSupport = null;
            LastFailureCode = evaluation.CanSeat
                ? string.Empty
                : evaluation.FailureCode;

            if (evaluation.HasPose && HeldItem != null)
            {
                PlacementEvaluation previewEvaluation = new PlacementEvaluation(
                    evaluation.CanSeat ? PlacementStatus.Valid : PlacementStatus.Blocked,
                    evaluation.Pose,
                    true);
                placementPreview?.Show(
                    HeldItem,
                    previewEvaluation,
                    DimmSeatPreviewSize);
            }
            else
            {
                placementPreview?.Hide();
            }

            SetCarryHandsState(blocked: !evaluation.CanSeat);
        }

        private void UpdateProcessorSeatPreview(
            ProcessorAssemblyItemBinding binding)
        {
            if (!IsProcessorSeatMode || HeldItem == null)
            {
                PlacementValid = false;
                CurrentPlacementStatus = PlacementStatus.ContextMissing;
                CurrentProcessorSocketStatus = ProcessorSocketStatus.ContextMissing;
                CurrentStackSupport = null;
                placementPreview?.Hide();
                processorSocket?.ResetFeedback();
                SetCarryHandsState(blocked: false);
                return;
            }

            ApplyProcessorSeatEvaluation(EvaluateProcessorSeat(binding));
        }

        private ProcessorSocketEvaluation EvaluateProcessorSeat(
            ProcessorAssemblyItemBinding binding)
        {
            ProcessorSocketProjection socketProjection = binding?.Socket ?? processorSocket;
            if (socketProjection == null)
            {
                return new ProcessorSocketEvaluation(
                    ProcessorSocketStatus.ContextMissing,
                    default,
                    false);
            }

            binding?.SyncProjectionToAuthority();
            bool hostSecured = binding != null &&
                               binding.Session != null &&
                               binding.Session.AssemblyBuild.MotherboardSeatState ==
                                   AssemblySeatState.SeatedSecured;
            return socketProjection.EvaluateSeat(
                resolver != null ? resolver.Origin : null,
                transform,
                HeldItem,
                obstructionMask,
                _placementRotationQuarterTurns,
                motor == null || motor.IsPaused,
                binding != null &&
                    binding.IsAuthorityInHands &&
                    !binding.IsSeated &&
                    hostSecured);
        }

        private void ApplyProcessorSeatEvaluation(
            ProcessorSocketEvaluation evaluation)
        {
            CurrentProcessorSocketStatus = evaluation.Status;
            PlacementValid = evaluation.CanSeat;
            CurrentPlacementStatus = evaluation.CanSeat
                ? PlacementStatus.Valid
                : PlacementStatus.Blocked;
            CurrentStackSupport = null;
            LastFailureCode = evaluation.CanSeat
                ? string.Empty
                : evaluation.FailureCode;

            if (evaluation.HasPose && HeldItem != null)
            {
                PlacementEvaluation previewEvaluation = new PlacementEvaluation(
                    evaluation.CanSeat ? PlacementStatus.Valid : PlacementStatus.Blocked,
                    evaluation.Pose,
                    true);
                placementPreview?.Show(
                    HeldItem,
                    previewEvaluation,
                    ProcessorSeatPreviewSize);
            }
            else
            {
                placementPreview?.Hide();
            }

            SetCarryHandsState(blocked: !evaluation.CanSeat);
        }

        private void UpdateMotherboardSeatPreview(
            MotherboardAssemblyItemBinding binding)
        {
            if (!IsMotherboardSeatMode || HeldItem == null)
            {
                PlacementValid = false;
                placementPreview?.Hide();
                motherboardSeat?.ResetFeedback();
                return;
            }

            ApplyMotherboardSeatEvaluation(EvaluateMotherboardSeat(binding));
        }

        private MotherboardSeatEvaluation EvaluateMotherboardSeat(
            MotherboardAssemblyItemBinding binding)
        {
            MotherboardSeatProjection seatProjection = binding?.Seat ?? motherboardSeat;
            if (seatProjection == null)
            {
                return new MotherboardSeatEvaluation(
                    MotherboardSeatStatus.ContextMissing,
                    default,
                    false);
            }

            return seatProjection.Evaluate(
                resolver != null ? resolver.Origin : null,
                transform,
                HeldItem,
                obstructionMask,
                _placementRotationQuarterTurns,
                motor == null || motor.IsPaused,
                binding.IsAuthorityInHands && !binding.IsSeated);
        }

        private void ApplyMotherboardSeatEvaluation(
            MotherboardSeatEvaluation evaluation)
        {
            CurrentMotherboardSeatStatus = evaluation.Status;
            PlacementValid = evaluation.IsValid;
            CurrentPlacementStatus = evaluation.IsValid
                ? PlacementStatus.Valid
                : PlacementStatus.Blocked;
            CurrentStackSupport = null;
            LastFailureCode = evaluation.IsValid
                ? string.Empty
                : evaluation.FailureCode;

            if (evaluation.HasPose && HeldItem != null)
            {
                PlacementEvaluation previewEvaluation = new PlacementEvaluation(
                    evaluation.IsValid ? PlacementStatus.Valid : PlacementStatus.Blocked,
                    evaluation.Pose,
                    true);
                placementPreview?.Show(
                    HeldItem,
                    previewEvaluation,
                    MotherboardSeatPreviewSize);
            }
            else
            {
                placementPreview?.Hide();
            }

            SetCarryHandsState(blocked: !evaluation.IsValid);
        }

        private void UpdateDimmSlotFocus()
        {
            if (dimmSlot == null || dimmAssemblyBinding == null)
            {
                ResetDimmSlotFocus();
                return;
            }

            dimmAssemblyBinding.SyncProjectionToAuthority();
            ApplyDimmSlotEvaluation(EvaluateDimmSlotInteraction());
        }

        private DimmSlotEvaluation EvaluateDimmSlotInteraction()
        {
            GarageStockFlowSession session = dimmAssemblyBinding != null
                ? dimmAssemblyBinding.Session
                : null;
            MemorySlotState state = session != null
                ? session.AssemblyBuild.MemorySlotState
                : MemorySlotState.Unsupported;
            bool retentionCloseAvailable = session != null &&
                                           (state == MemorySlotState.MemoryModuleRetained ||
                                            session.AssemblyBuild.MotherboardSeatState ==
                                                AssemblySeatState.SeatedSecured);
            return dimmSlot.EvaluateInteraction(
                resolver != null ? resolver.Origin : null,
                transform,
                dimmAssemblyBinding != null
                    ? dimmAssemblyBinding.PhysicalItem
                    : null,
                obstructionMask,
                motor == null || motor.IsPaused,
                dimmAssemblyBinding != null && dimmAssemblyBinding.IsSeated,
                state,
                retentionCloseAvailable);
        }

        private void ApplyDimmSlotEvaluation(DimmSlotEvaluation evaluation)
        {
            CurrentDimmSlotStatus = evaluation.Status;
            IsDimmSlotFocused = evaluation.CanOperateRetention || evaluation.CanRemove;
            HasDimmSlotContext = evaluation.HasOwnedContext;
            if (!IsDimmSlotFocused && HasDimmSlotContext)
            {
                LastFailureCode = evaluation.FailureCode;
            }
        }

        private void ResetDimmSlotFocus()
        {
            IsDimmSlotFocused = false;
            HasDimmSlotContext = false;
            CurrentDimmSlotStatus = DimmSlotStatus.ContextMissing;
            dimmSlot?.ResetFeedback();
        }

        private void UpdateProcessorSocketFocus()
        {
            if (processorSocket == null || processorAssemblyBinding == null)
            {
                ResetProcessorSocketFocus();
                return;
            }

            processorAssemblyBinding.SyncProjectionToAuthority();
            ApplyProcessorSocketEvaluation(EvaluateProcessorSocketInteraction());
        }

        private ProcessorSocketEvaluation EvaluateProcessorSocketInteraction()
        {
            GarageStockFlowSession session = processorAssemblyBinding != null
                ? processorAssemblyBinding.Session
                : null;
            ProcessorSocketState state = session != null
                ? session.AssemblyBuild.ProcessorSocketState
                : ProcessorSocketState.Unsupported;
            bool retentionCloseAvailable = session != null &&
                                           (state == ProcessorSocketState.ProcessorRetained ||
                                            session.AssemblyBuild.MotherboardSeatState ==
                                                AssemblySeatState.SeatedSecured);
            return processorSocket.EvaluateInteraction(
                resolver != null ? resolver.Origin : null,
                transform,
                processorAssemblyBinding != null
                    ? processorAssemblyBinding.PhysicalItem
                    : null,
                obstructionMask,
                motor == null || motor.IsPaused,
                processorAssemblyBinding != null && processorAssemblyBinding.IsSeated,
                state,
                retentionCloseAvailable);
        }

        private void ApplyProcessorSocketEvaluation(
            ProcessorSocketEvaluation evaluation)
        {
            CurrentProcessorSocketStatus = evaluation.Status;
            IsProcessorSocketFocused = evaluation.CanOperateRetention || evaluation.CanRemove;
            HasProcessorSocketContext = evaluation.HasOwnedContext;
            if (!IsProcessorSocketFocused && HasProcessorSocketContext)
            {
                LastFailureCode = evaluation.FailureCode;
            }
        }

        private void ResetProcessorSocketFocus()
        {
            IsProcessorSocketFocused = false;
            HasProcessorSocketContext = false;
            CurrentProcessorSocketStatus = ProcessorSocketStatus.ContextMissing;
            processorSocket?.ResetFeedback();
        }

        private void SelectAssemblyInteractionTarget()
        {
            bool dimmHasTarget = IsDimmSlotFocused || HasDimmSlotContext;
            bool processorHasTarget = IsProcessorSocketFocused || HasProcessorSocketContext;
            bool fastenerHasTarget = IsMotherboardFastenerFocused ||
                                     HasMotherboardFastenerContext;
            int targetCount = (dimmHasTarget ? 1 : 0) +
                              (processorHasTarget ? 1 : 0) +
                              (fastenerHasTarget ? 1 : 0);
            if (targetCount <= 1)
            {
                return;
            }

            Transform origin = resolver != null ? resolver.Origin : null;
            Collider dimmFocus = dimmSlot != null ? dimmSlot.FocusCollider : null;
            Collider processorFocus = processorSocket != null
                ? processorSocket.FocusCollider
                : null;
            Collider fastenerFocus = motherboardFastener != null
                ? motherboardFastener.FocusCollider
                : null;
            if (origin == null)
            {
                ResetDimmSlotFocus();
                ResetProcessorSocketFocus();
                ResetMotherboardFastenerFocus();
                return;
            }

            int winner = 0;
            float bestDot = -2f;
            float bestDistance = float.PositiveInfinity;
            ConsiderAssemblyTarget(
                origin,
                processorHasTarget ? processorFocus : null,
                1,
                ref winner,
                ref bestDot,
                ref bestDistance);
            ConsiderAssemblyTarget(
                origin,
                dimmHasTarget ? dimmFocus : null,
                2,
                ref winner,
                ref bestDot,
                ref bestDistance);
            ConsiderAssemblyTarget(
                origin,
                fastenerHasTarget ? fastenerFocus : null,
                3,
                ref winner,
                ref bestDot,
                ref bestDistance);

            if (winner != 1)
            {
                ResetProcessorSocketFocus();
            }

            if (winner != 2)
            {
                ResetDimmSlotFocus();
            }

            if (winner != 3)
            {
                ResetMotherboardFastenerFocus();
            }
        }

        private static void ConsiderAssemblyTarget(
            Transform origin,
            Collider focus,
            int candidate,
            ref int winner,
            ref float bestDot,
            ref float bestDistance)
        {
            if (origin == null || focus == null)
            {
                return;
            }

            Vector3 toFocus = focus.bounds.center - origin.position;
            float distance = toFocus.magnitude;
            float dot = distance > Mathf.Epsilon
                ? Vector3.Dot(origin.forward, toFocus / distance)
                : -1f;
            const float tieEpsilon = 0.0001f;
            bool wins = winner == 0 ||
                        dot > bestDot + tieEpsilon ||
                        (Mathf.Abs(dot - bestDot) <= tieEpsilon &&
                         distance < bestDistance - tieEpsilon);
            if (!wins)
            {
                return;
            }

            winner = candidate;
            bestDot = dot;
            bestDistance = distance;
        }

        private void UpdateMotherboardFastenerFocus()
        {
            if (motherboardFastener == null || motherboardAssemblyBinding == null)
            {
                ResetMotherboardFastenerFocus();
                return;
            }

            ApplyMotherboardFastenerEvaluation(EvaluateMotherboardFastener());
        }

        private MotherboardFastenerEvaluation EvaluateMotherboardFastener()
        {
            return motherboardFastener.Evaluate(
                resolver != null ? resolver.Origin : null,
                transform,
                obstructionMask,
                motor == null || motor.IsPaused,
                motherboardAssemblyBinding != null &&
                    motherboardAssemblyBinding.IsSeated,
                motherboardAssemblyBinding != null &&
                    motherboardAssemblyBinding.IsSecured);
        }

        private void ApplyMotherboardFastenerEvaluation(
            MotherboardFastenerEvaluation evaluation)
        {
            CurrentMotherboardFastenerStatus = evaluation.Status;
            IsMotherboardFastenerFocused = evaluation.CanOperate;
            HasMotherboardFastenerContext = evaluation.CanOperate ||
                                            evaluation.Status ==
                                                MotherboardFastenerStatus.LineOfSightBlocked ||
                                            evaluation.Status ==
                                                MotherboardFastenerStatus.Obstructed;
            if (!evaluation.CanOperate &&
                evaluation.Status != MotherboardFastenerStatus.OutOfRange &&
                evaluation.Status != MotherboardFastenerStatus.NotFocused &&
                evaluation.Status != MotherboardFastenerStatus.AuthorityBlocked)
            {
                LastFailureCode = evaluation.FailureCode;
            }
        }

        private void ResetMotherboardFastenerFocus()
        {
            IsMotherboardFastenerFocused = false;
            HasMotherboardFastenerContext = false;
            CurrentMotherboardFastenerStatus = MotherboardFastenerStatus.ContextMissing;
            motherboardFastener?.ResetFeedback();
        }

        private void CompleteHeldItemRelease()
        {
            HeldItem = null;
            _heldItemId = string.Empty;
            ResetPlacementState();
            atx24PowerCableBinding?.SyncProjectionToAuthority();
            processorAssemblyBinding?.SyncProjectionToAuthority();
            dimmAssemblyBinding?.SyncProjectionToAuthority();
            powerSupplyBinding?.SyncProjectionToAuthority();
            graphicsCardBinding?.SyncProjectionToAuthority();
            processorCoolerBinding?.SyncProjectionToAuthority();
            m2StorageAssemblyBinding?.SyncProjectionToAuthority();
            motor?.ClearCarryProfile();
            LastFailureCode = string.Empty;
            SetHandsState(VisibleHandsState.Empty);
        }

        private void SetPlacementMode(bool enabled)
        {
            IsPlacementMode = enabled && HeldItem != null && HeldItem.SupportsPlacement;
            IsAtx24PowerCableRouteMode = false;
            atx24PowerCableRoute?.SetRouteModeActive(active: false);
            IsProcessorSeatMode = false;
            IsDimmSeatMode = false;
            IsPowerSupplySeatMode = false;
            IsGraphicsCardSeatMode = false;
            IsProcessorCoolerSeatMode = false;
            IsM2StorageSeatMode = false;
            PlacementValid = false;
            CurrentStackSupport = null;
            CurrentPlacementStatus = PlacementStatus.ContextMissing;
            LastFailureCode = string.Empty;
            if (!IsPlacementMode)
            {
                _placementRotationQuarterTurns = 0;
                placementPreview?.Hide();
                SetCarryHandsState(blocked: false);
            }
        }

        private void UpdatePlacementPreview()
        {
            if (!IsPlacementMode || HeldItem == null)
            {
                PlacementValid = false;
                placementPreview?.Hide();
                return;
            }

            PlacementEvaluation evaluation = PlacementSolver.Evaluate(
                transform,
                HeldItem,
                supportMask,
                obstructionMask,
                _placementRotationQuarterTurns,
                stackSupportMask);
            ApplyPlacementEvaluation(evaluation);
        }

        private void ApplyPlacementEvaluation(PlacementEvaluation evaluation)
        {
            CurrentPlacementStatus = evaluation.Status;
            PlacementValid = evaluation.IsValid;
            CurrentStackSupport = evaluation.StackSupport;
            LastFailureCode = evaluation.IsValid ? string.Empty : evaluation.FailureCode;
            placementPreview?.Show(HeldItem, evaluation);
            SetCarryHandsState(blocked: !evaluation.IsValid);
        }

        private void ResetPlacementState()
        {
            IsPlacementMode = false;
            ResetAtx24PowerCableState();
            IsMotherboardSeatMode = false;
            IsProcessorSeatMode = false;
            IsDimmSeatMode = false;
            IsPowerSupplySeatMode = false;
            IsGraphicsCardSeatMode = false;
            IsProcessorCoolerSeatMode = false;
            IsM2StorageSeatMode = false;
            _placementRotationQuarterTurns = 0;
            PlacementValid = false;
            CurrentStackSupport = null;
            CurrentPlacementStatus = PlacementStatus.ContextMissing;
            CurrentMotherboardSeatStatus = MotherboardSeatStatus.ContextMissing;
            CurrentProcessorSocketStatus = ProcessorSocketStatus.ContextMissing;
            CurrentDimmSlotStatus = DimmSlotStatus.ContextMissing;
            CurrentPowerSupplyBayStatus = PowerSupplyBayStatus.ContextMissing;
            CurrentGraphicsCardSlotStatus =
                GraphicsCardSlotStatus.ContextMissing;
            CurrentProcessorCoolerSlotStatus =
                ProcessorCoolerSlotStatus.ContextMissing;
            CurrentM2StorageSlotStatus = M2StorageSlotStatus.ContextMissing;
            ResetPowerSupplyBayFocus();
            ResetGraphicsCardSlotFocus();
            ResetProcessorCoolerSlotFocus();
            ResetM2StorageSlotFocus();
            ResetDimmSlotFocus();
            ResetProcessorSocketFocus();
            ResetMotherboardFastenerFocus();
            placementPreview?.Hide();
            motherboardSeat?.ResetFeedback();
            processorSocket?.ResetFeedback();
            dimmSlot?.ResetFeedback();
            powerSupplyBay?.ResetFeedback();
            graphicsCardSlot?.ResetFeedback();
            processorCoolerSlot?.ResetFeedback();
            m2StorageSlot?.ResetFeedback();
        }

        private void OnDisable()
        {
            placementPreview?.Hide();
            if (Application.isPlaying && !_applicationQuitting && ActiveCart != null)
            {
                if (ActiveCart.IsDriven)
                {
                    ActiveCart.EndDrive();
                }

                ActiveCart = null;
                _activeCartId = string.Empty;
                FocusedCart = null;
                motor?.ClearTransportCartDriveProfile();
            }

            if (Application.isPlaying && !_applicationQuitting && HeldItem != null)
            {
                TryRecoverHeldItem();
            }
            else if (Application.isPlaying && !_applicationQuitting)
            {
                motor?.ClearCarryProfile();
            }
        }

        private void OnApplicationQuit()
        {
            _applicationQuitting = true;
        }

        private OperationResult Remember(OperationResult result)
        {
            LastFailureCode = result.IsFailure ? result.Error.Code : string.Empty;
            return result;
        }

        private void SetHandsState(VisibleHandsState state)
        {
            if (hands != null)
            {
                hands.SetState(state);
            }
        }

        private void SetCarryHandsState(bool blocked)
        {
            bool carryingLarge = HeldItem != null &&
                                 HeldItem.CarryProfile == PhysicalCarryProfile.LargeBox;
            SetHandsState(carryingLarge
                ? (blocked ? VisibleHandsState.LargeDropBlocked : VisibleHandsState.CarryingLargeItem)
                : (blocked ? VisibleHandsState.DropBlocked : VisibleHandsState.CarryingSmallItem));
        }

        private static InventoryItemWorldBinding GetInventoryBinding(PhysicalItemProjection item)
        {
            return item != null ? item.GetComponent<InventoryItemWorldBinding>() : null;
        }

        private static MotherboardAssemblyItemBinding GetMotherboardBinding(
            PhysicalItemProjection item)
        {
            return item != null
                ? item.GetComponent<MotherboardAssemblyItemBinding>()
                : null;
        }

        private static ProcessorAssemblyItemBinding GetProcessorBinding(
            PhysicalItemProjection item)
        {
            return item != null
                ? item.GetComponent<ProcessorAssemblyItemBinding>()
                : null;
        }

        private static DimmAssemblyItemBinding GetDimmBinding(
            PhysicalItemProjection item)
        {
            return item != null
                ? item.GetComponent<DimmAssemblyItemBinding>()
                : null;
        }

        private static string GetMotherboardSeatStatusLabel(
            MotherboardSeatStatus status)
        {
            return status switch
            {
                MotherboardSeatStatus.Valid => "HİZALI",
                MotherboardSeatStatus.OutOfRange => "YAKLAŞ",
                MotherboardSeatStatus.NotFocused => "SLOTU HEDEFLE",
                MotherboardSeatStatus.LineOfSightBlocked => "ÖNÜNÜ AÇ",
                MotherboardSeatStatus.OrientationInvalid => "YÖN",
                MotherboardSeatStatus.Unsupported => "TABLA YOK",
                MotherboardSeatStatus.Obstructed => "SLOT DOLU",
                MotherboardSeatStatus.Paused => "DURAKLATILDI",
                MotherboardSeatStatus.AuthorityBlocked => "İŞLEM YOK",
                _ => "BAĞLANTI YOK"
            };
        }

        private static string GetProcessorSocketStatusLabel(
            ProcessorSocketStatus status)
        {
            return status switch
            {
                ProcessorSocketStatus.ValidSeat => "HİZALI",
                ProcessorSocketStatus.OutOfRange => "YAKLAŞ",
                ProcessorSocketStatus.NotFocused => "CPU SOKETİNİ HEDEFLE",
                ProcessorSocketStatus.LineOfSightBlocked => "ÖNÜNÜ AÇ",
                ProcessorSocketStatus.OrientationInvalid => "ANAHTAR YÖNÜ",
                ProcessorSocketStatus.Obstructed => "SOKET ENGELLİ",
                ProcessorSocketStatus.Paused => "DURAKLATILDI",
                ProcessorSocketStatus.AuthorityBlocked => "ANAKART SABİT DEĞİL",
                ProcessorSocketStatus.ValidSeatedOpenRetentionBlocked =>
                    "ANAKART SABİT DEĞİL",
                _ => "BAĞLANTI YOK"
            };
        }

        private static string GetDimmSlotStatusLabel(DimmSlotStatus status)
        {
            return status switch
            {
                DimmSlotStatus.ValidSeat => "ÇENTİK HİZALI",
                DimmSlotStatus.OutOfRange => "YAKLAŞ",
                DimmSlotStatus.NotFocused => "A2 SLOTU HEDEFLE",
                DimmSlotStatus.LineOfSightBlocked => "ÖNÜNÜ AÇ",
                DimmSlotStatus.OrientationInvalid => "DDR5 ÇENTİK YÖNÜ",
                DimmSlotStatus.Obstructed => "A2 SLOTU ENGELLİ",
                DimmSlotStatus.Paused => "DURAKLATILDI",
                DimmSlotStatus.AuthorityBlocked => "ANAKART SABİT DEĞİL",
                DimmSlotStatus.ValidSeatedOpenRetentionBlocked =>
                    "ANAKART SABİT DEĞİL",
                _ => "BAĞLANTI YOK"
            };
        }

        private static string GetStockStateSuffix(PhysicalItemProjection item)
        {
            InventoryItemWorldBinding binding = GetInventoryBinding(item);
            return binding != null ? $"   |   {binding.LocationLabel}" : string.Empty;
        }

        private static void RollbackAuthorityTransfer(InventoryItemWorldBinding binding)
        {
            if (binding == null || !binding.HasPreparedTransfer)
            {
                return;
            }

            OperationResult rollback = binding.RollbackPreparedTransfer();
            if (rollback.IsFailure)
            {
                Debug.LogError($"STOCK_PROJECTION_ROLLBACK_FAILED code={rollback.Error.Code}");
            }
        }
    }
}
