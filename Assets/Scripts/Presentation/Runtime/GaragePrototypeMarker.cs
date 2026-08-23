using System;
using System.Collections;
using PCShopEmpire3D.Actors;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Economy;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Presentation.Input;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.Presentation.Player;
using PCShopEmpire3D.Retail;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation
{
    public sealed partial class GaragePrototypeMarker : MonoBehaviour
    {
        public const string ScenePath = "Assets/Scenes/Prototypes/GarageGraybox.unity";
        public const string Version = "garage-psu-four-screw-r29-v1";
        public const string ProcessorCoolerR27Marker =
            ProcessorCoolerRuntimeGeometry.RuntimeMarker;
        public const string PowerSupplyR29Marker =
            PowerSupplyRuntimeGeometry.RuntimeMarker;

        [SerializeField] private FirstPersonMotor playerMotor;
        [SerializeField] private PlayerInputAdapter playerInput;
        [SerializeField] private PlayerCarryController playerCarry;
        [SerializeField] private TransportCartProjection transportCart;
        [SerializeField] private GarageStockFlowRuntime stockFlow;
        [SerializeField] private GarageCustomerFlowRuntime customerFlow;
        [SerializeField] private CheckoutStationProjection checkoutStation;
        [SerializeField] private MotherboardSeatProjection motherboardSeat;
        [SerializeField] private MotherboardFastenerProjection motherboardFastener;
        [SerializeField] private MotherboardAssemblyItemBinding motherboardBinding;
        [SerializeField] private ProcessorSocketProjection processorSocket;
        [SerializeField] private ProcessorAssemblyItemBinding processorBinding;
        [SerializeField] private PhysicalItemProjection processor;
        [SerializeField] private DimmSlotProjection dimmSlot;
        [SerializeField] private DimmAssemblyItemBinding dimmBinding;
        [SerializeField] private PhysicalItemProjection memoryModule;
        [SerializeField] private M2StorageSlotProjection storageSlot;
        [SerializeField] private M2StorageAssemblyItemBinding storageBinding;
        [SerializeField] private PhysicalItemProjection storageDevice;
        [SerializeField] private ProcessorCoolerSlotProjection processorCoolerSlot;
        [SerializeField] private ProcessorCoolerAssemblyItemBinding processorCoolerBinding;
        [SerializeField] private PhysicalItemProjection processorCooler;
        [SerializeField] private ProcessorCoolerRuntimeGeometry processorCoolerGeometry;
        [SerializeField] private GraphicsCardSlotProjection graphicsCardSlot;
        [SerializeField] private GraphicsCardAssemblyItemBinding graphicsCardBinding;
        [SerializeField] private PhysicalItemProjection graphicsCard;
        [SerializeField] private PowerSupplyBayProjection powerSupplyBay;
        [SerializeField] private PowerSupplyAssemblyItemBinding powerSupplyBinding;
        [SerializeField] private PhysicalItemProjection powerSupply;
        [SerializeField] private PowerSupplyRuntimeGeometry powerSupplyGeometry;

        public FirstPersonMotor PlayerMotor => playerMotor;

        public PlayerInputAdapter PlayerInput => playerInput;

        public PlayerCarryController PlayerCarry => playerCarry;

        public TransportCartProjection TransportCart => transportCart;

        public GarageStockFlowRuntime StockFlow => stockFlow;

        public GarageCustomerFlowRuntime CustomerFlow => customerFlow;

        public CheckoutStationProjection CheckoutStation => checkoutStation;

        public MotherboardSeatProjection MotherboardSeat => motherboardSeat;

        public MotherboardFastenerProjection MotherboardFastener => motherboardFastener;

        public MotherboardAssemblyItemBinding MotherboardBinding => motherboardBinding;

        public ProcessorSocketProjection ProcessorSocket => processorSocket;

        public ProcessorAssemblyItemBinding ProcessorBinding => processorBinding;

        public PhysicalItemProjection Processor => processor;

        public DimmSlotProjection DimmSlot => dimmSlot;

        public DimmAssemblyItemBinding DimmBinding => dimmBinding;

        public PhysicalItemProjection MemoryModule => memoryModule;

        public M2StorageSlotProjection StorageSlot => storageSlot;

        public M2StorageAssemblyItemBinding StorageBinding => storageBinding;

        public PhysicalItemProjection StorageDevice => storageDevice;

        public ProcessorCoolerSlotProjection ProcessorCoolerSlot =>
            processorCoolerSlot;

        public ProcessorCoolerAssemblyItemBinding ProcessorCoolerBinding =>
            processorCoolerBinding;

        public PhysicalItemProjection ProcessorCooler => processorCooler;

        public ProcessorCoolerRuntimeGeometry ProcessorCoolerGeometry =>
            processorCoolerGeometry;

        public GraphicsCardSlotProjection GraphicsCardSlot => graphicsCardSlot;

        public GraphicsCardAssemblyItemBinding GraphicsCardBinding =>
            graphicsCardBinding;

        public PhysicalItemProjection GraphicsCard => graphicsCard;

        public PowerSupplyBayProjection PowerSupplyBay => powerSupplyBay;

        public PowerSupplyAssemblyItemBinding PowerSupplyBinding =>
            powerSupplyBinding;

        public PhysicalItemProjection PowerSupply => powerSupply;

        public PowerSupplyRuntimeGeometry PowerSupplyGeometry =>
            powerSupplyGeometry;

        /// <summary>
        /// Deliberately false until the scene owns exactly one canonical r27 cooler geometry.
        /// This is a smoke flag, not authority and not a substitute for the domain snapshot.
        /// </summary>
        public bool HasProcessorCoolerR27Runtime =>
            processorCoolerGeometry != null &&
            processorCoolerGeometry.IsCanonical &&
            processorCoolerSlot != null &&
            processorCoolerSlot.IsConfigured &&
            processorCoolerBinding != null &&
            processorCoolerBinding.Slot == processorCoolerSlot &&
            processorCoolerBinding.PhysicalItem == processorCooler &&
            FindObjectsByType<ProcessorCoolerRuntimeGeometry>(
                FindObjectsSortMode.None).Length == 1;

        public bool HasGraphicsCardR28Runtime =>
            graphicsCardSlot != null &&
            graphicsCardSlot.IsConfigured &&
            graphicsCardBinding != null &&
            graphicsCardBinding.Slot == graphicsCardSlot &&
            graphicsCardBinding.PhysicalItem == graphicsCard &&
            graphicsCard != null &&
            processorCooler != null &&
            graphicsCardSlot.ChassisClearanceBlockers.Length == 5 &&
            graphicsCardSlot.CoolerClearanceBlockers.Length == 1 &&
            graphicsCardSlot.CoolerClearanceBlockers[0] ==
                processorCooler.GetComponent<Collider>() &&
            graphicsCardSlot.FocusCollider.isTrigger &&
            graphicsCardSlot.FocusCollider.gameObject.layer ==
                LayerMask.NameToLayer("Interactable") &&
            graphicsCardSlot.SupportCollider.gameObject.layer ==
                LayerMask.NameToLayer("Ignore Raycast") &&
            CountCanonicalGraphicsCardProjections(
                GarageStockFlowSession.GraphicsCardAssemblyItemInstanceIdValue) == 1;

        public bool HasPowerSupplyR29Runtime =>
            powerSupplyGeometry != null &&
            powerSupplyGeometry.IsCanonical &&
            powerSupplyBay != null &&
            powerSupplyBay.IsConfigured &&
            powerSupplyBinding != null &&
            powerSupplyBinding.Slot == powerSupplyBay &&
            powerSupplyBinding.PhysicalItem == powerSupply &&
            powerSupply != null &&
            powerSupplyBay.FocusCollider.isTrigger &&
            powerSupplyBay.FocusCollider.gameObject.layer ==
                LayerMask.NameToLayer("Interactable") &&
            powerSupplyBay.SupportCollider.gameObject.layer ==
                LayerMask.NameToLayer("Ignore Raycast") &&
            CountCanonicalPowerSupplyProjections(
                GarageStockFlowSession.PowerSupplyItemInstanceIdValue) == 1;

        public void Configure(
            FirstPersonMotor motor,
            PlayerInputAdapter input,
            PlayerCarryController carry,
            TransportCartProjection cart,
            GarageStockFlowRuntime garageStockFlow = null,
            GarageCustomerFlowRuntime garageCustomerFlow = null,
            CheckoutStationProjection physicalCheckoutStation = null,
            MotherboardSeatProjection physicalMotherboardSeat = null,
            MotherboardFastenerProjection physicalMotherboardFastener = null,
            MotherboardAssemblyItemBinding physicalMotherboardBinding = null,
            ProcessorSocketProjection physicalProcessorSocket = null,
            ProcessorAssemblyItemBinding physicalProcessorBinding = null,
            PhysicalItemProjection physicalProcessor = null,
            DimmSlotProjection physicalDimmSlot = null,
            DimmAssemblyItemBinding physicalDimmBinding = null,
            PhysicalItemProjection physicalMemoryModule = null,
            M2StorageSlotProjection physicalStorageSlot = null,
            M2StorageAssemblyItemBinding physicalStorageBinding = null,
            PhysicalItemProjection physicalStorageDevice = null,
            ProcessorCoolerSlotProjection physicalProcessorCoolerSlot = null,
            ProcessorCoolerAssemblyItemBinding physicalProcessorCoolerBinding = null,
            PhysicalItemProjection physicalProcessorCooler = null,
            ProcessorCoolerRuntimeGeometry physicalProcessorCoolerGeometry = null,
            GraphicsCardSlotProjection physicalGraphicsCardSlot = null,
            GraphicsCardAssemblyItemBinding physicalGraphicsCardBinding = null,
            PhysicalItemProjection physicalGraphicsCard = null,
            PowerSupplyBayProjection physicalPowerSupplyBay = null,
            PowerSupplyAssemblyItemBinding physicalPowerSupplyBinding = null,
            PhysicalItemProjection physicalPowerSupply = null,
            PowerSupplyRuntimeGeometry physicalPowerSupplyGeometry = null)
        {
            playerMotor = motor;
            playerInput = input;
            playerCarry = carry;
            transportCart = cart;
            stockFlow = garageStockFlow;
            customerFlow = garageCustomerFlow;
            checkoutStation = physicalCheckoutStation;
            motherboardSeat = physicalMotherboardSeat;
            motherboardFastener = physicalMotherboardFastener;
            motherboardBinding = physicalMotherboardBinding;
            processorSocket = physicalProcessorSocket;
            processorBinding = physicalProcessorBinding;
            processor = physicalProcessor;
            dimmSlot = physicalDimmSlot;
            dimmBinding = physicalDimmBinding;
            memoryModule = physicalMemoryModule;
            storageSlot = physicalStorageSlot;
            storageBinding = physicalStorageBinding;
            storageDevice = physicalStorageDevice;
            processorCoolerSlot = physicalProcessorCoolerSlot;
            processorCoolerBinding = physicalProcessorCoolerBinding;
            processorCooler = physicalProcessorCooler;
            processorCoolerGeometry = physicalProcessorCoolerGeometry;
            graphicsCardSlot = physicalGraphicsCardSlot;
            graphicsCardBinding = physicalGraphicsCardBinding;
            graphicsCard = physicalGraphicsCard;
            powerSupplyBay = physicalPowerSupplyBay;
            powerSupplyBinding = physicalPowerSupplyBinding;
            powerSupply = physicalPowerSupply;
            powerSupplyGeometry = physicalPowerSupplyGeometry;
        }

        private void Start()
        {
            bool hasLargeBox = false;
            int smallBoxCount = 0;
            PhysicalItemProjection[] items = FindObjectsByType<PhysicalItemProjection>(
                FindObjectsSortMode.None);
            foreach (PhysicalItemProjection item in items)
            {
                if (item.CarryProfile == PhysicalCarryProfile.LargeBox)
                {
                    hasLargeBox = true;
                }
                else if (item.CarryProfile == PhysicalCarryProfile.SmallBox)
                {
                    smallBoxCount++;
                }
            }

            bool hasRotationAction = playerInput?.Actions?.FindActionMap(
                PlayerInputContract.PlayerMap,
                false)?.FindAction(PlayerInputContract.RotatePlacement, false) != null;
            bool hasRotationSurface = false;
            PlacementSurface[] surfaces = FindObjectsByType<PlacementSurface>(FindObjectsSortMode.None);
            foreach (PlacementSurface surface in surfaces)
            {
                if (Mathf.Approximately(surface.YawStepDegrees, 90f))
                {
                    hasRotationSurface = true;
                    break;
                }
            }

            Transform[] sceneTransforms = FindObjectsByType<Transform>(FindObjectsSortMode.None);
            bool hasLookdevCorner = false;
            bool hasLookdevVolume = false;
            bool hasTaskLight = false;
            foreach (Transform sceneTransform in sceneTransforms)
            {
                hasLookdevCorner |= sceneTransform.name == "VisualBenchmarkCorner";
                hasLookdevVolume |= sceneTransform.name == "GlobalLookdevVolume";
                hasTaskLight |= sceneTransform.name == "WorkbenchTaskLight";
            }

            bool hasArrivedStockFlow = stockFlow != null &&
                                       stockFlow.Session != null &&
                                       stockFlow.Session.Order.Status ==
                                       PCShopEmpire3D.Orders.PurchaseOrderStatus.Arrived;
            bool hasShelfOfferAuthority = hasArrivedStockFlow &&
                                          stockFlow.Session.RetailOffers != null &&
                                          stockFlow.Session.RetailOffers.Count == 0;
            bool hasBasketAuthority = hasArrivedStockFlow &&
                                      stockFlow.Session.RetailBaskets != null &&
                                      stockFlow.Session.RetailBaskets.Count == 0;
            bool hasCheckoutAuthority = hasArrivedStockFlow &&
                                        stockFlow.Session.RetailCheckouts != null &&
                                        stockFlow.Session.RetailCheckouts.Count == 0;
            bool hasCheckoutCompletionAuthority = hasCheckoutAuthority &&
                                                  stockFlow.Session.RetailCheckouts.CompletionCount == 0;
            bool hasEconomySettlementAuthority = hasCheckoutAuthority &&
                                                 stockFlow.Session.CheckoutSettlements != null &&
                                                 stockFlow.Session.CheckoutSettlements.SettlementCount == 0;
            bool hasCashLedgerAuthority = hasEconomySettlementAuthority &&
                                          stockFlow.Session.CheckoutSettlements.TransactionCount == 0;
            bool hasCustomerVisitAuthority = hasArrivedStockFlow &&
                                             stockFlow.Session.CustomerVisits != null &&
                                             stockFlow.Session.CustomerVisits.Count == 0;
            bool hasCustomerConsultationAuthority = hasCustomerVisitAuthority &&
                                                    stockFlow.Session.CustomerConsultations != null &&
                                                    !stockFlow.Session.PrototypeCustomerConsultationId.IsEmpty;
            bool hasCustomerBuyActionAuthority = hasArrivedStockFlow &&
                                                 stockFlow.Session.CustomerOfferActions != null &&
                                                 stockFlow.Session.CustomerOfferActions.Count == 0;
            bool hasCustomerLeaveActionAuthority = hasCustomerBuyActionAuthority &&
                                                   !stockFlow.Session.PrototypeCustomerLeaveActionId.IsEmpty &&
                                                   stockFlow.Session.PrototypeCustomerLeaveActionId !=
                                                   stockFlow.Session.PrototypeCustomerBuyActionId;
            bool hasCustomerNavigation = customerFlow != null &&
                                         customerFlow.NavigationReady &&
                                         customerFlow.CustomerAgent != null;
            bool hasPhysicalCheckoutStation = checkoutStation != null &&
                                              checkoutStation.InteractionCollider != null &&
                                              checkoutStation.StationStatusText != null &&
                                              checkoutStation.StationIdValue ==
                                                  CheckoutStationProjection.PrototypeStationIdValue;
            GarageStockFlowSession assemblySession = stockFlow?.Session;
            bool hasMotherboardSeat = motherboardSeat != null &&
                                      motherboardSeat.IsConfigured;
            bool hasMotherboardFastener = motherboardFastener != null &&
                                          motherboardFastener.IsConfigured &&
                                          motherboardFastener.FastenerIdValue ==
                                              GarageStockFlowSession.MotherboardFastenerIdValue &&
                                          motherboardFastener.Screwdriver != null &&
                                          motherboardFastener.StatusText != null &&
                                          motherboardFastener.MatchesAuthorityState(
                                              AssemblySeatState.Empty);
            bool hasMotherboardIdentity = assemblySession != null &&
                                          motherboardBinding != null &&
                                          motherboardBinding.PhysicalItem != null &&
                                          motherboardBinding.InventoryItemIdValue ==
                                              assemblySession.MotherboardItemId.Value &&
                                          motherboardBinding.PhysicalItem.ItemIdValue ==
                                              assemblySession.MotherboardItemId.Value &&
                                          assemblySession.Inventory.SerializedItemCount == 7 &&
                                          assemblySession.TryGetMotherboardItem(
                                              out InventoryItemRecord motherboardItem) &&
                                          motherboardItem.Id == assemblySession.MotherboardItemId &&
                                          motherboardItem.ProductId ==
                                              assemblySession.MotherboardProductId &&
                                          motherboardItem.ContainerId ==
                                              assemblySession.WorldFloorContainerId &&
                                          CountCanonicalMotherboardProjections(
                                              assemblySession.MotherboardItemId.Value) == 1 &&
                                          motherboardBinding.ValidateProjectionInvariant().IsSuccess;
            bool hasMotherboardAssembly = hasMotherboardSeat &&
                                          hasMotherboardFastener &&
                                          motherboardBinding != null &&
                                          motherboardBinding.Runtime == stockFlow &&
                                          motherboardBinding.Seat == motherboardSeat &&
                                          motherboardBinding.Fastener == motherboardFastener &&
                                          motherboardBinding.PhysicalItem != null &&
                                          motherboardBinding.PhysicalItem.CarryProfile ==
                                              PhysicalCarryProfile.PcComponent &&
                                          assemblySession != null &&
                                          assemblySession.AssemblyBuild.MotherboardSeatState ==
                                              AssemblySeatState.Empty &&
                                          assemblySession.AssemblyBuild.Revision == 0 &&
                                          assemblySession.AssemblyBuild.ReceiptCount == 0 &&
                                          assemblySession.AssemblyBuild.ValidateInvariants().IsSuccess &&
                                          hasMotherboardIdentity;
            bool hasProcessorSocket = processorSocket != null &&
                                      processorSocket.IsConfigured &&
                                      processorSocket.SlotIdValue ==
                                          GarageStockFlowSession.ProcessorSlotIdValue &&
                                      processorSocket.RetentionIdValue ==
                                          GarageStockFlowSession.ProcessorRetentionIdValue &&
                                      processorSocket.MatchesAuthorityState(
                                          AssemblySeatState.Empty,
                                          ProcessorSocketState.EmptyOpen);
            bool hasProcessorIdentity = assemblySession != null &&
                                        processorBinding != null &&
                                        processor != null &&
                                        processorBinding.PhysicalItem == processor &&
                                        processorBinding.InventoryItemIdValue ==
                                            assemblySession.ProcessorItemId.Value &&
                                        processor.ItemIdValue ==
                                            assemblySession.ProcessorItemId.Value &&
                                        assemblySession.TryGetProcessorItem(
                                            out InventoryItemRecord processorItem) &&
                                        processorItem.Id == assemblySession.ProcessorItemId &&
                                        processorItem.ProductId ==
                                            assemblySession.ProcessorProductId &&
                                        processorItem.ContainerId ==
                                            assemblySession.WorldFloorContainerId &&
                                        CountCanonicalProcessorProjections(
                                            assemblySession.ProcessorItemId.Value) == 1 &&
                                        processorBinding.ValidateProjectionInvariant().IsSuccess;
            bool hasProcessorAssembly = hasProcessorSocket &&
                                        hasProcessorIdentity &&
                                        processorBinding.Runtime == stockFlow &&
                                        processorBinding.Socket == processorSocket &&
                                        processor.CarryProfile ==
                                            PhysicalCarryProfile.PcComponent &&
                                        assemblySession != null &&
                                        assemblySession.AssemblyBuild.HasProcessorSocket &&
                                        assemblySession.AssemblyBuild.ProcessorSocketState ==
                                            ProcessorSocketState.EmptyOpen;
            bool hasDimmSlot = assemblySession != null &&
                               dimmSlot != null &&
                               dimmSlot.IsConfigured &&
                               dimmSlot.SlotIdValue ==
                                   GarageStockFlowSession.MemorySlotIdValue &&
                               dimmSlot.RetentionIdValue ==
                                   GarageStockFlowSession.MemoryRetentionIdValue &&
                               dimmSlot.ChannelIdValue ==
                                   GarageStockFlowSession.MemoryChannelIdValue &&
                               dimmSlot.BankIdValue ==
                                   GarageStockFlowSession.MemoryBankIdValue &&
                               dimmSlot.MatchesAuthorityState(
                                   assemblySession.AssemblyBuild.MotherboardSeatState,
                                   assemblySession.AssemblyBuild.MemorySlotState);
            bool hasMemoryIdentity = assemblySession != null &&
                                     dimmBinding != null &&
                                     memoryModule != null &&
                                     dimmBinding.Runtime == stockFlow &&
                                     dimmBinding.PhysicalItem == memoryModule &&
                                     dimmBinding.InventoryItemIdValue ==
                                         assemblySession.MemoryItemId.Value &&
                                     memoryModule.ItemIdValue ==
                                         assemblySession.MemoryItemId.Value &&
                                     assemblySession.TryGetMemoryItem(
                                         out InventoryItemRecord memoryItem) &&
                                     memoryItem.Id == assemblySession.MemoryItemId &&
                                     memoryItem.ProductId == assemblySession.MemoryProductId &&
                                     memoryItem.ContainerId ==
                                         assemblySession.WorldFloorContainerId &&
                                     CountCanonicalMemoryProjections(
                                         assemblySession.MemoryItemId.Value) == 1 &&
                                     dimmBinding.ValidateProjectionInvariant().IsSuccess;
            bool hasMemoryAssembly = hasDimmSlot &&
                                     hasMemoryIdentity &&
                                     dimmBinding.Slot == dimmSlot &&
                                     playerCarry != null &&
                                     playerCarry.MatchesDimmConfiguration(
                                         dimmSlot,
                                         dimmBinding) &&
                                     memoryModule.CarryProfile ==
                                         PhysicalCarryProfile.PcComponent &&
                                     assemblySession != null &&
                                     assemblySession.AssemblyBuild.HasMemorySlot &&
                                     assemblySession.AssemblyBuild.MemorySlotState ==
                                         MemorySlotState.EmptyOpen;
            bool hasStorageSlot = assemblySession != null &&
                                  storageSlot != null &&
                                  storageSlot.IsConfigured &&
                                  storageSlot.SlotIdValue ==
                                      GarageStockFlowSession.StorageSlotIdValue &&
                                  storageSlot.StandoffIdValue ==
                                      GarageStockFlowSession.StorageStandoffIdValue &&
                                  storageSlot.CaptiveScrewIdValue ==
                                      GarageStockFlowSession.StorageCaptiveScrewIdValue &&
                                  storageSlot.MatchesLogicalAuthorityState(
                                      assemblySession.AssemblyBuild.MotherboardSeatState,
                                      assemblySession.AssemblyBuild.StorageSlotState);
            bool hasStorageIdentity = assemblySession != null &&
                                      storageBinding != null &&
                                      storageDevice != null &&
                                      storageBinding.Runtime == stockFlow &&
                                      storageBinding.PhysicalItem == storageDevice &&
                                      storageBinding.InventoryItemIdValue ==
                                          assemblySession.StorageItemId.Value &&
                                      storageDevice.ItemIdValue ==
                                          assemblySession.StorageItemId.Value &&
                                      assemblySession.TryGetStorageItem(
                                          out InventoryItemRecord storageItem) &&
                                      storageItem.Id == assemblySession.StorageItemId &&
                                      storageItem.ProductId ==
                                          assemblySession.StorageProductId &&
                                      storageItem.ContainerId ==
                                          assemblySession.WorldFloorContainerId &&
                                      CountCanonicalStorageProjections(
                                          assemblySession.StorageItemId.Value) == 1 &&
                                      storageBinding.ValidateProjectionInvariant().IsSuccess;
            bool hasStorageAssembly = hasStorageSlot &&
                                      hasStorageIdentity &&
                                      storageBinding.Slot == storageSlot &&
                                      playerCarry != null &&
                                      playerCarry.MatchesM2StorageConfiguration(
                                          storageSlot,
                                          storageBinding) &&
                                      storageDevice.CarryProfile ==
                                          PhysicalCarryProfile.PcComponent &&
                                      assemblySession.AssemblyBuild.HasStorageSlot &&
                                      assemblySession.AssemblyBuild.StorageSlotState ==
                                          StorageSlotState.EmptyOpen;
            bool hasProcessorCoolerSlot = assemblySession != null &&
                                          processorCoolerSlot != null &&
                                          processorCoolerSlot.IsConfigured &&
                                          processorCoolerSlot.SlotIdValue ==
                                              GarageStockFlowSession
                                                  .ProcessorCoolerSlotIdValue &&
                                          processorCoolerSlot.BracketIdValue ==
                                              GarageStockFlowSession
                                                  .ProcessorCoolerBracketIdValue &&
                                          processorCoolerSlot.RetentionPointIdValues.Length == 4 &&
                                          processorCoolerSlot.RetentionPointIdValues[0] ==
                                              GarageStockFlowSession
                                                  .ProcessorCoolerRetentionPoint1IdValue &&
                                          processorCoolerSlot.RetentionPointIdValues[1] ==
                                              GarageStockFlowSession
                                                  .ProcessorCoolerRetentionPoint2IdValue &&
                                          processorCoolerSlot.RetentionPointIdValues[2] ==
                                              GarageStockFlowSession
                                                  .ProcessorCoolerRetentionPoint3IdValue &&
                                          processorCoolerSlot.RetentionPointIdValues[3] ==
                                              GarageStockFlowSession
                                                  .ProcessorCoolerRetentionPoint4IdValue &&
                                          memoryModule != null &&
                                          processorCoolerSlot.ClearanceBlockers.Length == 1 &&
                                          processorCoolerSlot.ClearanceBlockers[0] ==
                                              memoryModule.GetComponent<Collider>() &&
                                          processorCoolerSlot.MatchesLogicalAuthorityState(
                                              assemblySession.AssemblyBuild
                                                  .MotherboardSeatState,
                                              assemblySession.AssemblyBuild
                                                  .ProcessorSocketState,
                                              assemblySession.AssemblyBuild
                                                  .ProcessorCoolerSlotState);
            bool hasProcessorCoolerIdentity = assemblySession != null &&
                                              processorCoolerBinding != null &&
                                              processorCooler != null &&
                                              processorCoolerBinding.Runtime == stockFlow &&
                                              processorCoolerBinding.PhysicalItem ==
                                                  processorCooler &&
                                              processorCoolerBinding.InventoryItemIdValue ==
                                                  assemblySession.ProcessorCoolerItemId.Value &&
                                              processorCooler.ItemIdValue ==
                                                  assemblySession.ProcessorCoolerItemId.Value &&
                                              assemblySession.TryGetProcessorCoolerItem(
                                                  out InventoryItemRecord coolerItem) &&
                                              coolerItem.Id ==
                                                  assemblySession.ProcessorCoolerItemId &&
                                              coolerItem.ProductId ==
                                                  assemblySession.ProcessorCoolerProductId &&
                                              coolerItem.ContainerId ==
                                                  assemblySession.WorldFloorContainerId &&
                                              coolerItem.StateFlags ==
                                                  InventorySerializedItemStateFlags.None &&
                                              CountCanonicalProcessorCoolerProjections(
                                                  assemblySession.ProcessorCoolerItemId.Value) ==
                                                  1 &&
                                              processorCoolerBinding
                                                  .ValidateProjectionInvariant().IsSuccess;
            bool hasProcessorCoolerAssembly = hasProcessorCoolerSlot &&
                                              hasProcessorCoolerIdentity &&
                                              processorCoolerBinding.Slot ==
                                                  processorCoolerSlot &&
                                              playerCarry != null &&
                                              playerCarry
                                                  .MatchesProcessorCoolerConfiguration(
                                                      processorCoolerSlot,
                                                      processorCoolerBinding) &&
                                              processorCooler.CarryProfile ==
                                                  PhysicalCarryProfile.PcComponent &&
                                              assemblySession.AssemblyBuild
                                                  .HasProcessorCoolerSlot &&
                                              assemblySession.AssemblyBuild
                                                      .ProcessorCoolerSlotState ==
                                                  ProcessorCoolerSlotState.EmptyOpen &&
                                              HasProcessorCoolerR27Runtime;
            bool hasGraphicsCardSlot = assemblySession != null &&
                                       graphicsCardSlot != null &&
                                       graphicsCardSlot.IsConfigured &&
                                       graphicsCardSlot.SlotIdValue ==
                                           GarageStockFlowSession.GraphicsCardSlotIdValue &&
                                       graphicsCardSlot.LatchIdValue ==
                                           GarageStockFlowSession.GraphicsCardLatchIdValue &&
                                       graphicsCardSlot.RearBracketIdValue ==
                                           GarageStockFlowSession.GraphicsCardRearBracketIdValue &&
                                       graphicsCardSlot.RearBracketFastenerIdValue ==
                                           GarageStockFlowSession
                                               .GraphicsCardBracketFastenerIdValue &&
                                       graphicsCardSlot.MatchesLogicalAuthorityState(
                                           false,
                                           GraphicsCardSlotProjectionState.EmptyOpen);
            bool hasGraphicsCardIdentity = assemblySession != null &&
                                           graphicsCardBinding != null &&
                                           graphicsCard != null &&
                                           graphicsCardBinding.Runtime == stockFlow &&
                                           graphicsCardBinding.PhysicalItem == graphicsCard &&
                                           graphicsCardBinding.InventoryItemIdValue ==
                                               assemblySession
                                                   .GraphicsCardAssemblyItemId.Value &&
                                           graphicsCard.ItemIdValue ==
                                               assemblySession
                                                   .GraphicsCardAssemblyItemId.Value &&
                                           assemblySession.TryGetGraphicsCardAssemblyItem(
                                               out InventoryItemRecord graphicsCardItem) &&
                                           graphicsCardItem.Id ==
                                               assemblySession.GraphicsCardAssemblyItemId &&
                                           graphicsCardItem.ProductId ==
                                               assemblySession.ProductId &&
                                           graphicsCardItem.ContainerId ==
                                               assemblySession.WorldFloorContainerId &&
                                           CountCanonicalGraphicsCardProjections(
                                               assemblySession
                                                   .GraphicsCardAssemblyItemId.Value) == 1 &&
                                           graphicsCardBinding
                                               .ValidateProjectionInvariant().IsSuccess;
            bool hasGraphicsCardAssembly = hasGraphicsCardSlot &&
                                           hasGraphicsCardIdentity &&
                                           graphicsCardBinding.Slot == graphicsCardSlot &&
                                           playerCarry != null &&
                                           playerCarry.MatchesGraphicsCardConfiguration(
                                               graphicsCardSlot,
                                               graphicsCardBinding) &&
                                           graphicsCard.CarryProfile ==
                                               PhysicalCarryProfile.PcComponent &&
                                           assemblySession.AssemblyBuild.HasGraphicsCardSlot &&
                                           assemblySession.AssemblyBuild
                                                   .GraphicsCardSlotState ==
                                               GraphicsCardSlotState.EmptyOpen &&
                                           HasGraphicsCardR28Runtime;
            bool hasPowerSupplyBay = assemblySession != null &&
                                     powerSupplyBay != null &&
                                     powerSupplyBay.IsConfigured &&
                                     powerSupplyBay.SlotIdValue ==
                                         GarageStockFlowSession.PowerSupplyBaySlotIdValue &&
                                     powerSupplyBay.RearMountIdValue ==
                                         GarageStockFlowSession.PowerSupplyRearMountIdValue &&
                                     powerSupplyBay.TopLeftFastenerIdValue ==
                                         GarageStockFlowSession
                                             .PowerSupplyTopLeftFastenerIdValue &&
                                     powerSupplyBay.TopRightFastenerIdValue ==
                                         GarageStockFlowSession
                                             .PowerSupplyTopRightFastenerIdValue &&
                                     powerSupplyBay.BottomLeftFastenerIdValue ==
                                         GarageStockFlowSession
                                             .PowerSupplyBottomLeftFastenerIdValue &&
                                     powerSupplyBay.BottomRightFastenerIdValue ==
                                         GarageStockFlowSession
                                             .PowerSupplyBottomRightFastenerIdValue &&
                                     powerSupplyBay.MatchesLogicalAuthorityState(
                                         PowerSupplyBayProjectionState.EmptyOpen);
            bool hasPowerSupplyIdentity = assemblySession != null &&
                                          powerSupplyBinding != null &&
                                          powerSupply != null &&
                                          powerSupplyBinding.Runtime == stockFlow &&
                                          powerSupplyBinding.PhysicalItem == powerSupply &&
                                          powerSupplyBinding.InventoryItemIdValue ==
                                              assemblySession.PowerSupplyItemId.Value &&
                                          powerSupply.ItemIdValue ==
                                              assemblySession.PowerSupplyItemId.Value &&
                                          assemblySession.TryGetPowerSupplyItem(
                                              out InventoryItemRecord powerSupplyItem) &&
                                          powerSupplyItem.Id ==
                                              assemblySession.PowerSupplyItemId &&
                                          powerSupplyItem.ProductId ==
                                              assemblySession.PowerSupplyProductId &&
                                          powerSupplyItem.ContainerId ==
                                              assemblySession.WorldFloorContainerId &&
                                          CountCanonicalPowerSupplyProjections(
                                              assemblySession.PowerSupplyItemId.Value) == 1 &&
                                          powerSupplyBinding
                                              .ValidateProjectionInvariant().IsSuccess;
            bool hasPowerSupplyAssembly = hasPowerSupplyBay &&
                                          hasPowerSupplyIdentity &&
                                          powerSupplyBinding.Slot == powerSupplyBay &&
                                          playerCarry != null &&
                                          playerCarry.MatchesPowerSupplyConfiguration(
                                              powerSupplyBay,
                                              powerSupplyBinding) &&
                                          powerSupply.CarryProfile ==
                                              PhysicalCarryProfile.PcComponent &&
                                          assemblySession.AssemblyBuild.HasPowerSupplyBay &&
                                          assemblySession.AssemblyBuild.PowerSupplyBayState ==
                                              PowerSupplyBayState.EmptyOpen &&
                                          HasPowerSupplyR29Runtime;

            Debug.Log(
                $"GARAGE_GRAYBOX_RUNTIME_READY version={Version} " +
                $"scene={gameObject.scene.name} resolution={Screen.width}x{Screen.height} " +
                $"motor={(playerMotor != null ? "ok" : "missing")} " +
                $"input={(playerInput != null && playerInput.Actions != null ? "ok" : "missing")} " +
                $"carry={(playerCarry != null ? "ok" : "missing")} " +
                $"placement={(playerCarry != null && playerCarry.PlacementPreview != null ? "ok" : "missing")} " +
                $"large-carry={(hasLargeBox ? "ok" : "missing")} " +
                $"rotation={(hasRotationAction && hasRotationSurface ? "ok" : "missing")} " +
                $"stacking={(smallBoxCount >= 2 ? "ok" : "missing")} " +
                $"transport-cart={(transportCart != null ? "ok" : "missing")} " +
                $"inventory-flow={(hasArrivedStockFlow ? "arrived" : "missing")} " +
                $"parcel={(stockFlow?.Parcel != null && stockFlow.Parcel.IsSealed ? "sealed" : "missing")} " +
                $"shelf-offer={(hasShelfOfferAuthority ? "ready" : "missing")} " +
                $"basket-reservation={(hasBasketAuthority ? "ready" : "missing")} " +
                $"checkout-snapshot={(hasCheckoutAuthority ? "ready" : "missing")} " +
                $"checkout-completion={(hasCheckoutCompletionAuthority ? "ready" : "missing")} " +
                $"cash-payment={(hasEconomySettlementAuthority ? "ready" : "missing")} " +
                $"payment-receipt={(hasEconomySettlementAuthority ? "ready" : "missing")} " +
                $"economy-settlement={(hasEconomySettlementAuthority ? "ready" : "missing")} " +
                $"cash-ledger={(hasCashLedgerAuthority ? "ready" : "missing")} " +
                $"customer-visit={(hasCustomerVisitAuthority ? "ready" : "missing")} " +
                $"customer-consultation={(hasCustomerConsultationAuthority ? "ready" : "missing")} " +
                $"consultation-decision-gate={(hasCustomerConsultationAuthority ? "ready" : "missing")} " +
                $"customer-buy-action={(hasCustomerBuyActionAuthority ? "ready" : "missing")} " +
                $"customer-leave-action={(hasCustomerLeaveActionAuthority ? "ready" : "missing")} " +
                $"customer-navmesh={(hasCustomerNavigation ? "ready" : "missing")} " +
                $"checkout-station={(hasPhysicalCheckoutStation ? "ready" : "missing")} " +
                $"assembly={(hasMotherboardAssembly && hasProcessorAssembly && hasMemoryAssembly && hasStorageAssembly && hasProcessorCoolerAssembly && hasGraphicsCardAssembly && hasPowerSupplyAssembly ? "ready" : "missing")} " +
                $"motherboard-seat={(hasMotherboardSeat ? "ready" : "missing")} " +
                $"motherboard-fastener={(hasMotherboardFastener ? "ready" : "missing")} " +
                $"screwdriver={(hasMotherboardFastener ? "ready" : "missing")} " +
                $"motherboard-identity={(hasMotherboardIdentity ? "stable" : "missing")} " +
                $"processor-socket={(hasProcessorSocket ? "ready" : "missing")} " +
                $"processor-retention={(hasProcessorAssembly ? "ready" : "missing")} " +
                $"processor-identity={(hasProcessorIdentity ? "stable" : "missing")} " +
                $"dimm-slot={(hasDimmSlot ? "ready" : "missing")} " +
                $"dimm-dual-latch={(hasMemoryAssembly ? "ready" : "missing")} " +
                $"dimm-identity={(hasMemoryIdentity ? "stable" : "missing")} " +
                $"m2-slot={(hasStorageSlot ? "ready" : "missing")} " +
                $"m2-captive-screw={(hasStorageAssembly ? "ready" : "missing")} " +
                $"m2-identity={(hasStorageIdentity ? "stable" : "missing")} " +
                $"processor-cooler-slot={(hasProcessorCoolerSlot ? "ready" : "missing")} " +
                $"processor-cooler-retention={(hasProcessorCoolerAssembly ? "ready" : "missing")} " +
                $"processor-cooler-identity={(hasProcessorCoolerIdentity ? "stable" : "missing")} " +
                $"graphics-card-slot={(hasGraphicsCardSlot ? "ready" : "missing")} " +
                $"graphics-card-retention={(hasGraphicsCardAssembly ? "ready" : "missing")} " +
                $"graphics-card-identity={(hasGraphicsCardIdentity ? "stable" : "missing")} " +
                $"power-supply-bay={(hasPowerSupplyBay ? "ready" : "missing")} " +
                $"power-supply-four-screw={(hasPowerSupplyAssembly ? "ready" : "missing")} " +
                $"power-supply-identity={(hasPowerSupplyIdentity ? "stable" : "missing")} " +
                $"lookdev={(hasLookdevCorner && hasLookdevVolume && hasTaskLight ? "ok" : "missing")}");

            bool cartSmokeRequested = HasCommandLineArgument("-pse-cart-smoke");
            bool runStockFlowSmoke = HasCommandLineArgument("-pse-stock-flow-smoke");
            bool runCustomerFlowSmoke = HasCommandLineArgument("-pse-customer-flow-smoke");
            bool runAssemblySmoke = HasCommandLineArgument("-pse-assembly-smoke");
            bool runProcessorSmoke = HasCommandLineArgument("-pse-processor-smoke");
            bool runDimmSmoke = HasCommandLineArgument("-pse-dimm-smoke");
            bool runStorageSmoke = HasCommandLineArgument("-pse-storage-smoke");
            bool runProcessorCoolerSmoke =
                HasCommandLineArgument("-pse-cooler-smoke");
            bool runGraphicsCardSmoke =
                HasCommandLineArgument("-pse-gpu-smoke");
            bool runPowerSupplySmoke =
                HasCommandLineArgument("-pse-psu-smoke");
            int smokeCount = (cartSmokeRequested ? 1 : 0) +
                             (runStockFlowSmoke ? 1 : 0) +
                             (runCustomerFlowSmoke ? 1 : 0) +
                             (runAssemblySmoke ? 1 : 0) +
                             (runProcessorSmoke ? 1 : 0) +
                             (runDimmSmoke ? 1 : 0) +
                             (runStorageSmoke ? 1 : 0) +
                             (runProcessorCoolerSmoke ? 1 : 0) +
                             (runGraphicsCardSmoke ? 1 : 0) +
                             (runPowerSupplySmoke ? 1 : 0);
            if (smokeCount > 1)
            {
                Debug.LogError("GARAGE_RUNTIME_SMOKE smoke=failed code=smoke.conflicting-flags");
                return;
            }

            if (cartSmokeRequested && !Debug.isDebugBuild)
            {
                Debug.LogError(
                    "GARAGE_RUNTIME_SMOKE smoke=failed code=smoke.cart-requires-development-build");
                return;
            }

            if (runAssemblySmoke && !Debug.isDebugBuild)
            {
                Debug.LogError(
                    "GARAGE_MOTHERBOARD_ASSEMBLY_RUNTIME_SMOKE " +
                    "assembly-flow=failed code=smoke.assembly-requires-development-build");
                return;
            }

            if (runProcessorSmoke && !Debug.isDebugBuild)
            {
                Debug.LogError(
                    "GARAGE_CPU_SOCKET_RUNTIME_SMOKE " +
                    "cpu-socket-flow=failed code=smoke.processor-requires-development-build");
                return;
            }

            if (runDimmSmoke && !Debug.isDebugBuild)
            {
                Debug.LogError(
                    "GARAGE_DIMM_RUNTIME_SMOKE " +
                    "dimm-flow=failed code=smoke.dimm-requires-development-build");
                return;
            }

            if (runStorageSmoke && !Debug.isDebugBuild)
            {
                Debug.LogError(
                    "GARAGE_STORAGE_RUNTIME_SMOKE " +
                    "storage-flow=failed code=smoke.storage-requires-development-build");
                return;
            }

            if (runProcessorCoolerSmoke && !Debug.isDebugBuild)
            {
                Debug.LogError(
                    "GARAGE_COOLER_RUNTIME_SMOKE " +
                    "cooler-flow=failed code=smoke.cooler-requires-development-build");
                return;
            }

            if (runGraphicsCardSmoke && !Debug.isDebugBuild)
            {
                Debug.LogError(
                    "GARAGE_GRAPHICS_CARD_RUNTIME_SMOKE " +
                    "graphics-card-flow=failed code=smoke.gpu-requires-development-build");
                return;
            }

            if (runPowerSupplySmoke && !Debug.isDebugBuild)
            {
                Debug.LogError(
                    "GARAGE_PSU_RUNTIME_SMOKE " +
                    "psu-flow=failed code=smoke.psu-requires-development-build");
                return;
            }

            if (cartSmokeRequested)
            {
                StartCoroutine(RunTransportCartSmoke());
            }

            if (runStockFlowSmoke)
            {
                Application.runInBackground = true;
                StartCoroutine(RunStockFlowSmoke());
            }

            if (runCustomerFlowSmoke)
            {
                Application.runInBackground = true;
                StartCoroutine(RunCustomerFlowSmoke());
            }

            if (runAssemblySmoke)
            {
                Application.runInBackground = true;
                StartCoroutine(RunMotherboardAssemblySmoke());
            }

            if (runProcessorSmoke)
            {
                Application.runInBackground = true;
                StartCoroutine(RunProcessorSocketSmoke());
            }

            if (runDimmSmoke)
            {
                Application.runInBackground = true;
                StartCoroutine(RunDimmSlotSmoke());
            }

            if (runStorageSmoke)
            {
                Application.runInBackground = true;
                StartCoroutine(RunM2StorageSmoke());
            }

            if (runProcessorCoolerSmoke)
            {
                Application.runInBackground = true;
                StartCoroutine(RunProcessorCoolerSmoke());
            }

            if (runGraphicsCardSmoke)
            {
                Application.runInBackground = true;
                StartCoroutine(RunGraphicsCardSmoke());
            }

            if (runPowerSupplySmoke)
            {
                Application.runInBackground = true;
                StartCoroutine(RunPowerSupplySmoke());
            }
        }

        private IEnumerator RunStockFlowSmoke()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return null;

            GarageStockFlowSession session = stockFlow != null
                ? stockFlow.EnsureInitialized()
                : null;
            InventoryItemWorldBinding binding = stockFlow != null
                ? stockFlow.ItemBinding
                : null;
            DeliveryParcelProjection parcel = binding != null ? binding.Parcel : null;
            PhysicalItemProjection item = binding != null ? binding.Projection : null;
            if (playerMotor == null || playerCarry == null || session == null || item == null || parcel == null)
            {
                Debug.LogError(
                    "GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=failed code=smoke.context-missing");
                yield break;
            }

            if (session.Order.Status != PCShopEmpire3D.Orders.PurchaseOrderStatus.Arrived ||
                session.TryGetItem(out _) ||
                !session.TryGetGraphicsCardAssemblyItem(
                    out InventoryItemRecord initialAssemblyGraphicsCard) ||
                initialAssemblyGraphicsCard.Id !=
                    session.GraphicsCardAssemblyItemId ||
                initialAssemblyGraphicsCard.ProductId != session.ProductId ||
                initialAssemblyGraphicsCard.ContainerId !=
                    session.WorldFloorContainerId ||
                session.Inventory.GetTotalQuantity(session.ProductId).Value != 1 ||
                !parcel.IsSealed)
            {
                Debug.LogError(
                    "GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=failed code=smoke.arrival-contract");
                yield break;
            }

            OperationResult accept = playerCarry.TryPickup(item);
            if (accept.IsFailure || playerCarry.HeldItem != null)
            {
                Debug.LogError(
                    $"GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=failed " +
                    $"code={(accept.IsFailure ? accept.Error.Code : "smoke.accept-carried")}");
                yield break;
            }

            long inventoryRevisionBeforeOpen = session.Inventory.Revision;
            long orderRevisionBeforeOpen = session.Orders.Revision;
            OperationResult open = playerCarry.TryPickup(item);
            OperationResult repeatedOpen = binding.TryOpenParcel();
            if (open.IsFailure || repeatedOpen.IsFailure || playerCarry.HeldItem != null ||
                !parcel.IsOpened || parcel.OpenTransitionCount != 1 ||
                session.Inventory.Revision != inventoryRevisionBeforeOpen ||
                session.Orders.Revision != orderRevisionBeforeOpen)
            {
                string parcelFailureCode = open.IsFailure
                    ? open.Error.Code
                    : repeatedOpen.IsFailure
                        ? repeatedOpen.Error.Code
                        : "smoke.parcel-contract";
                Debug.LogError(
                    $"GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=failed " +
                    $"code={parcelFailureCode}");
                yield break;
            }

            OperationResult pickup = playerCarry.TryPickup(item);
            if (pickup.IsFailure || playerCarry.HeldItem != item)
            {
                Debug.LogError(
                    $"GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=failed " +
                    $"code={(pickup.IsFailure ? pickup.Error.Code : "smoke.pickup-missing")}");
                yield break;
            }

            SetPlayerPose(new Vector3(0f, 0.05f, -2.5f), Quaternion.identity);
            OperationResult drop = playerCarry.TryDrop();
            bool validInventory = session.TryGetItem(out PCShopEmpire3D.Inventory.InventoryItemRecord record) &&
                                  record.Id == session.ItemId &&
                                  record.ContainerId == session.WorldFloorContainerId &&
                                  session.TryGetGraphicsCardAssemblyItem(
                                      out InventoryItemRecord assemblyGraphicsCard) &&
                                  assemblyGraphicsCard.Id ==
                                      session.GraphicsCardAssemblyItemId &&
                                  assemblyGraphicsCard.ProductId == session.ProductId &&
                                  assemblyGraphicsCard.ContainerId ==
                                      session.WorldFloorContainerId &&
                                  session.Inventory.GetTotalQuantity(session.ProductId).Value == 2;
            if (drop.IsFailure || playerCarry.HeldItem != null || !validInventory)
            {
                Debug.LogError(
                    $"GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=failed " +
                    $"code={(drop.IsFailure ? drop.Error.Code : "smoke.inventory-mismatch")}");
                yield break;
            }

            long inventoryRevisionBeforeOffer = session.Inventory.Revision;
            long orderRevisionBeforeOffer = session.Orders.Revision;
            long retailRevisionBeforeOffer = session.RetailOffers.Revision;
            OperationResult publishOffer = session.PublishShelfOffer();
            OperationResult repeatedOffer = session.PublishShelfOffer();
            stockFlow.RefreshPresentation();
            PCShopEmpire3D.Retail.ShelfOfferRecord offer = null;
            bool validOffer = publishOffer.IsSuccess &&
                              repeatedOffer.IsSuccess &&
                              session.TryGetShelfOffer(out offer) &&
                              offer.Id == session.ShelfOfferId &&
                              offer.Price.MinorUnits == GarageStockFlowSession.PrototypePriceMinorUnits &&
                              offer.Price.Currency.Value == GarageStockFlowSession.PrototypeCurrencyCode &&
                              session.RetailOffers.Revision == retailRevisionBeforeOffer + 1 &&
                              session.Inventory.Revision == inventoryRevisionBeforeOffer &&
                              session.Orders.Revision == orderRevisionBeforeOffer &&
                              stockFlow.ShelfOfferText != null &&
                              stockFlow.ShelfOfferText.text == stockFlow.ShelfOfferLabelText;
            if (!validOffer)
            {
                Debug.LogError(
                    $"GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=failed " +
                    $"code={(publishOffer.IsFailure ? publishOffer.Error.Code : "smoke.shelf-offer-contract")}");
                yield break;
            }

            GarageStockFlowSession basketSession = GarageStockFlowSession.CreateArrived();
            OperationResult basketAccept = basketSession.AcceptArrivedDelivery();
            OperationResult basketShelfTransfer = basketSession.TransferItem(
                basketSession.ShelfContainerId);
            OperationResult basketOffer = basketSession.PublishShelfOffer();
            long basketInventoryBefore = basketSession.Inventory.Revision;
            long basketRetailBefore = basketSession.RetailBaskets.Revision;
            long basketOffersBefore = basketSession.RetailOffers.Revision;
            long basketOrdersBefore = basketSession.Orders.Revision;
            OperationResult basketReserve = basketSession.ReservePrototypeCustomerBasket();
            OperationResult basketRepeat = basketSession.ReservePrototypeCustomerBasket();
            bool basketReserved =
                basketAccept.IsSuccess &&
                basketShelfTransfer.IsSuccess &&
                basketOffer.IsSuccess &&
                basketReserve.IsSuccess &&
                basketRepeat.IsSuccess &&
                basketSession.TryGetPrototypeBasketLine(out var basketLine) &&
                basketLine.ItemId == basketSession.ItemId &&
                basketLine.OfferId == basketSession.ShelfOfferId &&
                basketLine.CustomerId == basketSession.PrototypeCustomerId &&
                basketSession.Inventory.TryGetReservation(
                    basketSession.PrototypeReservationId,
                    out InventoryReservation reservation) &&
                reservation.ItemId == basketSession.ItemId &&
                reservation.ClaimId == basketSession.PrototypeClaimId &&
                basketSession.Inventory.GetAvailableQuantity(basketSession.ProductId).Value == 0 &&
                basketSession.Inventory.GetTotalQuantity(basketSession.ProductId).Value == 1 &&
                basketSession.Inventory.Revision == basketInventoryBefore + 1 &&
                basketSession.RetailBaskets.Revision == basketRetailBefore + 1 &&
                basketSession.RetailOffers.Revision == basketOffersBefore &&
                basketSession.Orders.Revision == basketOrdersBefore &&
                basketSession.ValidateInvariants().IsSuccess;
            if (!basketReserved)
            {
                Debug.LogError(
                    $"GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=failed " +
                    $"code={(basketReserve.IsFailure ? basketReserve.Error.Code : "smoke.basket-reservation-contract")}");
                yield break;
            }

            long inventoryBeforeRelease = basketSession.Inventory.Revision;
            long retailBeforeRelease = basketSession.RetailBaskets.Revision;
            OperationResult basketRelease = basketSession.ReleasePrototypeCustomerBasket();
            bool basketReleased = basketRelease.IsSuccess &&
                                  basketSession.RetailBaskets.Count == 0 &&
                                  basketSession.Inventory.ReservationCount == 0 &&
                                  basketSession.Inventory.GetAvailableQuantity(
                                      basketSession.ProductId).Value == 1 &&
                                  basketSession.Inventory.GetTotalQuantity(
                                      basketSession.ProductId).Value == 1 &&
                                  basketSession.Inventory.Revision == inventoryBeforeRelease + 1 &&
                                  basketSession.RetailBaskets.Revision == retailBeforeRelease + 1 &&
                                  basketSession.ValidateInvariants().IsSuccess;
            if (!basketReleased)
            {
                Debug.LogError(
                    $"GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=failed " +
                    $"code={(basketRelease.IsFailure ? basketRelease.Error.Code : "smoke.basket-release-contract")}");
                yield break;
            }

            GarageStockFlowSession checkoutSession = GarageStockFlowSession.CreateArrived();
            OperationResult checkoutAccept = checkoutSession.AcceptArrivedDelivery();
            OperationResult checkoutShelfTransfer = checkoutSession.TransferItem(
                checkoutSession.ShelfContainerId);
            OperationResult checkoutOffer = checkoutSession.PublishShelfOffer();
            OperationResult checkoutReserve = checkoutSession.ReservePrototypeCustomerBasket();
            long checkoutInventoryBefore = checkoutSession.Inventory.Revision;
            long checkoutBasketBefore = checkoutSession.RetailBaskets.Revision;
            long checkoutOffersBefore = checkoutSession.RetailOffers.Revision;
            long checkoutOrdersBefore = checkoutSession.Orders.Revision;
            long checkoutRevisionBefore = checkoutSession.RetailCheckouts.Revision;
            OperationResult checkoutBegin = checkoutSession.BeginPrototypeCheckout();
            OperationResult checkoutRepeat = checkoutSession.BeginPrototypeCheckout();
            bool snapshotCreated =
                checkoutAccept.IsSuccess &&
                checkoutShelfTransfer.IsSuccess &&
                checkoutOffer.IsSuccess &&
                checkoutReserve.IsSuccess &&
                checkoutBegin.IsSuccess &&
                checkoutRepeat.IsSuccess &&
                checkoutSession.TryGetPrototypeCheckout(out var checkoutRecord) &&
                checkoutRecord.BasketId == checkoutSession.PrototypeBasketId &&
                checkoutRecord.CustomerId == checkoutSession.PrototypeCustomerId &&
                checkoutRecord.Currency.Value == GarageStockFlowSession.PrototypeCurrencyCode &&
                checkoutRecord.TotalMinorUnits ==
                    GarageStockFlowSession.PrototypePriceMinorUnits &&
                checkoutRecord.Lines.Count == 1 &&
                checkoutRecord.Lines[0].ItemId == checkoutSession.ItemId &&
                checkoutRecord.Lines[0].OfferId == checkoutSession.ShelfOfferId &&
                checkoutRecord.Lines[0].UnitPrice.MinorUnits ==
                    GarageStockFlowSession.PrototypePriceMinorUnits &&
                checkoutRecord.Lines[0].SourceOfferRevision == 1 &&
                checkoutSession.RetailCheckouts.Revision == checkoutRevisionBefore + 1 &&
                checkoutSession.Inventory.Revision == checkoutInventoryBefore &&
                checkoutSession.RetailBaskets.Revision == checkoutBasketBefore &&
                checkoutSession.RetailOffers.Revision == checkoutOffersBefore &&
                checkoutSession.Orders.Revision == checkoutOrdersBefore;
            if (!snapshotCreated)
            {
                Debug.LogError(
                    $"GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=failed " +
                    $"code={(checkoutBegin.IsFailure ? checkoutBegin.Error.Code : "smoke.checkout-contract")}");
                yield break;
            }

            const long updatedPriceMinorUnits = 59_999;
            OperationResult updatePrice = checkoutSession.RetailOffers.SetOffer(
                checkoutSession.ShelfOfferId,
                checkoutSession.ProductId,
                checkoutSession.ShelfContainerId,
                GarageStockFlowSession.PrototypeCurrencyCode,
                updatedPriceMinorUnits);
            OperationResult repeatAfterPriceChange = checkoutSession.BeginPrototypeCheckout();
            bool priceFrozen =
                updatePrice.IsSuccess &&
                repeatAfterPriceChange.IsSuccess &&
                checkoutSession.TryGetPrototypeCheckout(out checkoutRecord) &&
                checkoutSession.TryGetShelfOffer(out var updatedOffer) &&
                checkoutRecord.TotalMinorUnits ==
                    GarageStockFlowSession.PrototypePriceMinorUnits &&
                checkoutRecord.Lines[0].UnitPrice.MinorUnits ==
                    GarageStockFlowSession.PrototypePriceMinorUnits &&
                checkoutRecord.Lines[0].SourceOfferRevision == 1 &&
                updatedOffer.Price.MinorUnits == updatedPriceMinorUnits &&
                updatedOffer.OfferRevision == 2 &&
                checkoutSession.RetailCheckouts.Revision == checkoutRevisionBefore + 1 &&
                checkoutSession.Inventory.Revision == checkoutInventoryBefore &&
                checkoutSession.RetailBaskets.Revision == checkoutBasketBefore &&
                checkoutSession.RetailOffers.Revision == checkoutOffersBefore + 1 &&
                checkoutSession.Orders.Revision == checkoutOrdersBefore &&
                checkoutSession.ValidateInvariants().IsSuccess;
            if (!priceFrozen)
            {
                Debug.LogError(
                    $"GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=failed " +
                    $"code={(repeatAfterPriceChange.IsFailure ? repeatAfterPriceChange.Error.Code : "smoke.checkout-price-drift")}");
                yield break;
            }

            long settlementInventoryBefore = checkoutSession.Inventory.Revision;
            long settlementBasketBefore = checkoutSession.RetailBaskets.Revision;
            long settlementCheckoutBefore = checkoutSession.RetailCheckouts.Revision;
            long settlementOfferBefore = checkoutSession.RetailOffers.Revision;
            long settlementOrdersBefore = checkoutSession.Orders.Revision;
            long settlementEconomyBefore = checkoutSession.CheckoutSettlements.Revision;
            OperationResult settleCash = checkoutSession.SettlePrototypeCashCheckout();
            OperationResult repeatedSettlement = checkoutSession.SettlePrototypeCashCheckout();
            OperationResult conflictingSettlement =
                checkoutSession.CheckoutSettlements.SettleCashCheckout(
                    checkoutSession.PrototypeCheckoutSettlementId,
                    StableId<EconomyLedgerTransactionIdScope>.Parse(
                        "economy.ledger-transaction.smoke-conflict"),
                    checkoutSession.PrototypeCheckoutCompletionId,
                    checkoutSession.PrototypeCheckoutId,
                    GarageStockFlowSession.PrototypeCurrencyCode,
                    GarageStockFlowSession.PrototypePriceMinorUnits,
                    SimulationTimestamp.Create(7, 7_000L));
            OperationResult repeatedBeginAfterCompletion = checkoutSession.BeginPrototypeCheckout();
            CurrencyCode settlementCurrency = CurrencyCode.Create(
                GarageStockFlowSession.PrototypeCurrencyCode).Value;
            OperationResult<long> cashDelta = checkoutSession.CheckoutSettlements.GetAccountDelta(
                EconomyAccountKind.Cash,
                settlementCurrency);
            OperationResult<long> revenueDelta = checkoutSession.CheckoutSettlements.GetAccountDelta(
                EconomyAccountKind.SalesRevenue,
                settlementCurrency);
            OperationResult<long> cogsDelta = checkoutSession.CheckoutSettlements.GetAccountDelta(
                EconomyAccountKind.CostOfGoodsSold,
                settlementCurrency);
            OperationResult<long> inventoryAssetDelta =
                checkoutSession.CheckoutSettlements.GetAccountDelta(
                    EconomyAccountKind.InventoryAsset,
                    settlementCurrency);
            bool saleSettled =
                settleCash.IsSuccess &&
                repeatedSettlement.IsSuccess &&
                conflictingSettlement.Error ==
                    CheckoutSettlementFailures.SettlementIdentityConflict &&
                repeatedBeginAfterCompletion.IsSuccess &&
                checkoutSession.TryGetPrototypeCheckoutCompletion(
                    out RetailCheckoutCompletionRecord completionRecord) &&
                checkoutSession.CheckoutSettlements.TryGetSettlement(
                    checkoutSession.PrototypeCheckoutSettlementId,
                    out CheckoutSettlementReceipt settlementReceipt) &&
                checkoutSession.TryGetPrototypeLedgerTransaction(
                    out EconomyLedgerTransactionRecord ledgerTransaction) &&
                completionRecord.CheckoutId == checkoutSession.PrototypeCheckoutId &&
                completionRecord.BasketId == checkoutSession.PrototypeBasketId &&
                completionRecord.CustomerId == checkoutSession.PrototypeCustomerId &&
                completionRecord.Currency.Value ==
                    GarageStockFlowSession.PrototypeCurrencyCode &&
                completionRecord.TotalMinorUnits ==
                    GarageStockFlowSession.PrototypePriceMinorUnits &&
                completionRecord.Lines.Count == 1 &&
                completionRecord.Lines[0].ItemId == checkoutSession.ItemId &&
                settlementReceipt.Id == checkoutSession.PrototypeCheckoutSettlementId &&
                settlementReceipt.TransactionId == checkoutSession.PrototypeLedgerTransactionId &&
                settlementReceipt.CompletionId == completionRecord.Id &&
                settlementReceipt.CheckoutId == completionRecord.CheckoutId &&
                settlementReceipt.CustomerId == completionRecord.CustomerId &&
                settlementReceipt.PaymentMethod == CheckoutPaymentMethod.Cash &&
                settlementReceipt.Currency == completionRecord.Currency &&
                settlementReceipt.GrossMinorUnits ==
                    GarageStockFlowSession.PrototypePriceMinorUnits &&
                settlementReceipt.CostOfGoodsSoldMinorUnits ==
                    GarageStockFlowSession.PrototypeUnitCostMinorUnits &&
                settlementReceipt.GrossMarginMinorUnits ==
                    GarageStockFlowSession.PrototypePriceMinorUnits -
                    GarageStockFlowSession.PrototypeUnitCostMinorUnits &&
                ledgerTransaction.Id == checkoutSession.PrototypeLedgerTransactionId &&
                ledgerTransaction.SettlementId == settlementReceipt.Id &&
                ledgerTransaction.Entries.Count == 4 &&
                ledgerTransaction.Entries[0].Account == EconomyAccountKind.Cash &&
                ledgerTransaction.Entries[0].Direction == EconomyEntryDirection.Debit &&
                ledgerTransaction.Entries[0].MinorUnits ==
                    GarageStockFlowSession.PrototypePriceMinorUnits &&
                ledgerTransaction.Entries[1].Account == EconomyAccountKind.SalesRevenue &&
                ledgerTransaction.Entries[1].Direction == EconomyEntryDirection.Credit &&
                ledgerTransaction.Entries[1].MinorUnits ==
                    GarageStockFlowSession.PrototypePriceMinorUnits &&
                ledgerTransaction.Entries[2].Account == EconomyAccountKind.CostOfGoodsSold &&
                ledgerTransaction.Entries[2].Direction == EconomyEntryDirection.Debit &&
                ledgerTransaction.Entries[2].MinorUnits ==
                    GarageStockFlowSession.PrototypeUnitCostMinorUnits &&
                ledgerTransaction.Entries[3].Account == EconomyAccountKind.InventoryAsset &&
                ledgerTransaction.Entries[3].Direction == EconomyEntryDirection.Credit &&
                ledgerTransaction.Entries[3].MinorUnits ==
                    GarageStockFlowSession.PrototypeUnitCostMinorUnits &&
                ledgerTransaction.Entries[0].MinorUnits +
                    ledgerTransaction.Entries[2].MinorUnits ==
                    ledgerTransaction.Entries[1].MinorUnits +
                    ledgerTransaction.Entries[3].MinorUnits &&
                cashDelta.IsSuccess &&
                cashDelta.Value == GarageStockFlowSession.PrototypePriceMinorUnits &&
                revenueDelta.IsSuccess &&
                revenueDelta.Value == GarageStockFlowSession.PrototypePriceMinorUnits &&
                cogsDelta.IsSuccess &&
                cogsDelta.Value == GarageStockFlowSession.PrototypeUnitCostMinorUnits &&
                inventoryAssetDelta.IsSuccess &&
                inventoryAssetDelta.Value == -GarageStockFlowSession.PrototypeUnitCostMinorUnits &&
                !checkoutSession.TryGetItem(out _) &&
                checkoutSession.Inventory.GetTotalQuantity(
                    checkoutSession.ProductId).Value == 0 &&
                checkoutSession.Inventory.GetAvailableQuantity(
                    checkoutSession.ProductId).Value == 0 &&
                checkoutSession.Inventory.ReservationCount == 0 &&
                checkoutSession.RetailBaskets.Count == 0 &&
                checkoutSession.RetailCheckouts.Count == 1 &&
                checkoutSession.RetailCheckouts.CompletionCount == 1 &&
                checkoutSession.CheckoutSettlements.SettlementCount == 1 &&
                checkoutSession.CheckoutSettlements.TransactionCount == 1 &&
                checkoutSession.Inventory.Revision == settlementInventoryBefore + 1 &&
                checkoutSession.RetailBaskets.Revision == settlementBasketBefore + 1 &&
                checkoutSession.RetailCheckouts.Revision == settlementCheckoutBefore + 1 &&
                checkoutSession.RetailOffers.Revision == settlementOfferBefore &&
                checkoutSession.Orders.Revision == settlementOrdersBefore &&
                checkoutSession.CheckoutSettlements.Revision == settlementEconomyBefore + 1 &&
                checkoutSession.ValidateInvariants().IsSuccess;
            if (!saleSettled)
            {
                string settlementFailureCode = settleCash.IsFailure
                    ? settleCash.Error.Code
                    : repeatedSettlement.IsFailure
                        ? repeatedSettlement.Error.Code
                        : repeatedBeginAfterCompletion.IsFailure
                            ? repeatedBeginAfterCompletion.Error.Code
                            : "smoke.cash-settlement-contract";
                Debug.LogError(
                    $"GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=failed " +
                    $"code={settlementFailureCode}");
                yield break;
            }

            Transform cameraPivot = playerMotor.transform.Find("CameraPivot");
            if (cameraPivot != null)
            {
                cameraPivot.localRotation = Quaternion.Euler(12f, 0f, 0f);
            }

            Debug.Log(
                $"GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=ok accepted=ok parcel-open=ok carry=ok " +
                $"world-floor=ok shelf-offer=ok price-minor={offer.Price.MinorUnits} " +
                $"currency={offer.Price.Currency.Value} " +
                "basket-reservation=ok release=ok " +
                "checkout-snapshot=ok price-frozen=ok " +
                "cash-payment=ok payment-receipt=ok economy-settlement=ok " +
                "cash-ledger=ok revenue=ok cogs=ok inventory-asset=ok ledger-balanced=ok " +
                "payment-replay=ok payment-conflict-blocked=ok stock-consumed=ok " +
                $"stable={(item.ItemIdValue == session.ItemId.Value ? "ok" : "missing")} " +
                $"completed-quantity={checkoutSession.Inventory.GetTotalQuantity(checkoutSession.ProductId).Value} " +
                $"projection-quantity={session.Inventory.GetTotalQuantity(session.ProductId).Value}");
            yield return new WaitForEndOfFrame();
        }

        private IEnumerator RunCustomerFlowSmoke()
        {
            yield return null;
            playerMotor?.SetPaused(true);
            yield return new WaitForFixedUpdate();

            GarageStockFlowSession session = stockFlow != null
                ? stockFlow.EnsureInitialized()
                : null;
            InventoryItemWorldBinding liveBinding = stockFlow != null
                ? stockFlow.ItemBinding
                : null;
            if (playerMotor == null || playerCarry == null || session == null ||
                customerFlow == null || checkoutStation == null ||
                liveBinding == null || liveBinding.Projection == null ||
                checkoutStation.InteractionCollider == null ||
                !customerFlow.NavigationReady || customerFlow.CustomerAgent == null)
            {
                playerMotor?.SetPaused(false);
                LogCustomerFlowSmokeFailure("smoke.context-missing");
                yield break;
            }

            OperationResult accept = session.AcceptArrivedDelivery();
            OperationResult shelfTransfer = session.TransferItem(session.ShelfContainerId);
            OperationResult publishOffer = session.PublishShelfOffer();
            stockFlow.RefreshPresentation();
            if (accept.IsFailure || shelfTransfer.IsFailure || publishOffer.IsFailure ||
                session.Inventory.GetTotalQuantity(session.ProductId).Value != 2 ||
                !session.TryGetItem(out InventoryItemRecord shelfItem) ||
                shelfItem.Id != session.ItemId ||
                shelfItem.ContainerId != session.ShelfContainerId ||
                !session.TryGetGraphicsCardAssemblyItem(
                    out InventoryItemRecord assemblyGraphicsCard) ||
                assemblyGraphicsCard.Id != session.GraphicsCardAssemblyItemId ||
                assemblyGraphicsCard.ProductId != session.ProductId ||
                assemblyGraphicsCard.ContainerId != session.WorldFloorContainerId ||
                !session.TryGetShelfOffer(out _))
            {
                playerMotor.SetPaused(false);
                string code = accept.IsFailure
                    ? accept.Error.Code
                    : shelfTransfer.IsFailure
                        ? shelfTransfer.Error.Code
                        : publishOffer.IsFailure
                            ? publishOffer.Error.Code
                            : "smoke.stock-setup-mismatch";
                LogCustomerFlowSmokeFailure(code);
                yield break;
            }

            long isolatedInventoryRevision = session.Inventory.Revision;
            long isolatedOrderRevision = session.Orders.Revision;
            long isolatedOfferRevision = session.RetailOffers.Revision;
            long isolatedBasketRevision = session.RetailBaskets.Revision;
            long isolatedCheckoutRevision = session.RetailCheckouts.Revision;
            long isolatedEconomyRevision = session.CheckoutSettlements.Revision;
            long isolatedConsultationRevision = session.CustomerConsultations.Revision;
            float customerAgentSpeed = customerFlow.CustomerAgent.speed;
            customerFlow.CustomerAgent.speed = Mathf.Min(customerAgentSpeed, 0.10f);
            playerMotor.SetPaused(false);

            const int routeStepLimit = 900;
            int waitSteps = 0;
            while (!customerFlow.VisitStarted && waitSteps < 100)
            {
                waitSteps++;
                yield return new WaitForFixedUpdate();
            }

            if (!customerFlow.VisitStarted || !customerFlow.CustomerVisible ||
                !session.TryGetPrototypeCustomerVisit(out CustomerVisitRecord enteringVisit) ||
                enteringVisit.State != CustomerVisitState.Entering)
            {
                customerFlow.CustomerAgent.speed = customerAgentSpeed;
                playerMotor.SetPaused(false);
                LogCustomerFlowSmokeFailure("smoke.visit-start-mismatch");
                yield break;
            }

            waitSteps = 0;
            while (!customerFlow.HasAssignedRoute && waitSteps < 100)
            {
                waitSteps++;
                yield return new WaitForFixedUpdate();
            }

            if (!customerFlow.HasAssignedRoute ||
                customerFlow.CustomerAgent.remainingDistance <=
                customerFlow.CustomerAgent.stoppingDistance + 0.10f)
            {
                customerFlow.CustomerAgent.speed = customerAgentSpeed;
                playerMotor.SetPaused(false);
                LogCustomerFlowSmokeFailure("smoke.moving-route-missing");
                yield break;
            }

            playerMotor.SetPaused(true);
            yield return new WaitForFixedUpdate();
            yield return null;
            Vector3 pausedPosition = customerFlow.CustomerAgent.transform.position;
            SimulationTimestamp pausedTime = customerFlow.CurrentSimulationTime;
            for (int step = 0; step < 5; step++)
            {
                yield return new WaitForFixedUpdate();
            }

            bool pauseFrozen = customerFlow.CurrentSimulationTime == pausedTime &&
                               Vector3.Distance(
                                   customerFlow.CustomerAgent.transform.position,
                                   pausedPosition) < 0.001f;
            customerFlow.CustomerAgent.speed = customerAgentSpeed;
            playerMotor.SetPaused(false);
            yield return new WaitForFixedUpdate();
            if (!pauseFrozen)
            {
                LogCustomerFlowSmokeFailure("smoke.pause-drift");
                yield break;
            }

            waitSteps = 0;
            while (waitSteps < routeStepLimit &&
                   session.TryGetPrototypeCustomerVisit(out CustomerVisitRecord browseCandidate) &&
                   browseCandidate.State != CustomerVisitState.Browsing)
            {
                waitSteps++;
                yield return new WaitForFixedUpdate();
            }

            bool browseReached = session.TryGetPrototypeCustomerVisit(out CustomerVisitRecord browsingVisit) &&
                                 browsingVisit.State == CustomerVisitState.Browsing &&
                                 browsingVisit.TotalRouteFailureCount == 0 &&
                                 session.Inventory.Revision == isolatedInventoryRevision &&
                                 session.Orders.Revision == isolatedOrderRevision &&
                                 session.RetailOffers.Revision == isolatedOfferRevision &&
                                 session.RetailBaskets.Revision == isolatedBasketRevision &&
                                 session.RetailCheckouts.Revision == isolatedCheckoutRevision &&
                                 session.CheckoutSettlements.Revision == isolatedEconomyRevision;
            browseReached = browseReached &&
                            session.CustomerConsultations.Revision ==
                                isolatedConsultationRevision;
            if (!browseReached)
            {
                LogCustomerFlowSmokeFailure("smoke.browse-route-or-authority-drift");
                yield break;
            }

            long decisionCustomerRevision = session.CustomerVisits.Revision;
            long decisionInventoryRevision = session.Inventory.Revision;
            long decisionOrderRevision = session.Orders.Revision;
            long decisionOfferRevision = session.RetailOffers.Revision;
            long decisionBasketRevision = session.RetailBaskets.Revision;
            long decisionCheckoutRevision = session.RetailCheckouts.Revision;
            long decisionEconomyRevision = session.CheckoutSettlements.Revision;
            long consultationRevision = session.CustomerConsultations.Revision;
            OperationResult<CustomerOfferDecision> gatedDecision =
                session.EvaluatePrototypeCustomerOffer();
            bool decisionGated = gatedDecision.Error ==
                                 CustomerOfferDecisionFailures.ConsultationRequired &&
                                 customerFlow.CurrentOfferDecision == null &&
                                 session.CustomerConsultations.Revision == consultationRevision &&
                                 session.CustomerVisits.Revision == decisionCustomerRevision &&
                                 session.Inventory.Revision == decisionInventoryRevision &&
                                 session.Orders.Revision == decisionOrderRevision &&
                                 session.RetailOffers.Revision == decisionOfferRevision &&
                                 session.RetailBaskets.Revision == decisionBasketRevision &&
                                 session.RetailCheckouts.Revision == decisionCheckoutRevision &&
                                 session.CheckoutSettlements.Revision == decisionEconomyRevision;
            if (!decisionGated)
            {
                LogCustomerFlowSmokeFailure("smoke.consultation-decision-gate-mismatch");
                yield break;
            }

            OperationResult consultation = session.ConsultPrototypeCustomer(
                customerFlow.CurrentConsultationTime);
            OperationResult consultationReplay = session.ConsultPrototypeCustomer(
                customerFlow.CurrentConsultationTime);
            CustomerConsultationRecord consultationRecord = null;
            bool consultationRecorded = consultation.IsSuccess &&
                                        consultationReplay.IsSuccess &&
                                        session.CustomerConsultations.Revision ==
                                            consultationRevision + 1 &&
                                        session.TryGetPrototypeCustomerConsultation(
                                            out consultationRecord) &&
                                        consultationRecord.VisitId == browsingVisit.Id &&
                                        consultationRecord.IntentId == browsingVisit.Intent.Id &&
                                        consultationRecord.Need == browsingVisit.Intent.Need &&
                                        consultationRecord.ProductId == browsingVisit.Intent.ProductId &&
                                        session.CustomerVisits.Revision == decisionCustomerRevision &&
                                        session.Inventory.Revision == decisionInventoryRevision &&
                                        session.Orders.Revision == decisionOrderRevision &&
                                        session.RetailOffers.Revision == decisionOfferRevision &&
                                        session.RetailBaskets.Revision == decisionBasketRevision &&
                                        session.RetailCheckouts.Revision == decisionCheckoutRevision &&
                                        session.CheckoutSettlements.Revision == decisionEconomyRevision;
            if (!consultationRecorded)
            {
                LogCustomerFlowSmokeFailure(
                    consultation.IsFailure
                        ? consultation.Error.Code
                        : consultationReplay.IsFailure
                            ? consultationReplay.Error.Code
                            : "smoke.consultation-provenance-mismatch");
                yield break;
            }

            OperationResult<CustomerOfferDecision> offerDecisionResult =
                session.EvaluatePrototypeCustomerOffer();
            CustomerOfferDecision displayedDecision = customerFlow.CurrentOfferDecision;
            bool offerDecision = offerDecisionResult.IsSuccess &&
                                 displayedDecision != null &&
                                 displayedDecision.Equals(offerDecisionResult.Value) &&
                                 offerDecisionResult.Value.DecisionKind ==
                                 CustomerOfferDecisionKind.Buy &&
                                 offerDecisionResult.Value.ReasonCode ==
                                 CustomerOfferDecisionReasonCodes.BuyExactProductWithinLimit &&
                                 offerDecisionResult.Value.VisitId == browsingVisit.Id &&
                                 offerDecisionResult.Value.Consultation.Id ==
                                     consultationRecord.Id &&
                                 offerDecisionResult.Value.OfferRevision == 1 &&
                                 offerDecisionResult.Value.OfferPrice.MinorUnits ==
                                 GarageStockFlowSession.PrototypePriceMinorUnits &&
                                 offerDecisionResult.Value.MaximumAcceptedPrice.MinorUnits ==
                                 GarageStockFlowSession.PrototypeMaximumAcceptedPriceMinorUnits &&
                                 customerFlow.StateText.Contains("KARAR: SATIN AL") &&
                                 customerFlow.OfferDecisionReasonCode ==
                                 CustomerOfferDecisionReasonCodes.BuyExactProductWithinLimit &&
                                 session.TryGetPrototypeCustomerVisit(
                                     out CustomerVisitRecord decisionVisit) &&
                                 decisionVisit.State == CustomerVisitState.Browsing &&
                                 session.CustomerVisits.Revision == decisionCustomerRevision &&
                                 session.Inventory.Revision == decisionInventoryRevision &&
                                 session.Orders.Revision == decisionOrderRevision &&
                                 session.RetailOffers.Revision == decisionOfferRevision &&
                                 session.RetailBaskets.Revision == decisionBasketRevision &&
                                 session.RetailCheckouts.Revision == decisionCheckoutRevision &&
                                 session.CheckoutSettlements.Revision == decisionEconomyRevision &&
                                 session.CustomerConsultations.Revision ==
                                     consultationRevision + 1 &&
                                 session.RetailBaskets.Count == 0 &&
                                 session.RetailCheckouts.Count == 0;
            if (!offerDecision)
            {
                LogCustomerFlowSmokeFailure(
                    offerDecisionResult.IsFailure
                        ? offerDecisionResult.Error.Code
                        : "smoke.offer-decision-mismatch");
                yield break;
            }

            long actionRevision = session.CustomerOfferActions.Revision;
            OperationResult buyAction = session.ApplyPrototypeCustomerBuy(
                displayedDecision,
                customerFlow.CurrentOfferActionTime);
            bool buyApplied = buyAction.IsSuccess &&
                              session.CustomerOfferActions.Revision == actionRevision + 1 &&
                              session.CustomerVisits.Revision == decisionCustomerRevision + 1 &&
                              session.Inventory.Revision == decisionInventoryRevision + 1 &&
                              session.RetailBaskets.Revision == decisionBasketRevision + 1 &&
                              session.Orders.Revision == decisionOrderRevision &&
                              session.RetailOffers.Revision == decisionOfferRevision &&
                              session.RetailCheckouts.Revision == decisionCheckoutRevision &&
                              session.CustomerConsultations.Revision ==
                                  consultationRevision + 1 &&
                              session.TryGetPrototypeCustomerBuyAction(out _) &&
                              session.TryGetPrototypeBasketLine(out RetailBasketLineRecord actionLine) &&
                              actionLine.IsActionOwned;
            if (!buyApplied)
            {
                LogCustomerFlowSmokeFailure(
                    buyAction.IsFailure
                        ? buyAction.Error.Code
                        : "smoke.buy-action-mismatch");
                yield break;
            }

            waitSteps = 0;
            while (waitSteps < routeStepLimit &&
                   session.TryGetPrototypeCustomerVisit(out CustomerVisitRecord checkoutCandidate) &&
                   checkoutCandidate.State != CustomerVisitState.AwaitingCheckout)
            {
                waitSteps++;
                yield return new WaitForFixedUpdate();
            }

            if (!session.TryGetPrototypeCustomerVisit(out CustomerVisitRecord awaitingVisit) ||
                awaitingVisit.State != CustomerVisitState.AwaitingCheckout ||
                awaitingVisit.TotalRouteFailureCount != 0)
            {
                LogCustomerFlowSmokeFailure("smoke.checkout-route-mismatch");
                yield break;
            }

            long shelfCheckoutRevision = session.RetailCheckouts.Revision;
            long shelfEconomyRevision = session.CheckoutSettlements.Revision;
            MovePlayerToPhysicalItem(liveBinding.Projection, -Vector3.right, 1.25f);
            playerCarry.ProcessInputFrame();
            bool shelfBypassBlocked = playerCarry.FocusedItem == liveBinding.Projection &&
                                      liveBinding.RequiresCheckoutStart &&
                                      playerCarry.PromptText.Contains("KASA İSTASYONUNA GİT") &&
                                      session.RetailCheckouts.Revision == shelfCheckoutRevision &&
                                      session.CheckoutSettlements.Revision == shelfEconomyRevision;
            if (!shelfBypassBlocked)
            {
                LogCustomerFlowSmokeFailure("smoke.shelf-checkout-bypass");
                yield break;
            }

            MovePlayerToCheckoutStation(1.45f);
            checkoutStation.RefreshPresentation();
            if (!checkoutStation.IsFocused ||
                !checkoutStation.PromptText.Contains("KASAYI BAŞLAT"))
            {
                LogCustomerFlowSmokeFailure(
                    string.IsNullOrEmpty(checkoutStation.LastFailureCode)
                        ? "smoke.checkout-station-focus-missing"
                        : checkoutStation.LastFailureCode);
                yield break;
            }

            OperationResult beginCheckout = checkoutStation.TryOperate();
            bool checkoutStartedAtStation = beginCheckout.IsSuccess &&
                                            liveBinding.RequiresCheckoutCompletion &&
                                            session.RetailCheckouts.Revision ==
                                                shelfCheckoutRevision + 1 &&
                                            session.CheckoutSettlements.Revision ==
                                                shelfEconomyRevision &&
                                            checkoutStation.PromptText.Contains(
                                                "NAKİT ÖDEMEYİ AL");
            if (checkoutStartedAtStation)
            {
                yield return null;
                checkoutStation.RefreshPresentation();
            }

            OperationResult settleCash = checkoutStartedAtStation
                ? checkoutStation.TryOperate()
                : OperationResult.Fail(
                    Failure.FromCode("smoke.checkout-station-start-mismatch"));
            if (beginCheckout.IsFailure || settleCash.IsFailure)
            {
                LogCustomerFlowSmokeFailure(
                    beginCheckout.IsFailure ? beginCheckout.Error.Code : settleCash.Error.Code);
                yield break;
            }

            waitSteps = 0;
            while (waitSteps < routeStepLimit &&
                   session.TryGetPrototypeCustomerVisit(out CustomerVisitRecord exitCandidate) &&
                   exitCandidate.State != CustomerVisitState.Exited)
            {
                waitSteps++;
                yield return new WaitForFixedUpdate();
            }

            stockFlow.RefreshPresentation();
            bool hasExitedVisit = session.TryGetPrototypeCustomerVisit(
                out CustomerVisitRecord exitedVisit);
            bool hasFulfilledReceipt = session.TryGetPrototypeCheckoutSettlement(
                out CheckoutSettlementReceipt fulfilledReceipt);
            bool hasFulfilledTransaction = session.TryGetPrototypeLedgerTransaction(
                out EconomyLedgerTransactionRecord fulfilledTransaction);
            bool invariantsValid = session.ValidateInvariants().IsSuccess;
            bool hasRemainingMotherboard = session.TryGetMotherboardItem(
                out InventoryItemRecord remainingMotherboard);
            bool hasRemainingProcessor = session.TryGetProcessorItem(
                out InventoryItemRecord remainingProcessor);
            bool hasRemainingGraphicsCard =
                session.TryGetGraphicsCardAssemblyItem(
                    out InventoryItemRecord remainingGraphicsCard);
            bool motherboardProjectionValid = motherboardBinding != null &&
                                                motherboardBinding.ValidateProjectionInvariant().IsSuccess;
            bool processorProjectionValid = processorBinding != null &&
                                              processorBinding.ValidateProjectionInvariant().IsSuccess;
            bool motherboardIsolated = session.Inventory.SerializedItemCount == 7 &&
                                       hasRemainingMotherboard &&
                                       remainingMotherboard.Id == session.MotherboardItemId &&
                                       remainingMotherboard.ProductId == session.MotherboardProductId &&
                                       remainingMotherboard.ContainerId == session.WorldFloorContainerId &&
                                       hasRemainingProcessor &&
                                       remainingProcessor.Id == session.ProcessorItemId &&
                                       remainingProcessor.ProductId == session.ProcessorProductId &&
                                       remainingProcessor.ContainerId == session.WorldFloorContainerId &&
                                       hasRemainingGraphicsCard &&
                                       remainingGraphicsCard.Id ==
                                           session.GraphicsCardAssemblyItemId &&
                                       remainingGraphicsCard.ProductId == session.ProductId &&
                                       remainingGraphicsCard.ContainerId ==
                                           session.WorldFloorContainerId &&
                                       session.AssemblyBuild.Revision == 0 &&
                                       session.AssemblyBuild.ProcessorSocketState ==
                                           ProcessorSocketState.EmptyOpen &&
                                       motherboardProjectionValid &&
                                       processorProjectionValid;
            bool fulfilled = hasExitedVisit &&
                             exitedVisit.State == CustomerVisitState.Exited &&
                             exitedVisit.ExitReason == CustomerVisitExitReason.Fulfilled &&
                             !exitedVisit.RouteFallbackUsed &&
                             exitedVisit.TotalRouteFailureCount == 0 &&
                             !customerFlow.CustomerVisible &&
                             !session.TryGetItem(out _) &&
                             session.Inventory.GetTotalQuantity(session.ProductId).Value == 1 &&
                             session.Inventory.GetAvailableQuantity(session.ProductId).Value == 1 &&
                             session.RetailBaskets.Count == 0 &&
                             session.RetailCheckouts.CompletionCount == 1 &&
                             session.CheckoutSettlements.SettlementCount == 1 &&
                             session.CheckoutSettlements.TransactionCount == 1 &&
                             hasFulfilledReceipt &&
                             fulfilledReceipt.PaymentMethod == CheckoutPaymentMethod.Cash &&
                             fulfilledReceipt.GrossMinorUnits ==
                                 GarageStockFlowSession.PrototypePriceMinorUnits &&
                             fulfilledReceipt.CostOfGoodsSoldMinorUnits ==
                                 GarageStockFlowSession.PrototypeUnitCostMinorUnits &&
                             hasFulfilledTransaction &&
                             fulfilledTransaction.Entries.Count == 4 &&
                             fulfilledTransaction.Entries[0].MinorUnits +
                                 fulfilledTransaction.Entries[2].MinorUnits ==
                                 fulfilledTransaction.Entries[1].MinorUnits +
                             fulfilledTransaction.Entries[3].MinorUnits &&
                             !liveBinding.Projection.gameObject.activeSelf &&
                             motherboardIsolated &&
                             invariantsValid;
            if (!fulfilled)
            {
                LogCustomerFlowSmokeFailure(
                    "smoke.fulfilled-exit-mismatch " +
                    $"visit={(hasExitedVisit ? exitedVisit.State.ToString() : "missing")} " +
                    $"reason={(hasExitedVisit ? exitedVisit.ExitReason.ToString() : "missing")} " +
                    $"fallback={(hasExitedVisit && exitedVisit.RouteFallbackUsed ? "yes" : "no")} " +
                    $"route-failures={(hasExitedVisit ? exitedVisit.TotalRouteFailureCount.ToString() : "missing")} " +
                    $"visible={(customerFlow.CustomerVisible ? "yes" : "no")} " +
                    $"stock={session.Inventory.GetTotalQuantity(session.ProductId).Value} " +
                    $"basket={session.RetailBaskets.Count} " +
                    $"completion={session.RetailCheckouts.CompletionCount} " +
                    $"settlement={session.CheckoutSettlements.SettlementCount} " +
                    $"transaction={session.CheckoutSettlements.TransactionCount} " +
                    $"receipt={(hasFulfilledReceipt ? "ok" : "missing")} " +
                    $"ledger={(hasFulfilledTransaction ? "ok" : "missing")} " +
                    $"projection={(liveBinding.Projection.gameObject.activeSelf ? "visible" : "hidden")} " +
                    $"global-items={session.Inventory.SerializedItemCount} " +
                    $"motherboard-id={(hasRemainingMotherboard && remainingMotherboard.Id == session.MotherboardItemId ? "ok" : "mismatch")} " +
                    $"motherboard-product={(hasRemainingMotherboard && remainingMotherboard.ProductId == session.MotherboardProductId ? "ok" : "mismatch")} " +
                    $"motherboard-container={(hasRemainingMotherboard ? remainingMotherboard.ContainerId.Value : "missing")} " +
                    $"processor-id={(hasRemainingProcessor && remainingProcessor.Id == session.ProcessorItemId ? "ok" : "mismatch")} " +
                    $"processor-container={(hasRemainingProcessor ? remainingProcessor.ContainerId.Value : "missing")} " +
                    $"assembly-revision={session.AssemblyBuild.Revision} " +
                    $"motherboard-projection={(motherboardProjectionValid ? "ok" : "failed")} " +
                    $"processor-projection={(processorProjectionValid ? "ok" : "failed")} " +
                    $"invariants={(invariantsValid ? "ok" : "failed")}");
                yield break;
            }

            GarageStockFlowSession staleSession = GarageStockFlowSession.CreateArrived();
            OperationResult staleAccept = staleSession.AcceptArrivedDelivery();
            OperationResult staleShelf = staleSession.TransferItem(staleSession.ShelfContainerId);
            OperationResult stalePublish = staleSession.PublishShelfOffer();
            OperationResult staleStart = staleSession.StartPrototypeCustomerVisit(
                SimulationTimestamp.Create(1, 20));
            OperationResult staleBrowse = staleSession.MarkPrototypeCustomerBrowseArrival(
                SimulationTimestamp.Create(2, 40));
            OperationResult staleConsultation = staleSession.ConsultPrototypeCustomer(
                SimulationTimestamp.Create(3, 60));
            OperationResult<CustomerOfferDecision> staleDecisionResult =
                staleSession.EvaluatePrototypeCustomerOffer();
            OperationResult staleOfferDrift = staleSession.RetailOffers.SetOffer(
                staleSession.ShelfOfferId,
                staleSession.ProductId,
                staleSession.ShelfContainerId,
                GarageStockFlowSession.PrototypeCurrencyCode,
                GarageStockFlowSession.PrototypePriceMinorUnits + 1);
            long staleActionRevision = staleSession.CustomerOfferActions.Revision;
            long staleActorRevision = staleSession.CustomerVisits.Revision;
            long staleInventoryRevision = staleSession.Inventory.Revision;
            long staleBasketRevision = staleSession.RetailBaskets.Revision;
            long staleOfferRevision = staleSession.RetailOffers.Revision;
            long staleCheckoutRevision = staleSession.RetailCheckouts.Revision;
            long staleOrderRevision = staleSession.Orders.Revision;
            long staleConsultationRevision = staleSession.CustomerConsultations.Revision;
            OperationResult staleApply = staleDecisionResult.IsSuccess
                ? staleSession.ApplyPrototypeCustomerBuy(
                    staleDecisionResult.Value,
                    SimulationTimestamp.Create(4, 80))
                : OperationResult.Fail(staleDecisionResult.Error);
            bool staleBlocked = staleAccept.IsSuccess &&
                                staleShelf.IsSuccess &&
                                stalePublish.IsSuccess &&
                                staleStart.IsSuccess &&
                                staleBrowse.IsSuccess &&
                                staleConsultation.IsSuccess &&
                                staleDecisionResult.IsSuccess &&
                                staleOfferDrift.IsSuccess &&
                                staleApply.Error ==
                                    CustomerOfferDecisionActionFailures.DecisionStale &&
                                staleSession.CustomerOfferActions.Revision == staleActionRevision &&
                                staleSession.CustomerVisits.Revision == staleActorRevision &&
                                staleSession.Inventory.Revision == staleInventoryRevision &&
                                staleSession.RetailBaskets.Revision == staleBasketRevision &&
                                staleSession.RetailOffers.Revision == staleOfferRevision &&
                                staleSession.RetailCheckouts.Revision == staleCheckoutRevision &&
                                staleSession.Orders.Revision == staleOrderRevision &&
                                staleSession.CustomerConsultations.Revision ==
                                    staleConsultationRevision &&
                                staleSession.CustomerOfferActions.Count == 0 &&
                                staleSession.RetailBaskets.Count == 0 &&
                                staleSession.Inventory.ReservationCount == 0 &&
                                staleSession.ValidateInvariants().IsSuccess;
            if (!staleBlocked)
            {
                LogCustomerFlowSmokeFailure(
                    staleApply.IsFailure
                        ? staleApply.Error.Code
                        : "smoke.stale-decision-mutated-authority");
                yield break;
            }

            GarageStockFlowSession foreignReceiptSession =
                GarageStockFlowSession.CreateArrived();
            OperationResult foreignReceiptAccept =
                foreignReceiptSession.AcceptArrivedDelivery();
            OperationResult foreignReceiptShelf = foreignReceiptSession.TransferItem(
                foreignReceiptSession.ShelfContainerId);
            OperationResult foreignReceiptPublish =
                foreignReceiptSession.PublishShelfOffer();
            OperationResult foreignReceiptStart =
                foreignReceiptSession.StartPrototypeCustomerVisit(
                    SimulationTimestamp.Create(1, 20));
            OperationResult foreignReceiptBrowse =
                foreignReceiptSession.MarkPrototypeCustomerBrowseArrival(
                    SimulationTimestamp.Create(2, 40));
            OperationResult foreignReceiptConsult =
                foreignReceiptSession.ConsultPrototypeCustomer(
                    SimulationTimestamp.Create(3, 60));
            bool hasForeignReceipt =
                foreignReceiptSession.TryGetPrototypeCustomerConsultation(
                    out CustomerConsultationRecord foreignReceipt);

            GarageStockFlowSession receiptOwnerSession =
                GarageStockFlowSession.CreateArrived();
            OperationResult receiptOwnerAccept = receiptOwnerSession.AcceptArrivedDelivery();
            OperationResult receiptOwnerShelf = receiptOwnerSession.TransferItem(
                receiptOwnerSession.ShelfContainerId);
            OperationResult receiptOwnerPublish = receiptOwnerSession.PublishShelfOffer();
            OperationResult receiptOwnerStart = receiptOwnerSession.StartPrototypeCustomerVisit(
                SimulationTimestamp.Create(1, 20));
            OperationResult receiptOwnerBrowse =
                receiptOwnerSession.MarkPrototypeCustomerBrowseArrival(
                    SimulationTimestamp.Create(2, 40));
            bool hasReceiptOwnerVisit = receiptOwnerSession.TryGetPrototypeCustomerVisit(
                out CustomerVisitRecord receiptOwnerVisit);
            bool hasReceiptOwnerOffer = receiptOwnerSession.TryGetShelfOffer(
                out ShelfOfferRecord receiptOwnerOffer);
            OperationResult<CustomerOfferDecision> foreignReceiptDecision =
                hasForeignReceipt && hasReceiptOwnerVisit && hasReceiptOwnerOffer
                    ? CustomerOfferDecisionEvaluator.Evaluate(
                        receiptOwnerVisit,
                        foreignReceipt,
                        receiptOwnerOffer,
                        ShelfPrice.Create(
                            GarageStockFlowSession.PrototypeCurrencyCode,
                            GarageStockFlowSession.PrototypeMaximumAcceptedPriceMinorUnits).Value)
                    : OperationResult<CustomerOfferDecision>.Fail(
                        CustomerOfferDecisionFailures.InputInvalid);
            long receiptOwnerActionRevision = receiptOwnerSession.CustomerOfferActions.Revision;
            long receiptOwnerVisitRevision = receiptOwnerSession.CustomerVisits.Revision;
            long receiptOwnerInventoryRevision = receiptOwnerSession.Inventory.Revision;
            long receiptOwnerBasketRevision = receiptOwnerSession.RetailBaskets.Revision;
            long receiptOwnerOfferRevision = receiptOwnerSession.RetailOffers.Revision;
            long receiptOwnerCheckoutRevision = receiptOwnerSession.RetailCheckouts.Revision;
            long receiptOwnerOrderRevision = receiptOwnerSession.Orders.Revision;
            long receiptOwnerConsultationRevision =
                receiptOwnerSession.CustomerConsultations.Revision;
            bool staleConsultationBlocked = foreignReceiptAccept.IsSuccess &&
                                            foreignReceiptShelf.IsSuccess &&
                                            foreignReceiptPublish.IsSuccess &&
                                            foreignReceiptStart.IsSuccess &&
                                            foreignReceiptBrowse.IsSuccess &&
                                            foreignReceiptConsult.IsSuccess &&
                                            hasForeignReceipt &&
                                            receiptOwnerAccept.IsSuccess &&
                                            receiptOwnerShelf.IsSuccess &&
                                            receiptOwnerPublish.IsSuccess &&
                                            receiptOwnerStart.IsSuccess &&
                                            receiptOwnerBrowse.IsSuccess &&
                                            foreignReceiptDecision.Error ==
                                                CustomerOfferDecisionFailures.ConsultationMismatch &&
                                            !receiptOwnerSession.CustomerConsultations.Owns(
                                                foreignReceipt) &&
                                            receiptOwnerSession.CustomerOfferActions.Revision ==
                                                receiptOwnerActionRevision &&
                                            receiptOwnerSession.CustomerVisits.Revision ==
                                                receiptOwnerVisitRevision &&
                                            receiptOwnerSession.Inventory.Revision ==
                                                receiptOwnerInventoryRevision &&
                                            receiptOwnerSession.RetailBaskets.Revision ==
                                                receiptOwnerBasketRevision &&
                                            receiptOwnerSession.RetailOffers.Revision ==
                                                receiptOwnerOfferRevision &&
                                            receiptOwnerSession.RetailCheckouts.Revision ==
                                                receiptOwnerCheckoutRevision &&
                                            receiptOwnerSession.Orders.Revision ==
                                                receiptOwnerOrderRevision &&
                                            receiptOwnerSession.CustomerConsultations.Revision ==
                                                receiptOwnerConsultationRevision &&
                                            receiptOwnerSession.CustomerOfferActions.Count == 0 &&
                                            receiptOwnerSession.RetailBaskets.Count == 0 &&
                                            receiptOwnerSession.Inventory.ReservationCount == 0 &&
                                            receiptOwnerSession.ValidateInvariants().IsSuccess &&
                                            foreignReceiptSession.ValidateInvariants().IsSuccess;
            if (!staleConsultationBlocked)
            {
                LogCustomerFlowSmokeFailure(
                    foreignReceiptDecision.IsFailure
                        ? foreignReceiptDecision.Error.Code
                        : "smoke.foreign-consultation-not-blocked");
                yield break;
            }

            GarageStockFlowSession leaveSession = GarageStockFlowSession.CreateArrived();
            OperationResult leaveAccept = leaveSession.AcceptArrivedDelivery();
            OperationResult leaveShelf = leaveSession.TransferItem(
                leaveSession.ShelfContainerId);
            OperationResult leavePublish = leaveSession.PublishShelfOffer();
            OperationResult leavePrice = leaveSession.RetailOffers.SetOffer(
                leaveSession.ShelfOfferId,
                leaveSession.ProductId,
                leaveSession.ShelfContainerId,
                GarageStockFlowSession.PrototypeCurrencyCode,
                GarageStockFlowSession.PrototypeMaximumAcceptedPriceMinorUnits + 1);
            OperationResult leaveStart = leaveSession.StartPrototypeCustomerVisit(
                SimulationTimestamp.Create(1, 20));
            OperationResult leaveBrowse = leaveSession.MarkPrototypeCustomerBrowseArrival(
                SimulationTimestamp.Create(2, 40));
            OperationResult leaveConsultation = leaveSession.ConsultPrototypeCustomer(
                SimulationTimestamp.Create(3, 60));
            OperationResult<CustomerOfferDecision> leaveDecision =
                leaveSession.EvaluatePrototypeCustomerOffer();
            long leaveActionRevision = leaveSession.CustomerOfferActions.Revision;
            long leaveActorRevision = leaveSession.CustomerVisits.Revision;
            long leaveInventoryRevision = leaveSession.Inventory.Revision;
            long leaveBasketRevision = leaveSession.RetailBaskets.Revision;
            long leaveOfferRevision = leaveSession.RetailOffers.Revision;
            long leaveCheckoutRevision = leaveSession.RetailCheckouts.Revision;
            long leaveOrderRevision = leaveSession.Orders.Revision;
            long leaveConsultationRevision = leaveSession.CustomerConsultations.Revision;
            OperationResult leaveApply = leaveDecision.IsSuccess
                ? leaveSession.ApplyPrototypeCustomerLeave(
                    leaveDecision.Value,
                    SimulationTimestamp.Create(4, 80))
                : OperationResult.Fail(leaveDecision.Error);
            OperationResult leaveExit = leaveApply.IsSuccess
                ? leaveSession.MarkPrototypeCustomerExitArrival(
                    SimulationTimestamp.Create(5, 100))
                : OperationResult.Fail(leaveApply.Error);
            bool leaveAction = leaveAccept.IsSuccess &&
                               leaveShelf.IsSuccess &&
                               leavePublish.IsSuccess &&
                               leavePrice.IsSuccess &&
                               leaveStart.IsSuccess &&
                               leaveBrowse.IsSuccess &&
                               leaveConsultation.IsSuccess &&
                               leaveDecision.IsSuccess &&
                               leaveDecision.Value.DecisionKind ==
                                   CustomerOfferDecisionKind.Leave &&
                               leaveApply.IsSuccess &&
                               leaveExit.IsSuccess &&
                               leaveSession.CustomerOfferActions.Revision ==
                                   leaveActionRevision + 1 &&
                               leaveSession.CustomerVisits.Revision ==
                                   leaveActorRevision + 2 &&
                               leaveSession.Inventory.Revision == leaveInventoryRevision &&
                               leaveSession.RetailBaskets.Revision == leaveBasketRevision &&
                               leaveSession.RetailOffers.Revision == leaveOfferRevision &&
                               leaveSession.RetailCheckouts.Revision == leaveCheckoutRevision &&
                               leaveSession.Orders.Revision == leaveOrderRevision &&
                               leaveSession.CustomerConsultations.Revision ==
                                   leaveConsultationRevision &&
                               leaveSession.TryGetPrototypeCustomerLeaveAction(
                                   out CustomerOfferDecisionActionRecord leaveRecord) &&
                               leaveRecord.IsLeave &&
                               !leaveRecord.HasReservation &&
                               leaveSession.TryGetPrototypeCustomerVisit(
                                   out CustomerVisitRecord declinedVisit) &&
                               declinedVisit.State == CustomerVisitState.Exited &&
                               declinedVisit.ExitReason ==
                                   CustomerVisitExitReason.OfferDeclined &&
                               leaveSession.RetailBaskets.Count == 0 &&
                               leaveSession.Inventory.ReservationCount == 0 &&
                               leaveSession.ValidateInvariants().IsSuccess;
            if (!leaveAction)
            {
                LogCustomerFlowSmokeFailure(
                    leaveApply.IsFailure
                        ? leaveApply.Error.Code
                        : leaveExit.IsFailure
                            ? leaveExit.Error.Code
                            : "smoke.leave-action-mismatch");
                yield break;
            }

            GarageStockFlowSession staleLeaveSession = GarageStockFlowSession.CreateArrived();
            OperationResult staleLeaveAccept = staleLeaveSession.AcceptArrivedDelivery();
            OperationResult staleLeaveShelf = staleLeaveSession.TransferItem(
                staleLeaveSession.ShelfContainerId);
            OperationResult staleLeavePublish = staleLeaveSession.PublishShelfOffer();
            OperationResult staleLeavePrice = staleLeaveSession.RetailOffers.SetOffer(
                staleLeaveSession.ShelfOfferId,
                staleLeaveSession.ProductId,
                staleLeaveSession.ShelfContainerId,
                GarageStockFlowSession.PrototypeCurrencyCode,
                GarageStockFlowSession.PrototypeMaximumAcceptedPriceMinorUnits + 1);
            OperationResult staleLeaveStart = staleLeaveSession.StartPrototypeCustomerVisit(
                SimulationTimestamp.Create(1, 20));
            OperationResult staleLeaveBrowse =
                staleLeaveSession.MarkPrototypeCustomerBrowseArrival(
                    SimulationTimestamp.Create(2, 40));
            OperationResult staleLeaveConsultation =
                staleLeaveSession.ConsultPrototypeCustomer(
                    SimulationTimestamp.Create(3, 60));
            OperationResult<CustomerOfferDecision> staleLeaveDecision =
                staleLeaveSession.EvaluatePrototypeCustomerOffer();
            OperationResult staleLeaveDrift = staleLeaveSession.RetailOffers.SetOffer(
                staleLeaveSession.ShelfOfferId,
                staleLeaveSession.ProductId,
                staleLeaveSession.ShelfContainerId,
                GarageStockFlowSession.PrototypeCurrencyCode,
                GarageStockFlowSession.PrototypePriceMinorUnits);
            long staleLeaveActionRevision = staleLeaveSession.CustomerOfferActions.Revision;
            long staleLeaveActorRevision = staleLeaveSession.CustomerVisits.Revision;
            long staleLeaveInventoryRevision = staleLeaveSession.Inventory.Revision;
            long staleLeaveBasketRevision = staleLeaveSession.RetailBaskets.Revision;
            long staleLeaveOfferRevision = staleLeaveSession.RetailOffers.Revision;
            long staleLeaveCheckoutRevision = staleLeaveSession.RetailCheckouts.Revision;
            long staleLeaveOrderRevision = staleLeaveSession.Orders.Revision;
            long staleLeaveConsultationRevision =
                staleLeaveSession.CustomerConsultations.Revision;
            OperationResult staleLeaveApply = staleLeaveDecision.IsSuccess
                ? staleLeaveSession.ApplyPrototypeCustomerLeave(
                    staleLeaveDecision.Value,
                    SimulationTimestamp.Create(4, 80))
                : OperationResult.Fail(staleLeaveDecision.Error);
            bool staleLeaveBlocked = staleLeaveAccept.IsSuccess &&
                                     staleLeaveShelf.IsSuccess &&
                                     staleLeavePublish.IsSuccess &&
                                     staleLeavePrice.IsSuccess &&
                                     staleLeaveStart.IsSuccess &&
                                     staleLeaveBrowse.IsSuccess &&
                                     staleLeaveConsultation.IsSuccess &&
                                     staleLeaveDecision.IsSuccess &&
                                     staleLeaveDecision.Value.DecisionKind ==
                                         CustomerOfferDecisionKind.Leave &&
                                     staleLeaveDrift.IsSuccess &&
                                     staleLeaveApply.Error ==
                                         CustomerOfferDecisionActionFailures.DecisionStale &&
                                     staleLeaveSession.CustomerOfferActions.Revision ==
                                         staleLeaveActionRevision &&
                                     staleLeaveSession.CustomerVisits.Revision ==
                                         staleLeaveActorRevision &&
                                     staleLeaveSession.Inventory.Revision ==
                                         staleLeaveInventoryRevision &&
                                     staleLeaveSession.RetailBaskets.Revision ==
                                         staleLeaveBasketRevision &&
                                     staleLeaveSession.RetailOffers.Revision ==
                                         staleLeaveOfferRevision &&
                                     staleLeaveSession.RetailCheckouts.Revision ==
                                         staleLeaveCheckoutRevision &&
                                     staleLeaveSession.Orders.Revision == staleLeaveOrderRevision &&
                                     staleLeaveSession.CustomerConsultations.Revision ==
                                         staleLeaveConsultationRevision &&
                                     staleLeaveSession.CustomerOfferActions.Count == 0 &&
                                     staleLeaveSession.RetailBaskets.Count == 0 &&
                                     staleLeaveSession.Inventory.ReservationCount == 0 &&
                                     staleLeaveSession.TryGetPrototypeCustomerVisit(
                                         out CustomerVisitRecord staleLeaveVisit) &&
                                     staleLeaveVisit.State == CustomerVisitState.Browsing &&
                                     staleLeaveSession.ValidateInvariants().IsSuccess;
            if (!staleLeaveBlocked)
            {
                LogCustomerFlowSmokeFailure(
                    staleLeaveApply.IsFailure
                        ? staleLeaveApply.Error.Code
                        : "smoke.stale-leave-mutated-authority");
                yield break;
            }

            GarageStockFlowSession routeFallbackSession = GarageStockFlowSession.CreateArrived();
            long fallbackInventoryRevision = routeFallbackSession.Inventory.Revision;
            long fallbackOrderRevision = routeFallbackSession.Orders.Revision;
            long fallbackOfferRevision = routeFallbackSession.RetailOffers.Revision;
            long fallbackBasketRevision = routeFallbackSession.RetailBaskets.Revision;
            long fallbackCheckoutRevision = routeFallbackSession.RetailCheckouts.Revision;
            long fallbackConsultationRevision =
                routeFallbackSession.CustomerConsultations.Revision;
            OperationResult fallbackStart = routeFallbackSession.StartPrototypeCustomerVisit(
                SimulationTimestamp.Create(1, 20));
            OperationResult routeFailureOne = routeFallbackSession.ReportPrototypeCustomerRouteFailure(
                SimulationTimestamp.Create(2, 40));
            OperationResult routeFailureTwo = routeFallbackSession.ReportPrototypeCustomerRouteFailure(
                SimulationTimestamp.Create(3, 60));
            OperationResult exitFailureOne = routeFallbackSession.ReportPrototypeCustomerRouteFailure(
                SimulationTimestamp.Create(4, 80));
            OperationResult exitFailureTwo = routeFallbackSession.ReportPrototypeCustomerRouteFailure(
                SimulationTimestamp.Create(5, 100));
            bool routeFallback = fallbackStart.IsSuccess &&
                                 routeFailureOne.IsSuccess &&
                                 routeFailureTwo.IsSuccess &&
                                 exitFailureOne.IsSuccess &&
                                 exitFailureTwo.IsSuccess &&
                                 routeFallbackSession.TryGetPrototypeCustomerVisit(
                                     out CustomerVisitRecord fallbackVisit) &&
                                 fallbackVisit.State == CustomerVisitState.Exited &&
                                 fallbackVisit.ExitReason == CustomerVisitExitReason.RouteUnavailable &&
                                 fallbackVisit.RouteFallbackUsed &&
                                 fallbackVisit.TotalRouteFailureCount == 4 &&
                                 routeFallbackSession.Inventory.Revision == fallbackInventoryRevision &&
                                 routeFallbackSession.Orders.Revision == fallbackOrderRevision &&
                                 routeFallbackSession.RetailOffers.Revision == fallbackOfferRevision &&
                                 routeFallbackSession.RetailBaskets.Revision == fallbackBasketRevision &&
                                 routeFallbackSession.RetailCheckouts.Revision == fallbackCheckoutRevision &&
                                 routeFallbackSession.CustomerConsultations.Revision ==
                                     fallbackConsultationRevision &&
                                 routeFallbackSession.ValidateInvariants().IsSuccess;
            if (!routeFallback)
            {
                LogCustomerFlowSmokeFailure("smoke.route-fallback-mismatch");
                yield break;
            }

            GarageStockFlowSession timeoutSession = GarageStockFlowSession.CreateArrived();
            long timeoutInventoryRevision = timeoutSession.Inventory.Revision;
            long timeoutOrderRevision = timeoutSession.Orders.Revision;
            long timeoutOfferRevision = timeoutSession.RetailOffers.Revision;
            long timeoutBasketRevision = timeoutSession.RetailBaskets.Revision;
            long timeoutCheckoutRevision = timeoutSession.RetailCheckouts.Revision;
            long timeoutConsultationRevision =
                timeoutSession.CustomerConsultations.Revision;
            OperationResult timeoutStart = timeoutSession.StartPrototypeCustomerVisit(
                SimulationTimestamp.Create(1, 20));
            OperationResult patienceTimeout = timeoutSession.AdvanceCustomerTime(
                SimulationTimestamp.Create(3001, 60_020));
            OperationResult exitTimeout = timeoutSession.AdvanceCustomerTime(
                SimulationTimestamp.Create(6001, 120_020));
            bool timeoutFallback = timeoutStart.IsSuccess &&
                                   patienceTimeout.IsSuccess &&
                                   exitTimeout.IsSuccess &&
                                   timeoutSession.TryGetPrototypeCustomerVisit(
                                       out CustomerVisitRecord timeoutVisit) &&
                                   timeoutVisit.State == CustomerVisitState.Exited &&
                                   timeoutVisit.ExitReason == CustomerVisitExitReason.PatienceExpired &&
                                   timeoutVisit.RouteFallbackUsed &&
                                   timeoutSession.Inventory.Revision == timeoutInventoryRevision &&
                                   timeoutSession.Orders.Revision == timeoutOrderRevision &&
                                   timeoutSession.RetailOffers.Revision == timeoutOfferRevision &&
                                   timeoutSession.RetailBaskets.Revision == timeoutBasketRevision &&
                                   timeoutSession.RetailCheckouts.Revision == timeoutCheckoutRevision &&
                                   timeoutSession.CustomerConsultations.Revision ==
                                       timeoutConsultationRevision &&
                                   timeoutSession.ValidateInvariants().IsSuccess;
            if (!timeoutFallback)
            {
                LogCustomerFlowSmokeFailure("smoke.timeout-fallback-mismatch");
                yield break;
            }

            Debug.Log(
                "GARAGE_CUSTOMER_VISIT_RUNTIME_SMOKE customer-visit=ok runtime-route=ok " +
                "pause=ok consultation=ok consultation-replay=ok decision-gated=ok " +
                "stale-consultation-blocked=ok offer-decision=ok buy-action=ok " +
                "stale-blocked=ok awaiting-checkout-gate=ok fulfilled=ok " +
                "checkout-station=ok station-focus=ok station-los=ok " +
                "shelf-bypass-blocked=ok checkout-start=ok " +
                "cash-payment=ok payment-receipt=ok economy-settlement=ok cash-ledger=ok " +
                "leave-action=ok stale-leave-blocked=ok " +
                "domain-route-fallback=ok domain-timeout-fallback=ok " +
                "authority-isolated=ok stock-consumed=ok stock-projection-hidden=ok " +
                "customer-hidden=ok");
            yield return new WaitForEndOfFrame();
        }

        private static void LogCustomerFlowSmokeFailure(string code)
        {
            Debug.LogError(
                $"GARAGE_CUSTOMER_VISIT_RUNTIME_SMOKE customer-visit=failed code={code}");
        }

        private IEnumerator RunMotherboardAssemblySmoke()
        {
            yield return null;
            yield return new WaitForFixedUpdate();

            if (playerMotor == null ||
                playerCarry == null ||
                stockFlow == null ||
                motherboardSeat == null ||
                motherboardFastener == null ||
                motherboardBinding == null ||
                motherboardBinding.PhysicalItem == null)
            {
                LogMotherboardAssemblySmokeFailure("smoke.context-missing");
                yield break;
            }

            playerMotor.SetPaused(false);
            GarageStockFlowSession session = stockFlow.EnsureInitialized();
            PhysicalItemProjection motherboard = motherboardBinding.PhysicalItem;
            int physicalInstanceId = motherboard.GetInstanceID();
            int physicalMotherboardCount = CountCanonicalMotherboardProjections(
                session.MotherboardItemId.Value);

            if (motherboardBinding.Runtime != stockFlow ||
                motherboardBinding.Seat != motherboardSeat ||
                motherboardBinding.Fastener != motherboardFastener ||
                motherboardBinding.InventoryItemIdValue !=
                    session.MotherboardItemId.Value ||
                motherboard.ItemIdValue != session.MotherboardItemId.Value ||
                physicalMotherboardCount != 1 ||
                session.Inventory.SerializedItemCount != 3 ||
                !session.TryGetMotherboardItem(out InventoryItemRecord looseItem) ||
                looseItem.Id != session.MotherboardItemId ||
                looseItem.ProductId != session.MotherboardProductId ||
                looseItem.ContainerId != session.WorldFloorContainerId ||
                !session.TryGetProcessorItem(out InventoryItemRecord looseProcessor) ||
                looseProcessor.Id != session.ProcessorItemId ||
                looseProcessor.ProductId != session.ProcessorProductId ||
                looseProcessor.ContainerId != session.WorldFloorContainerId ||
                session.AssemblyBuild.ProcessorSocketState !=
                    ProcessorSocketState.EmptyOpen ||
                motherboardFastener.FastenerIdValue !=
                    session.MotherboardFastenerId.Value ||
                motherboardFastener.FocusCollider == null ||
                motherboardFastener.FocusCollider.enabled ||
                !motherboardFastener.MatchesAuthorityState(AssemblySeatState.Empty) ||
                session.AssemblyBuild.MotherboardSeatState != AssemblySeatState.Empty ||
                session.AssemblyBuild.Revision != 0 ||
                session.AssemblyBuild.ReceiptCount != 0 ||
                session.AssemblyBuild.ValidateInvariants().IsFailure ||
                motherboardBinding.ValidateProjectionInvariant().IsFailure)
            {
                LogMotherboardAssemblySmokeFailure("smoke.authority-identity-mismatch");
                yield break;
            }

            long initialAssemblyRevision = session.AssemblyBuild.Revision;
            long initialInventoryRevision = session.Inventory.Revision;
            int initialReceiptCount = session.AssemblyBuild.ReceiptCount;

            long orderRevision = session.Orders.Revision;
            long offerRevision = session.RetailOffers.Revision;
            long basketRevision = session.RetailBaskets.Revision;
            long checkoutRevision = session.RetailCheckouts.Revision;
            long settlementRevision = session.CheckoutSettlements.Revision;
            long visitRevision = session.CustomerVisits.Revision;
            long consultationRevision = session.CustomerConsultations.Revision;
            long actionRevision = session.CustomerOfferActions.Revision;

            OperationResult<PcComponentSpecification> specification =
                session.Components.Get(session.MotherboardProductId);
            AssemblyCompatibilityResult compatibility = specification.IsSuccess
                ? AssemblyCompatibilityEvaluator.EvaluateMotherboardSeat(
                    specification.Value,
                    MotherboardFormFactor.MicroAtx)
                : AssemblyCompatibilityResult.Incompatible(specification.Error);
            OperationResult<PcComponentSpecification> mismatchSpecification =
                PcComponentSpecification.Create(
                    session.Catalog,
                    session.MotherboardProductId,
                    PcComponentKind.Motherboard,
                    MotherboardFormFactor.Atx);
            AssemblyCompatibilityResult mismatch = mismatchSpecification.IsSuccess
                ? AssemblyCompatibilityEvaluator.EvaluateMotherboardSeat(
                    mismatchSpecification.Value,
                    MotherboardFormFactor.MicroAtx)
                : AssemblyCompatibilityResult.Incompatible(mismatchSpecification.Error);
            bool compatible = compatibility.IsCompatible &&
                              compatibility.Reason.IsNone;
            bool mismatchBlocked = !mismatch.IsCompatible &&
                                   mismatch.Reason ==
                                       AssemblyFailures.MotherboardFormFactorMismatch &&
                                   session.AssemblyBuild.Revision == 0 &&
                                   session.AssemblyBuild.ReceiptCount == 0;
            if (!compatible || !mismatchBlocked)
            {
                LogMotherboardAssemblySmokeFailure(
                    compatible ? "smoke.mismatch-not-blocked" : "smoke.compatibility-mismatch");
                yield break;
            }

            OperationResult pickup = playerCarry.TryPickup(motherboard);
            if (pickup.IsFailure ||
                playerCarry.HeldItem != motherboard ||
                !motherboardBinding.IsAuthorityInHands ||
                session.AssemblyBuild.Revision != initialAssemblyRevision ||
                session.Inventory.Revision != initialInventoryRevision + 1 ||
                session.AssemblyBuild.ReceiptCount != initialReceiptCount)
            {
                LogMotherboardAssemblySmokeFailure(
                    pickup.IsFailure ? pickup.Error.Code : "smoke.pickup-projection-mismatch");
                yield break;
            }

            MovePlayerToMotherboardSeat();
            OperationResult beginGuidedSeat = playerCarry.TrySetMotherboardSeatMode(true);
            bool previewReady = beginGuidedSeat.IsSuccess &&
                                playerCarry.IsMotherboardSeatMode &&
                                playerCarry.PlacementValid &&
                                playerCarry.CurrentMotherboardSeatStatus ==
                                    MotherboardSeatStatus.Valid &&
                                playerCarry.PlacementPreview != null &&
                                playerCarry.PlacementPreview.IsVisible &&
                                playerCarry.PlacementPreview.IsShowingValidPose &&
                                ApproximatelySamePose(
                                    playerCarry.PlacementPreview.CurrentPose,
                                    motherboardSeat.SnapPose);
            if (!previewReady)
            {
                LogMotherboardAssemblySmokeFailure(
                    beginGuidedSeat.IsFailure
                        ? beginGuidedSeat.Error.Code
                        : string.IsNullOrEmpty(playerCarry.LastFailureCode)
                            ? "smoke.preview-invalid"
                            : playerCarry.LastFailureCode);
                yield break;
            }

            OperationResult attach = playerCarry.TryConfirmMotherboardSeat();
            AssemblyBuildSnapshot attachedSnapshot = session.AssemblyBuild.GetSnapshot();
            bool attached = attach.IsSuccess &&
                            playerCarry.HeldItem == null &&
                            attachedSnapshot.MotherboardSeatState ==
                                AssemblySeatState.SeatedUnsecured &&
                            attachedSnapshot.MotherboardItemId == session.MotherboardItemId &&
                            session.TryGetMotherboardItem(out InventoryItemRecord seatedItem) &&
                            seatedItem.ContainerId == session.WorkbenchContainerId &&
                            session.AssemblyBuild.Revision == initialAssemblyRevision + 1 &&
                            session.Inventory.Revision == initialInventoryRevision + 2 &&
                            session.AssemblyBuild.ReceiptCount == initialReceiptCount + 1 &&
                            motherboardFastener.FocusCollider.enabled &&
                            motherboardFastener.MatchesAuthorityState(
                                AssemblySeatState.SeatedUnsecured) &&
                            motherboardBinding.ValidateProjectionInvariant().IsSuccess &&
                            ApproximatelySamePose(
                                new Pose(
                                    motherboard.transform.position,
                                    motherboard.transform.rotation),
                                motherboardSeat.SnapPose);
            if (!attached)
            {
                LogMotherboardAssemblySmokeFailure(
                    attach.IsFailure
                        ? attach.Error.Code
                        : "smoke.attach-projection-mismatch");
                yield break;
            }

            var attachReceipts = session.AssemblyBuild.GetReceipts();
            if (attachReceipts.Count != 1)
            {
                LogMotherboardAssemblySmokeFailure("smoke.attach-receipt-mismatch");
                yield break;
            }

            AssemblyOperationReceipt attachReceipt = attachReceipts[0];
            long attachedAssemblyRevision = session.AssemblyBuild.Revision;
            long attachedInventoryRevision = session.Inventory.Revision;
            OperationResult<AssemblyOperationReceipt> attachReplay =
                session.AttachMotherboard(attachReceipt.OperationId);
            bool attachReplayed = attachReplay.IsSuccess &&
                                  ReferenceEquals(attachReplay.Value, attachReceipt) &&
                                  session.AssemblyBuild.Revision == attachedAssemblyRevision &&
                                  session.Inventory.Revision == attachedInventoryRevision &&
                                  session.AssemblyBuild.ReceiptCount == 1;
            if (!attachReplayed)
            {
                LogMotherboardAssemblySmokeFailure("smoke.attach-replay-mismatch");
                yield break;
            }

            OperationResult duplicateConfirm = playerCarry.TryConfirmMotherboardSeat();
            bool duplicateSeatConfirmBlocked = duplicateConfirm.IsFailure &&
                                               session.AssemblyBuild.Revision ==
                                                   attachedAssemblyRevision &&
                                               session.Inventory.Revision ==
                                                   attachedInventoryRevision &&
                                               session.AssemblyBuild.ReceiptCount == 1;
            if (!duplicateSeatConfirmBlocked)
            {
                LogMotherboardAssemblySmokeFailure("smoke.input-double-consumed");
                yield break;
            }

            MovePlayerToMotherboardFastener();
            long fastenerInventoryRevision = session.Inventory.Revision;
            OperationResult secure = playerCarry.TryOperateMotherboardFastener();
            AssemblyBuildSnapshot securedSnapshot = session.AssemblyBuild.GetSnapshot();
            var securedReceipts = session.AssemblyBuild.GetReceipts();
            AssemblyOperationReceipt secureReceipt = securedReceipts.Count > 1
                ? securedReceipts[1]
                : null;
            bool secured = secure.IsSuccess &&
                           secureReceipt != null &&
                           secureReceipt.OperationKind ==
                               AssemblyOperationKind.SecureMotherboardFastener &&
                           secureReceipt.SourceAttachOperationId == attachReceipt.OperationId &&
                           secureReceipt.SourceSecureOperationId.IsEmpty &&
                           secureReceipt.FastenerId == session.MotherboardFastenerId &&
                           securedSnapshot.MotherboardSeatState ==
                               AssemblySeatState.SeatedSecured &&
                           securedSnapshot.SecuredByOperationId == secureReceipt.OperationId &&
                           session.AssemblyBuild.Revision == initialAssemblyRevision + 2 &&
                           session.AssemblyBuild.ReceiptCount == initialReceiptCount + 2 &&
                           session.Inventory.Revision == fastenerInventoryRevision &&
                           motherboardFastener.MatchesAuthorityState(
                               AssemblySeatState.SeatedSecured) &&
                           motherboardBinding.ValidateProjectionInvariant().IsSuccess;
            if (!secured)
            {
                LogMotherboardAssemblySmokeFailure(
                    secure.IsFailure ? secure.Error.Code : "smoke.secure-mismatch");
                yield break;
            }

            OperationResult<AssemblyOperationReceipt> secureReplay =
                session.SecureMotherboardFastener(
                    secureReceipt.OperationId,
                    attachReceipt.OperationId,
                    secureReceipt.ExpectedAssemblyRevision);
            bool secureReplayed = secureReplay.IsSuccess &&
                                  ReferenceEquals(secureReplay.Value, secureReceipt) &&
                                  session.AssemblyBuild.Revision ==
                                      initialAssemblyRevision + 2 &&
                                  session.AssemblyBuild.ReceiptCount ==
                                      initialReceiptCount + 2 &&
                                  session.Inventory.Revision == fastenerInventoryRevision;
            if (!secureReplayed)
            {
                LogMotherboardAssemblySmokeFailure("smoke.secure-replay-mismatch");
                yield break;
            }

            Pose securedPose = new Pose(
                motherboard.transform.position,
                motherboard.transform.rotation);
            Transform securedParent = motherboard.transform.parent;
            OperationResult blockedDetach = playerCarry.TryPickup(motherboard);
            bool detachBlocked = blockedDetach.IsFailure &&
                                 blockedDetach.Error == AssemblyFailures.ComponentSecured &&
                                 playerCarry.HeldItem == null &&
                                 motherboard.transform.parent == securedParent &&
                                 ApproximatelySamePose(
                                     new Pose(
                                         motherboard.transform.position,
                                         motherboard.transform.rotation),
                                     securedPose) &&
                                 session.AssemblyBuild.MotherboardSeatState ==
                                     AssemblySeatState.SeatedSecured &&
                                 session.AssemblyBuild.Revision ==
                                     initialAssemblyRevision + 2 &&
                                 session.AssemblyBuild.ReceiptCount ==
                                     initialReceiptCount + 2 &&
                                 session.Inventory.Revision == fastenerInventoryRevision;
            if (!detachBlocked)
            {
                LogMotherboardAssemblySmokeFailure("smoke.secured-detach-not-blocked");
                yield break;
            }

            StableId<AssemblyOperationIdScope> directDetachOperationId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.prototype-001.smoke-secured-detach.r000003");
            OperationResult<AssemblyOperationReceipt> authorityBlockedDetach =
                session.DetachMotherboard(directDetachOperationId);
            AssemblyBuildSnapshot authorityBlockedSnapshot =
                session.AssemblyBuild.GetSnapshot();
            bool authorityDetachBlocked = authorityBlockedDetach.IsFailure &&
                                          authorityBlockedDetach.Error ==
                                              AssemblyFailures.ComponentSecured &&
                                          authorityBlockedSnapshot.MotherboardSeatState ==
                                              securedSnapshot.MotherboardSeatState &&
                                          authorityBlockedSnapshot.MotherboardItemId ==
                                              securedSnapshot.MotherboardItemId &&
                                          authorityBlockedSnapshot.InstalledByOperationId ==
                                              securedSnapshot.InstalledByOperationId &&
                                          authorityBlockedSnapshot.SecuredByOperationId ==
                                              securedSnapshot.SecuredByOperationId &&
                                          session.AssemblyBuild.Revision ==
                                              initialAssemblyRevision + 2 &&
                                          session.AssemblyBuild.ReceiptCount ==
                                              initialReceiptCount + 2 &&
                                          session.Inventory.Revision ==
                                              fastenerInventoryRevision &&
                                          playerCarry.HeldItem == null &&
                                          motherboard.transform.parent == securedParent &&
                                          ApproximatelySamePose(
                                              new Pose(
                                                  motherboard.transform.position,
                                                  motherboard.transform.rotation),
                                              securedPose);
            if (!authorityDetachBlocked)
            {
                LogMotherboardAssemblySmokeFailure(
                    "smoke.secured-detach-authority-not-blocked");
                yield break;
            }

            OperationResult unsecure = playerCarry.TryOperateMotherboardFastener();
            AssemblyBuildSnapshot unsecuredSnapshot = session.AssemblyBuild.GetSnapshot();
            var unsecuredReceipts = session.AssemblyBuild.GetReceipts();
            AssemblyOperationReceipt unsecureReceipt = unsecuredReceipts.Count > 2
                ? unsecuredReceipts[2]
                : null;
            bool unsecured = unsecure.IsSuccess &&
                              unsecureReceipt != null &&
                              unsecureReceipt.OperationKind ==
                                  AssemblyOperationKind.UnsecureMotherboardFastener &&
                              unsecureReceipt.SourceAttachOperationId == attachReceipt.OperationId &&
                              unsecureReceipt.SourceSecureOperationId == secureReceipt.OperationId &&
                              unsecuredSnapshot.MotherboardSeatState ==
                                  AssemblySeatState.SeatedUnsecured &&
                              unsecuredSnapshot.SecuredByOperationId.IsEmpty &&
                              session.AssemblyBuild.Revision == initialAssemblyRevision + 3 &&
                              session.AssemblyBuild.ReceiptCount == initialReceiptCount + 3 &&
                              session.Inventory.Revision == fastenerInventoryRevision &&
                              motherboardFastener.MatchesAuthorityState(
                                  AssemblySeatState.SeatedUnsecured) &&
                              motherboardBinding.ValidateProjectionInvariant().IsSuccess;
            if (!unsecured)
            {
                LogMotherboardAssemblySmokeFailure(
                    unsecure.IsFailure ? unsecure.Error.Code : "smoke.unsecure-mismatch");
                yield break;
            }

            OperationResult<AssemblyOperationReceipt> unsecureReplay =
                session.UnsecureMotherboardFastener(
                    unsecureReceipt.OperationId,
                    attachReceipt.OperationId,
                    secureReceipt.OperationId,
                    unsecureReceipt.ExpectedAssemblyRevision);
            bool unsecureReplayed = unsecureReplay.IsSuccess &&
                                    ReferenceEquals(unsecureReplay.Value, unsecureReceipt) &&
                                    session.AssemblyBuild.Revision ==
                                        initialAssemblyRevision + 3 &&
                                    session.AssemblyBuild.ReceiptCount ==
                                        initialReceiptCount + 3 &&
                                    session.Inventory.Revision == fastenerInventoryRevision;
            if (!unsecureReplayed)
            {
                LogMotherboardAssemblySmokeFailure("smoke.unsecure-replay-mismatch");
                yield break;
            }

            OperationResult detach = playerCarry.TryPickup(motherboard);
            AssemblyBuildSnapshot detachedSnapshot = session.AssemblyBuild.GetSnapshot();
            bool detached = detach.IsSuccess &&
                            playerCarry.HeldItem == motherboard &&
                            detachedSnapshot.MotherboardSeatState == AssemblySeatState.Empty &&
                            detachedSnapshot.MotherboardItemId.IsEmpty &&
                            session.AssemblyBuild.Revision == initialAssemblyRevision + 4 &&
                            session.Inventory.Revision == initialInventoryRevision + 3 &&
                            session.AssemblyBuild.ReceiptCount == initialReceiptCount + 4 &&
                            motherboardBinding.IsAuthorityInHands;
            if (!detached)
            {
                LogMotherboardAssemblySmokeFailure(
                    detach.IsFailure ? detach.Error.Code : "smoke.detach-projection-mismatch");
                yield break;
            }

            OperationResult recovery = playerCarry.TryRecoverHeldItem();
            AssemblyBuildSnapshot recoveredSnapshot = session.AssemblyBuild.GetSnapshot();
            AssemblyOperationReceipt detachReceipt = null;
            AssemblyOperationReceipt recoveryAttachReceipt = null;
            var finalReceipts = session.AssemblyBuild.GetReceipts();
            for (int index = 0; index < finalReceipts.Count; index++)
            {
                AssemblyOperationReceipt receipt = finalReceipts[index];
                if (receipt.OperationKind == AssemblyOperationKind.DetachMotherboard)
                {
                    detachReceipt = receipt;
                }
                else if (receipt.OperationKind == AssemblyOperationKind.AttachMotherboard &&
                         receipt.OperationId != attachReceipt.OperationId)
                {
                    recoveryAttachReceipt = receipt;
                }
            }

            bool receiptLineage = detachReceipt != null &&
                                  recoveryAttachReceipt != null &&
                                  detachReceipt.SourceAttachOperationId ==
                                      attachReceipt.OperationId &&
                                  detachReceipt.AssemblyRevision ==
                                      initialAssemblyRevision + 4 &&
                                  detachReceipt.InventoryRevision ==
                                      initialInventoryRevision + 3 &&
                                  recoveryAttachReceipt.AssemblyRevision ==
                                      initialAssemblyRevision + 5 &&
                                  recoveryAttachReceipt.InventoryRevision ==
                                      initialInventoryRevision + 4 &&
                                  recoveredSnapshot.InstalledByOperationId ==
                                      recoveryAttachReceipt.OperationId;
            bool identityStable = motherboard.GetInstanceID() == physicalInstanceId &&
                                  motherboardBinding.PhysicalItem == motherboard &&
                                  motherboard.ItemIdValue == session.MotherboardItemId.Value &&
                                  motherboardBinding.InventoryItemIdValue ==
                                      session.MotherboardItemId.Value &&
                                  CountCanonicalMotherboardProjections(
                                      session.MotherboardItemId.Value) == 1;
            bool recovered = recovery.IsSuccess &&
                             playerCarry.HeldItem == null &&
                             recoveredSnapshot.MotherboardSeatState ==
                                 AssemblySeatState.SeatedUnsecured &&
                             recoveredSnapshot.MotherboardItemId == session.MotherboardItemId &&
                             session.TryGetMotherboardItem(out InventoryItemRecord recoveredItem) &&
                             recoveredItem.Id == session.MotherboardItemId &&
                             recoveredItem.ProductId == session.MotherboardProductId &&
                             recoveredItem.ContainerId == session.WorkbenchContainerId &&
                             session.Inventory.SerializedItemCount == 7 &&
                             session.TryGetProcessorItem(
                                 out InventoryItemRecord unchangedProcessor) &&
                             unchangedProcessor.Id == session.ProcessorItemId &&
                             unchangedProcessor.ProductId == session.ProcessorProductId &&
                             unchangedProcessor.ContainerId == session.WorldFloorContainerId &&
                             recoveredSnapshot.ProcessorSocketState ==
                                 ProcessorSocketState.EmptyOpen &&
                             session.AssemblyBuild.Revision == initialAssemblyRevision + 5 &&
                             session.Inventory.Revision == initialInventoryRevision + 4 &&
                             session.AssemblyBuild.ReceiptCount == initialReceiptCount + 5 &&
                             motherboardFastener.MatchesAuthorityState(
                                 AssemblySeatState.SeatedUnsecured) &&
                             motherboardBinding.ValidateProjectionInvariant().IsSuccess &&
                             session.ValidateInvariants().IsSuccess;
            bool authorityIsolated = session.Orders.Revision == orderRevision &&
                                     session.RetailOffers.Revision == offerRevision &&
                                     session.RetailBaskets.Revision == basketRevision &&
                                     session.RetailCheckouts.Revision == checkoutRevision &&
                                     session.CheckoutSettlements.Revision == settlementRevision &&
                                     session.CustomerVisits.Revision == visitRevision &&
                                     session.CustomerConsultations.Revision ==
                                         consultationRevision &&
                                     session.CustomerOfferActions.Revision == actionRevision;
            if (!recovered || !identityStable || !authorityIsolated || !receiptLineage)
            {
                LogMotherboardAssemblySmokeFailure(
                    recovery.IsFailure
                        ? recovery.Error.Code
                        : !identityStable
                            ? "smoke.identity-mismatch"
                            : !authorityIsolated
                                ? "smoke.authority-isolation-mismatch"
                                : !receiptLineage
                                    ? "smoke.receipt-lineage-mismatch"
                                    : "smoke.recovery-projection-mismatch");
                yield break;
            }

            long recoveredAssemblyRevision = session.AssemblyBuild.Revision;
            long recoveredInventoryRevision = session.Inventory.Revision;
            int recoveredReceiptCount = session.AssemblyBuild.ReceiptCount;
            Pose recoveredPose = new Pose(
                motherboard.transform.position,
                motherboard.transform.rotation);
            Transform recoveredParent = motherboard.transform.parent;
            OperationResult<AssemblyOperationReceipt> delayedSecureReplay =
                session.SecureMotherboardFastener(
                    secureReceipt.OperationId,
                    attachReceipt.OperationId,
                    secureReceipt.ExpectedAssemblyRevision);
            bool secureDelayedReplayed = delayedSecureReplay.IsSuccess &&
                                         ReferenceEquals(
                                             delayedSecureReplay.Value,
                                             secureReceipt) &&
                                         session.AssemblyBuild.Revision ==
                                             recoveredAssemblyRevision &&
                                         session.AssemblyBuild.ReceiptCount ==
                                             recoveredReceiptCount &&
                                         session.Inventory.Revision ==
                                             recoveredInventoryRevision &&
                                         session.AssemblyBuild.MotherboardSeatState ==
                                             AssemblySeatState.SeatedUnsecured &&
                                         motherboard.transform.parent == recoveredParent &&
                                         ApproximatelySamePose(
                                             new Pose(
                                                 motherboard.transform.position,
                                                 motherboard.transform.rotation),
                                             recoveredPose);
            if (!secureDelayedReplayed)
            {
                LogMotherboardAssemblySmokeFailure(
                    "smoke.secure-delayed-replay-mismatch");
                yield break;
            }

            OperationResult<AssemblyOperationReceipt> delayedUnsecureReplay =
                session.UnsecureMotherboardFastener(
                    unsecureReceipt.OperationId,
                    attachReceipt.OperationId,
                    secureReceipt.OperationId,
                    unsecureReceipt.ExpectedAssemblyRevision);
            bool unsecureDelayedReplayed = delayedUnsecureReplay.IsSuccess &&
                                           ReferenceEquals(
                                               delayedUnsecureReplay.Value,
                                               unsecureReceipt) &&
                                           session.AssemblyBuild.Revision ==
                                               recoveredAssemblyRevision &&
                                           session.AssemblyBuild.ReceiptCount ==
                                               recoveredReceiptCount &&
                                           session.Inventory.Revision ==
                                               recoveredInventoryRevision &&
                                           session.AssemblyBuild.MotherboardSeatState ==
                                               AssemblySeatState.SeatedUnsecured &&
                                           motherboard.transform.parent == recoveredParent &&
                                           ApproximatelySamePose(
                                               new Pose(
                                                   motherboard.transform.position,
                                                   motherboard.transform.rotation),
                                               recoveredPose) &&
                                           motherboardFastener.MatchesAuthorityState(
                                               AssemblySeatState.SeatedUnsecured) &&
                                           motherboardBinding.ValidateProjectionInvariant()
                                               .IsSuccess &&
                                           session.ValidateInvariants().IsSuccess;
            if (!unsecureDelayedReplayed)
            {
                LogMotherboardAssemblySmokeFailure(
                    "smoke.unsecure-delayed-replay-mismatch");
                yield break;
            }

            Debug.Log(
                "GARAGE_MOTHERBOARD_ASSEMBLY_RUNTIME_SMOKE assembly-flow=ok " +
                "compatible=ok mismatch-blocked=ok attach=ok attach-replay=ok " +
                "fastener=ok secure=ok secure-replay=ok secure-delayed-replay=ok " +
                "detach-blocked=ok detach-authority-blocked=ok " +
                "unsecure=ok unsecure-replay=ok unsecure-delayed-replay=ok detach=ok " +
                "duplicate-seat-confirm-blocked=ok authority-isolated=ok " +
                "identity-stable=ok recovery=ok");
            yield return new WaitForEndOfFrame();
        }

        private IEnumerator RunProcessorSocketSmoke()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return null;

            GarageStockFlowSession session = stockFlow != null
                ? stockFlow.EnsureInitialized()
                : null;
            if (playerMotor == null ||
                playerCarry == null ||
                session == null ||
                motherboardBinding == null ||
                motherboardSeat == null ||
                motherboardFastener == null ||
                processorSocket == null ||
                processorBinding == null ||
                processor == null)
            {
                LogProcessorSocketSmokeFailure("smoke.context-missing");
                yield break;
            }

            Pose initialProcessorPose = new Pose(
                processor.transform.position,
                processor.transform.rotation);
            Transform initialProcessorParent = processor.transform.parent;
            int initialProcessorInstanceId = processor.GetInstanceID();

            bool preflight = session.AssemblyBuild.HasProcessorSocket &&
                             session.AssemblyBuild.MotherboardSeatState ==
                                 AssemblySeatState.Empty &&
                             session.AssemblyBuild.ProcessorSocketState ==
                                 ProcessorSocketState.EmptyOpen &&
                             session.TryGetProcessorItem(
                                 out InventoryItemRecord looseProcessor) &&
                             looseProcessor.Id == session.ProcessorItemId &&
                             looseProcessor.ProductId == session.ProcessorProductId &&
                             looseProcessor.ContainerId == session.WorldFloorContainerId &&
                             CountCanonicalProcessorProjections(
                                 session.ProcessorItemId.Value) == 1 &&
                             processorBinding.ValidateProjectionInvariant().IsSuccess;
            if (!preflight)
            {
                LogProcessorSocketSmokeFailure("smoke.preflight-mismatch");
                yield break;
            }

            OperationResult motherboardPickup = playerCarry.TryPickup(
                motherboardBinding.PhysicalItem);
            MovePlayerToMotherboardSeat();
            OperationResult motherboardMode =
                playerCarry.TrySetMotherboardSeatMode(true);
            OperationResult motherboardAttach = playerCarry.TryConfirmMotherboardSeat();
            MovePlayerToMotherboardFastener();
            OperationResult motherboardSecure =
                playerCarry.TryOperateMotherboardFastener();
            if (motherboardPickup.IsFailure ||
                motherboardMode.IsFailure ||
                motherboardAttach.IsFailure ||
                motherboardSecure.IsFailure ||
                session.AssemblyBuild.MotherboardSeatState !=
                    AssemblySeatState.SeatedSecured)
            {
                LogProcessorSocketSmokeFailure("smoke.motherboard-preflight-failed");
                yield break;
            }

            OperationResult processorPickup = playerCarry.TryPickup(processor);
            MovePlayerToProcessorSocket();
            OperationResult processorMode = playerCarry.TrySetProcessorSeatMode(true);
            long keyedAssemblyRevision = session.AssemblyBuild.Revision;
            long keyedInventoryRevision = session.Inventory.Revision;
            int keyedReceiptCount = session.AssemblyBuild.ReceiptCount;
            Pose carriedProcessorPose = new Pose(
                processor.transform.position,
                processor.transform.rotation);
            OperationResult wrongOrientation =
                playerCarry.TryRotateProcessorSeatPreviewClockwise();
            OperationResult wrongOrientationConfirm =
                playerCarry.TryConfirmProcessorSeat();
            bool wrongOrientationBlocked = wrongOrientation.IsSuccess &&
                                           wrongOrientationConfirm.Error.Code ==
                                               "assembly-processor.orientation-invalid" &&
                                           !playerCarry.PlacementValid &&
                                           playerCarry.CurrentProcessorSocketStatus ==
                                               ProcessorSocketStatus.OrientationInvalid &&
                                           playerCarry.HeldItem == processor &&
                                           session.AssemblyBuild.Revision ==
                                               keyedAssemblyRevision &&
                                           session.Inventory.Revision ==
                                               keyedInventoryRevision &&
                                           session.AssemblyBuild.ReceiptCount ==
                                               keyedReceiptCount &&
                                           ApproximatelySamePose(
                                               new Pose(
                                                   processor.transform.position,
                                                   processor.transform.rotation),
                                               carriedProcessorPose);
            for (int turn = 0; turn < 3; turn++)
            {
                playerCarry.TryRotateProcessorSeatPreviewClockwise();
            }

            bool validPreview = processorMode.IsSuccess &&
                                wrongOrientationBlocked &&
                                playerCarry.IsProcessorSeatMode &&
                                playerCarry.PlacementRotationQuarterTurns == 0 &&
                                playerCarry.PlacementValid &&
                                playerCarry.CurrentProcessorSocketStatus ==
                                    ProcessorSocketStatus.ValidSeat &&
                                session.AssemblyBuild.Revision == keyedAssemblyRevision &&
                                session.Inventory.Revision == keyedInventoryRevision &&
                                session.AssemblyBuild.ReceiptCount == keyedReceiptCount;
            OperationResult processorSeat = playerCarry.TryConfirmProcessorSeat();
            if (processorPickup.IsFailure ||
                !validPreview ||
                processorSeat.IsFailure ||
                session.AssemblyBuild.ProcessorSocketState !=
                    ProcessorSocketState.ProcessorSeatedOpen ||
                processorBinding.ValidateProjectionInvariant().IsFailure)
            {
                LogProcessorSocketSmokeFailure("smoke.processor-seat-failed");
                yield break;
            }

            var receiptsAfterSeat = session.AssemblyBuild.GetReceipts();
            AssemblyOperationReceipt seatReceipt =
                receiptsAfterSeat[receiptsAfterSeat.Count - 1];
            MovePlayerToProcessorSocket();
            OperationResult close = playerCarry.TryOperateProcessorRetention();
            var receiptsAfterClose = session.AssemblyBuild.GetReceipts();
            AssemblyOperationReceipt closeReceipt =
                receiptsAfterClose[receiptsAfterClose.Count - 1];
            bool retained = close.IsSuccess &&
                            closeReceipt.OperationKind ==
                                AssemblyOperationKind.CloseProcessorRetention &&
                            session.AssemblyBuild.ProcessorSocketState ==
                                ProcessorSocketState.ProcessorRetained &&
                            processorSocket.MatchesAuthorityState(
                                AssemblySeatState.SeatedSecured,
                                ProcessorSocketState.ProcessorRetained);
            long retainedGateAssemblyRevision = session.AssemblyBuild.Revision;
            long retainedGateInventoryRevision = session.Inventory.Revision;
            int retainedGateReceiptCount = session.AssemblyBuild.ReceiptCount;
            Pose retainedGatePose = new Pose(
                processor.transform.position,
                processor.transform.rotation);
            OperationResult retainedRemoval = playerCarry.TryPickup(processor);
            bool retainedGate = retainedRemoval.IsFailure &&
                                retainedRemoval.Error ==
                                    AssemblyFailures.ProcessorRetained &&
                                playerCarry.HeldItem == null &&
                                session.AssemblyBuild.ProcessorSocketState ==
                                    ProcessorSocketState.ProcessorRetained &&
                                session.AssemblyBuild.Revision ==
                                    retainedGateAssemblyRevision &&
                                session.Inventory.Revision ==
                                    retainedGateInventoryRevision &&
                                session.AssemblyBuild.ReceiptCount ==
                                    retainedGateReceiptCount &&
                                ApproximatelySamePose(
                                    new Pose(
                                        processor.transform.position,
                                        processor.transform.rotation),
                                    retainedGatePose);
            OperationResult open = playerCarry.TryOperateProcessorRetention();
            var receiptsAfterOpen = session.AssemblyBuild.GetReceipts();
            AssemblyOperationReceipt openReceipt =
                receiptsAfterOpen[receiptsAfterOpen.Count - 1];
            bool reopened = open.IsSuccess &&
                            session.AssemblyBuild.ProcessorSocketState ==
                                ProcessorSocketState.ProcessorSeatedOpen &&
                            processorSocket.MatchesAuthorityState(
                                AssemblySeatState.SeatedSecured,
                                ProcessorSocketState.ProcessorSeatedOpen);
            if (!retained || !retainedGate || !reopened)
            {
                LogProcessorSocketSmokeFailure("smoke.retention-cycle-failed");
                yield break;
            }

            OperationResult remove = playerCarry.TryPickup(processor);
            var receiptsAfterRemove = session.AssemblyBuild.GetReceipts();
            AssemblyOperationReceipt removeReceipt =
                receiptsAfterRemove[receiptsAfterRemove.Count - 1];
            if (remove.IsFailure ||
                playerCarry.HeldItem != processor ||
                !processorBinding.IsAuthorityInHands ||
                session.AssemblyBuild.ProcessorSocketState !=
                    ProcessorSocketState.EmptyOpen)
            {
                LogProcessorSocketSmokeFailure("smoke.processor-remove-failed");
                yield break;
            }

            MovePlayerToMotherboardFastener();
            OperationResult motherboardUnsecure =
                playerCarry.TryOperateMotherboardFastener();
            OperationResult recovery = playerCarry.TryRecoverHeldItem();
            long finalAssemblyRevision = session.AssemblyBuild.Revision;
            int finalReceiptCount = session.AssemblyBuild.ReceiptCount;
            long finalInventoryRevision = session.Inventory.Revision;
            OperationResult<AssemblyOperationReceipt> delayedSeatReplay =
                session.SeatProcessor(
                    seatReceipt.OperationId,
                    seatReceipt.SourceAttachOperationId,
                    seatReceipt.SourceSecureOperationId,
                    seatReceipt.ExpectedAssemblyRevision);
            OperationResult<AssemblyOperationReceipt> delayedCloseReplay =
                session.CloseProcessorRetention(
                    closeReceipt.OperationId,
                    seatReceipt.OperationId,
                    closeReceipt.ExpectedAssemblyRevision);
            OperationResult<AssemblyOperationReceipt> delayedOpenReplay =
                session.OpenProcessorRetention(
                    openReceipt.OperationId,
                    openReceipt.SourceProcessorSeatOperationId,
                    openReceipt.SourceProcessorRetentionOperationId,
                    openReceipt.ExpectedAssemblyRevision);
            OperationResult<AssemblyOperationReceipt> delayedRemoveReplay =
                session.RemoveProcessor(
                    removeReceipt.OperationId,
                    removeReceipt.SourceProcessorSeatOperationId,
                    removeReceipt.ExpectedAssemblyRevision);
            bool replayStable = delayedSeatReplay.IsSuccess &&
                                ReferenceEquals(delayedSeatReplay.Value, seatReceipt) &&
                                delayedCloseReplay.IsSuccess &&
                                ReferenceEquals(
                                    delayedCloseReplay.Value,
                                    closeReceipt) &&
                                delayedOpenReplay.IsSuccess &&
                                ReferenceEquals(delayedOpenReplay.Value, openReceipt) &&
                                delayedRemoveReplay.IsSuccess &&
                                ReferenceEquals(delayedRemoveReplay.Value, removeReceipt) &&
                                session.AssemblyBuild.Revision == finalAssemblyRevision &&
                                session.AssemblyBuild.ReceiptCount == finalReceiptCount &&
                                session.Inventory.Revision == finalInventoryRevision;
            bool recovered = motherboardUnsecure.IsSuccess &&
                             recovery.IsSuccess &&
                             playerCarry.HeldItem == null &&
                             session.AssemblyBuild.MotherboardSeatState ==
                                 AssemblySeatState.SeatedUnsecured &&
                             session.AssemblyBuild.ProcessorSocketState ==
                                 ProcessorSocketState.EmptyOpen &&
                             session.TryGetProcessorItem(
                                 out InventoryItemRecord recoveredProcessor) &&
                             recoveredProcessor.Id == session.ProcessorItemId &&
                             recoveredProcessor.ProductId ==
                                 session.ProcessorProductId &&
                             recoveredProcessor.ContainerId ==
                                 session.WorldFloorContainerId &&
                             processor.GetInstanceID() == initialProcessorInstanceId &&
                             processor.transform.parent == initialProcessorParent &&
                             ApproximatelySamePose(
                                 new Pose(
                                     processor.transform.position,
                                     processor.transform.rotation),
                                 initialProcessorPose) &&
                             ApproximatelySamePose(
                                 new Pose(
                                     processor.Body.position,
                                     processor.Body.rotation),
                                 initialProcessorPose) &&
                             ApproximatelySamePose(
                                 new Pose(
                                     processor.LastSafePosition,
                                     processor.LastSafeRotation),
                                 initialProcessorPose) &&
                             processor.Ownership == PhysicalItemOwnership.World &&
                             processor.IsStablePlacement &&
                             CountCanonicalProcessorProjections(
                                 session.ProcessorItemId.Value) == 1 &&
                             session.Inventory.SerializedItemCount == 7 &&
                             session.Inventory.GetContainerQuantity(
                                 session.HandsContainerId).Value == 0 &&
                             session.Inventory.GetContainerQuantity(
                                 session.ProcessorSocketContainerId).Value == 0 &&
                             processorBinding.ValidateProjectionInvariant().IsSuccess &&
                             session.ValidateInvariants().IsSuccess;
            if (!recovered || !replayStable)
            {
                LogProcessorSocketSmokeFailure(
                    !recovered
                        ? "smoke.recovery-failed"
                        : "smoke.delayed-replay-failed");
                yield break;
            }

            Debug.Log(
                "GARAGE_CPU_SOCKET_RUNTIME_SMOKE cpu-socket-flow=ok " +
                "preflight=ok retention-cycle=ok recovery=ok " +
                "keyed-orientation=ok retained-remove-gate=ok replay=ok identity=stable");
            yield return new WaitForEndOfFrame();
        }

        private IEnumerator RunDimmSlotSmoke()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return null;

            GarageStockFlowSession session = stockFlow != null
                ? stockFlow.EnsureInitialized()
                : null;
            if (playerMotor == null ||
                playerCarry == null ||
                session == null ||
                motherboardBinding == null ||
                motherboardSeat == null ||
                motherboardFastener == null ||
                dimmSlot == null ||
                dimmBinding == null ||
                memoryModule == null)
            {
                LogDimmSlotSmokeFailure("smoke.context-missing");
                yield break;
            }

            Pose initialMemoryPose = new Pose(
                memoryModule.transform.position,
                memoryModule.transform.rotation);
            Transform initialMemoryParent = memoryModule.transform.parent;
            int initialMemoryInstanceId = memoryModule.GetInstanceID();
            bool slotChannel = dimmSlot.IsConfigured &&
                               dimmSlot.SlotIdValue == session.MemorySlotId.Value &&
                               dimmSlot.RetentionIdValue == session.MemoryRetentionId.Value &&
                               dimmSlot.ChannelIdValue == session.MemoryChannelId.Value &&
                               dimmSlot.BankIdValue == session.MemoryBankId.Value &&
                               dimmBinding.Slot == dimmSlot &&
                               playerCarry.MatchesDimmConfiguration(dimmSlot, dimmBinding);
            bool preflight = slotChannel &&
                             session.AssemblyBuild.HasMemorySlot &&
                             session.AssemblyBuild.MotherboardSeatState ==
                                 AssemblySeatState.Empty &&
                             session.AssemblyBuild.MemorySlotState ==
                                 MemorySlotState.EmptyOpen &&
                             session.TryGetMemoryItem(
                                 out InventoryItemRecord looseMemory) &&
                             looseMemory.Id == session.MemoryItemId &&
                             looseMemory.ProductId == session.MemoryProductId &&
                             looseMemory.ContainerId == session.WorldFloorContainerId &&
                             CountCanonicalMemoryProjections(
                                 session.MemoryItemId.Value) == 1 &&
                             session.Inventory.SerializedItemCount == 7 &&
                             dimmBinding.ValidateProjectionInvariant().IsSuccess;
            if (!preflight)
            {
                LogDimmSlotSmokeFailure("smoke.preflight-mismatch");
                yield break;
            }

            long orderRevision = session.Orders.Revision;
            long offerRevision = session.RetailOffers.Revision;
            long basketRevision = session.RetailBaskets.Revision;
            long checkoutRevision = session.RetailCheckouts.Revision;
            long settlementRevision = session.CheckoutSettlements.Revision;
            long visitRevision = session.CustomerVisits.Revision;
            long consultationRevision = session.CustomerConsultations.Revision;
            long actionRevision = session.CustomerOfferActions.Revision;

            OperationResult motherboardPickup = playerCarry.TryPickup(
                motherboardBinding.PhysicalItem);
            MovePlayerToMotherboardSeat();
            OperationResult motherboardMode =
                playerCarry.TrySetMotherboardSeatMode(true);
            OperationResult motherboardAttach = playerCarry.TryConfirmMotherboardSeat();
            MovePlayerToMotherboardFastener();
            OperationResult motherboardSecure =
                playerCarry.TryOperateMotherboardFastener();
            if (motherboardPickup.IsFailure ||
                motherboardMode.IsFailure ||
                motherboardAttach.IsFailure ||
                motherboardSecure.IsFailure ||
                session.AssemblyBuild.MotherboardSeatState !=
                    AssemblySeatState.SeatedSecured)
            {
                LogDimmSlotSmokeFailure("smoke.motherboard-preflight-failed");
                yield break;
            }

            OperationResult memoryPickup = playerCarry.TryPickup(memoryModule);
            MovePlayerToDimmSlot();
            OperationResult memoryMode = playerCarry.TrySetDimmSeatMode(true);
            long keyedAssemblyRevision = session.AssemblyBuild.Revision;
            long keyedInventoryRevision = session.Inventory.Revision;
            int keyedReceiptCount = session.AssemblyBuild.ReceiptCount;
            Pose carriedMemoryPose = new Pose(
                memoryModule.transform.position,
                memoryModule.transform.rotation);
            OperationResult wrongOrientation =
                playerCarry.TryRotateDimmSeatPreviewClockwise();
            OperationResult wrongOrientationConfirm =
                playerCarry.TryConfirmDimmSeat();
            bool wrongOrientationBlocked = wrongOrientation.IsSuccess &&
                                           wrongOrientationConfirm.IsFailure &&
                                           wrongOrientationConfirm.Error ==
                                               AssemblyFailures.DimmOrientationMismatch &&
                                           !playerCarry.PlacementValid &&
                                           playerCarry.CurrentDimmSlotStatus ==
                                               DimmSlotStatus.OrientationInvalid &&
                                           playerCarry.HeldItem == memoryModule &&
                                           session.AssemblyBuild.Revision ==
                                               keyedAssemblyRevision &&
                                           session.Inventory.Revision ==
                                               keyedInventoryRevision &&
                                           session.AssemblyBuild.ReceiptCount ==
                                               keyedReceiptCount &&
                                           ApproximatelySamePose(
                                               new Pose(
                                                   memoryModule.transform.position,
                                                   memoryModule.transform.rotation),
                                               carriedMemoryPose);
            playerCarry.TryRotateDimmSeatPreviewClockwise();

            bool validPreview = memoryMode.IsSuccess &&
                                wrongOrientationBlocked &&
                                playerCarry.IsDimmSeatMode &&
                                playerCarry.PlacementRotationQuarterTurns == 0 &&
                                playerCarry.PlacementValid &&
                                playerCarry.CurrentDimmSlotStatus ==
                                    DimmSlotStatus.ValidSeat &&
                                session.AssemblyBuild.Revision == keyedAssemblyRevision &&
                                session.Inventory.Revision == keyedInventoryRevision &&
                                session.AssemblyBuild.ReceiptCount == keyedReceiptCount;
            OperationResult memorySeat = playerCarry.TryConfirmDimmSeat();
            var receiptsAfterSeat = session.AssemblyBuild.GetReceipts();
            AssemblyOperationReceipt seatReceipt =
                receiptsAfterSeat[receiptsAfterSeat.Count - 1];
            if (memoryPickup.IsFailure ||
                !validPreview ||
                memorySeat.IsFailure ||
                seatReceipt.OperationKind != AssemblyOperationKind.SeatMemoryModule ||
                seatReceipt.DimmKeyOrientation != DimmKeyOrientation.NotchAligned ||
                session.AssemblyBuild.MemorySlotState !=
                    MemorySlotState.MemoryModuleSeatedOpen ||
                dimmBinding.ValidateProjectionInvariant().IsFailure)
            {
                LogDimmSlotSmokeFailure("smoke.memory-seat-failed");
                yield break;
            }

            long duplicateAssemblyRevision = session.AssemblyBuild.Revision;
            long duplicateInventoryRevision = session.Inventory.Revision;
            int duplicateReceiptCount = session.AssemblyBuild.ReceiptCount;
            OperationResult duplicateSeat = dimmBinding.TryAttachAt(
                dimmSlot.SnapPose,
                DimmKeyOrientation.NotchAligned);
            bool duplicateSeatBlocked = duplicateSeat.IsFailure &&
                                        duplicateSeat.Error.Code ==
                                            "assembly-memory.attach-authority-mismatch" &&
                                        session.AssemblyBuild.Revision ==
                                            duplicateAssemblyRevision &&
                                        session.Inventory.Revision ==
                                            duplicateInventoryRevision &&
                                        session.AssemblyBuild.ReceiptCount ==
                                            duplicateReceiptCount &&
                                        session.AssemblyBuild.MemorySlotState ==
                                            MemorySlotState.MemoryModuleSeatedOpen;

            MovePlayerToDimmSlot();
            long closeAssemblyRevision = session.AssemblyBuild.Revision;
            long closeInventoryRevision = session.Inventory.Revision;
            int closeReceiptCount = session.AssemblyBuild.ReceiptCount;
            OperationResult close = playerCarry.TryOperateDimmRetention();
            var receiptsAfterClose = session.AssemblyBuild.GetReceipts();
            AssemblyOperationReceipt closeReceipt =
                receiptsAfterClose[receiptsAfterClose.Count - 1];
            bool closeAuthorityIsolated = close.IsSuccess &&
                                          closeReceipt.OperationKind ==
                                              AssemblyOperationKind.CloseMemoryRetention &&
                                          session.AssemblyBuild.Revision ==
                                              closeAssemblyRevision + 1 &&
                                          session.AssemblyBuild.ReceiptCount ==
                                              closeReceiptCount + 1 &&
                                          session.Inventory.Revision ==
                                              closeInventoryRevision &&
                                          session.AssemblyBuild.MemorySlotState ==
                                              MemorySlotState.MemoryModuleRetained &&
                                          dimmSlot.LatchVisualPhase ==
                                              DimmLatchVisualPhase.ClosingLeft &&
                                          dimmBinding.ValidateProjectionInvariant().IsSuccess;
            dimmSlot.AdvanceLatchAnimation(0.10f);
            bool closingRight = dimmSlot.LatchVisualPhase ==
                                DimmLatchVisualPhase.ClosingRight &&
                                session.AssemblyBuild.Revision ==
                                    closeAssemblyRevision + 1 &&
                                session.AssemblyBuild.ReceiptCount ==
                                    closeReceiptCount + 1 &&
                                session.Inventory.Revision == closeInventoryRevision;
            dimmSlot.AdvanceLatchAnimation(0.10f);
            bool closeStable = dimmSlot.MatchesAuthorityState(
                AssemblySeatState.SeatedSecured,
                MemorySlotState.MemoryModuleRetained);

            long retainedGateAssemblyRevision = session.AssemblyBuild.Revision;
            long retainedGateInventoryRevision = session.Inventory.Revision;
            int retainedGateReceiptCount = session.AssemblyBuild.ReceiptCount;
            Pose retainedPose = new Pose(
                memoryModule.transform.position,
                memoryModule.transform.rotation);
            OperationResult retainedRemoval = playerCarry.TryPickup(memoryModule);
            bool retainedGate = retainedRemoval.IsFailure &&
                                retainedRemoval.Error ==
                                    AssemblyFailures.MemoryModuleRetained &&
                                playerCarry.HeldItem == null &&
                                session.AssemblyBuild.Revision ==
                                    retainedGateAssemblyRevision &&
                                session.Inventory.Revision ==
                                    retainedGateInventoryRevision &&
                                session.AssemblyBuild.ReceiptCount ==
                                    retainedGateReceiptCount &&
                                ApproximatelySamePose(
                                    new Pose(
                                        memoryModule.transform.position,
                                        memoryModule.transform.rotation),
                                    retainedPose);

            MovePlayerToMotherboardFastener();
            OperationResult motherboardUnsecure =
                playerCarry.TryOperateMotherboardFastener();
            long hostGateAssemblyRevision = session.AssemblyBuild.Revision;
            long hostGateInventoryRevision = session.Inventory.Revision;
            int hostGateReceiptCount = session.AssemblyBuild.ReceiptCount;
            OperationResult hostDetach = playerCarry.TryPickup(
                motherboardBinding.PhysicalItem);
            bool hostDetachGate = motherboardUnsecure.IsSuccess &&
                                  hostDetach.IsFailure &&
                                  hostDetach.Error ==
                                      AssemblyFailures.MemoryModuleInstalled &&
                                  playerCarry.HeldItem == null &&
                                  session.AssemblyBuild.MotherboardSeatState ==
                                      AssemblySeatState.SeatedUnsecured &&
                                  session.AssemblyBuild.MemorySlotState ==
                                      MemorySlotState.MemoryModuleRetained &&
                                  session.AssemblyBuild.Revision ==
                                      hostGateAssemblyRevision &&
                                  session.Inventory.Revision ==
                                      hostGateInventoryRevision &&
                                  session.AssemblyBuild.ReceiptCount ==
                                      hostGateReceiptCount;

            MovePlayerToDimmSlot();
            long openAssemblyRevision = session.AssemblyBuild.Revision;
            long openInventoryRevision = session.Inventory.Revision;
            int openReceiptCount = session.AssemblyBuild.ReceiptCount;
            OperationResult open = playerCarry.TryOperateDimmRetention();
            var receiptsAfterOpen = session.AssemblyBuild.GetReceipts();
            AssemblyOperationReceipt openReceipt =
                receiptsAfterOpen[receiptsAfterOpen.Count - 1];
            bool openAuthorityIsolated = open.IsSuccess &&
                                         openReceipt.OperationKind ==
                                             AssemblyOperationKind.OpenMemoryRetention &&
                                         session.AssemblyBuild.Revision ==
                                             openAssemblyRevision + 1 &&
                                         session.AssemblyBuild.ReceiptCount ==
                                             openReceiptCount + 1 &&
                                         session.Inventory.Revision ==
                                             openInventoryRevision &&
                                         session.AssemblyBuild.MemorySlotState ==
                                             MemorySlotState.MemoryModuleSeatedOpen &&
                                         dimmSlot.LatchVisualPhase ==
                                             DimmLatchVisualPhase.OpeningRight &&
                                         dimmBinding.ValidateProjectionInvariant().IsSuccess;
            dimmSlot.AdvanceLatchAnimation(0.10f);
            bool openingLeft = dimmSlot.LatchVisualPhase ==
                               DimmLatchVisualPhase.OpeningLeft &&
                               session.AssemblyBuild.Revision ==
                                   openAssemblyRevision + 1 &&
                               session.AssemblyBuild.ReceiptCount ==
                                   openReceiptCount + 1 &&
                               session.Inventory.Revision == openInventoryRevision;
            dimmSlot.AdvanceLatchAnimation(0.10f);
            bool openStable = dimmSlot.MatchesAuthorityState(
                AssemblySeatState.SeatedUnsecured,
                MemorySlotState.MemoryModuleSeatedOpen);
            bool latchOrder = closeAuthorityIsolated &&
                              closingRight &&
                              closeStable &&
                              openAuthorityIsolated &&
                              openingLeft &&
                              openStable;
            bool dimmTransitionsIsolated = duplicateSeatBlocked &&
                                           closeAuthorityIsolated &&
                                           openAuthorityIsolated &&
                                           retainedGate &&
                                           hostDetachGate;
            if (!latchOrder || !retainedGate || !hostDetachGate)
            {
                LogDimmSlotSmokeFailure("smoke.retention-cycle-failed");
                yield break;
            }

            OperationResult remove = playerCarry.TryPickup(memoryModule);
            var receiptsAfterRemove = session.AssemblyBuild.GetReceipts();
            AssemblyOperationReceipt removeReceipt =
                receiptsAfterRemove[receiptsAfterRemove.Count - 1];
            if (remove.IsFailure ||
                removeReceipt.OperationKind != AssemblyOperationKind.RemoveMemoryModule ||
                playerCarry.HeldItem != memoryModule ||
                !dimmBinding.IsAuthorityInHands ||
                session.AssemblyBuild.MemorySlotState != MemorySlotState.EmptyOpen)
            {
                LogDimmSlotSmokeFailure("smoke.memory-remove-failed");
                yield break;
            }

            OperationResult recovery = playerCarry.TryRecoverHeldItem();
            long finalAssemblyRevision = session.AssemblyBuild.Revision;
            int finalReceiptCount = session.AssemblyBuild.ReceiptCount;
            long finalInventoryRevision = session.Inventory.Revision;
            OperationResult<AssemblyOperationReceipt> delayedSeatReplay =
                session.SeatMemoryModule(
                    seatReceipt.OperationId,
                    seatReceipt.DimmKeyOrientation,
                    seatReceipt.SourceAttachOperationId,
                    seatReceipt.SourceSecureOperationId,
                    seatReceipt.ExpectedAssemblyRevision);
            OperationResult<AssemblyOperationReceipt> delayedCloseReplay =
                session.CloseMemoryRetention(
                    closeReceipt.OperationId,
                    closeReceipt.SourceMemorySeatOperationId,
                    closeReceipt.ExpectedAssemblyRevision);
            OperationResult<AssemblyOperationReceipt> delayedOpenReplay =
                session.OpenMemoryRetention(
                    openReceipt.OperationId,
                    openReceipt.SourceMemorySeatOperationId,
                    openReceipt.SourceMemoryRetentionOperationId,
                    openReceipt.ExpectedAssemblyRevision);
            OperationResult<AssemblyOperationReceipt> delayedRemoveReplay =
                session.RemoveMemoryModule(
                    removeReceipt.OperationId,
                    removeReceipt.SourceMemorySeatOperationId,
                    removeReceipt.ExpectedAssemblyRevision);
            bool replayStable = delayedSeatReplay.IsSuccess &&
                                ReferenceEquals(delayedSeatReplay.Value, seatReceipt) &&
                                delayedCloseReplay.IsSuccess &&
                                ReferenceEquals(delayedCloseReplay.Value, closeReceipt) &&
                                delayedOpenReplay.IsSuccess &&
                                ReferenceEquals(delayedOpenReplay.Value, openReceipt) &&
                                delayedRemoveReplay.IsSuccess &&
                                ReferenceEquals(delayedRemoveReplay.Value, removeReceipt) &&
                                session.AssemblyBuild.Revision == finalAssemblyRevision &&
                                session.AssemblyBuild.ReceiptCount == finalReceiptCount &&
                                session.Inventory.Revision == finalInventoryRevision;
            bool recovered = recovery.IsSuccess &&
                             playerCarry.HeldItem == null &&
                             session.AssemblyBuild.MotherboardSeatState ==
                                 AssemblySeatState.SeatedUnsecured &&
                             session.AssemblyBuild.MemorySlotState ==
                                 MemorySlotState.EmptyOpen &&
                             session.TryGetMemoryItem(
                                 out InventoryItemRecord recoveredMemory) &&
                             recoveredMemory.Id == session.MemoryItemId &&
                             recoveredMemory.ProductId == session.MemoryProductId &&
                             recoveredMemory.ContainerId == session.WorldFloorContainerId &&
                             memoryModule.GetInstanceID() == initialMemoryInstanceId &&
                             memoryModule.transform.parent == initialMemoryParent &&
                             ApproximatelySamePose(
                                 new Pose(
                                     memoryModule.transform.position,
                                     memoryModule.transform.rotation),
                                 initialMemoryPose) &&
                             ApproximatelySamePose(
                                 new Pose(
                                     memoryModule.Body.position,
                                     memoryModule.Body.rotation),
                                 initialMemoryPose) &&
                             ApproximatelySamePose(
                                 new Pose(
                                     memoryModule.LastSafePosition,
                                     memoryModule.LastSafeRotation),
                                 initialMemoryPose) &&
                             memoryModule.Ownership == PhysicalItemOwnership.World &&
                             memoryModule.IsStablePlacement &&
                             CountCanonicalMemoryProjections(
                                 session.MemoryItemId.Value) == 1 &&
                             session.Inventory.SerializedItemCount == 7 &&
                             session.Inventory.GetContainerQuantity(
                                 session.HandsContainerId).Value == 0 &&
                             session.Inventory.GetContainerQuantity(
                                 session.MemorySlotContainerId).Value == 0 &&
                             dimmBinding.ValidateProjectionInvariant().IsSuccess &&
                             session.ValidateInvariants().IsSuccess;
            bool authorityIsolated = dimmTransitionsIsolated &&
                                     session.Orders.Revision == orderRevision &&
                                     session.RetailOffers.Revision == offerRevision &&
                                     session.RetailBaskets.Revision == basketRevision &&
                                     session.RetailCheckouts.Revision == checkoutRevision &&
                                     session.CheckoutSettlements.Revision == settlementRevision &&
                                     session.CustomerVisits.Revision == visitRevision &&
                                     session.CustomerConsultations.Revision ==
                                         consultationRevision &&
                                     session.CustomerOfferActions.Revision == actionRevision &&
                                     session.AssemblyBuild.ProcessorSocketState ==
                                         ProcessorSocketState.EmptyOpen &&
                                     session.TryGetProcessorItem(
                                         out InventoryItemRecord unchangedProcessor) &&
                                     unchangedProcessor.Id == session.ProcessorItemId &&
                                     unchangedProcessor.ProductId ==
                                         session.ProcessorProductId &&
                                     unchangedProcessor.ContainerId ==
                                         session.WorldFloorContainerId;
            if (!recovered || !replayStable || !duplicateSeatBlocked || !authorityIsolated)
            {
                LogDimmSlotSmokeFailure(
                    !recovered
                        ? "smoke.recovery-failed"
                        : !replayStable
                            ? "smoke.delayed-replay-failed"
                            : !duplicateSeatBlocked
                                ? "smoke.duplicate-seat-not-blocked"
                                : "smoke.authority-isolation-failed");
                yield break;
            }

            Debug.Log(
                "GARAGE_DIMM_RUNTIME_SMOKE dimm-flow=ok preflight=ok " +
                "slot-channel=ok keyed-orientation=ok latch-order=ok " +
                "duplicate-seat-blocked=ok retained-remove-gate=ok " +
                "host-detach-gate=ok replay=ok authority-isolated=ok " +
                "identity=stable recovery=ok");
            yield return new WaitForEndOfFrame();
        }

        private IEnumerator RunM2StorageSmoke()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return null;

            GarageStockFlowSession session = stockFlow != null
                ? stockFlow.EnsureInitialized()
                : null;
            if (playerMotor == null ||
                playerCarry == null ||
                session == null ||
                motherboardBinding == null ||
                motherboardSeat == null ||
                motherboardFastener == null ||
                storageSlot == null ||
                storageBinding == null ||
                storageDevice == null)
            {
                LogM2StorageSmokeFailure("smoke.context-missing");
                yield break;
            }

            Pose initialPose = new Pose(
                storageDevice.transform.position,
                storageDevice.transform.rotation);
            Transform initialParent = storageDevice.transform.parent;
            int initialInstanceId = storageDevice.GetInstanceID();
            bool slotInterface = storageSlot.IsConfigured &&
                                 storageSlot.SlotIdValue == session.StorageSlotId.Value &&
                                 storageSlot.StandoffIdValue ==
                                     session.StorageStandoffId.Value &&
                                 storageSlot.CaptiveScrewIdValue ==
                                     session.StorageCaptiveScrewId.Value &&
                                 storageBinding.Slot == storageSlot &&
                                 playerCarry.MatchesM2StorageConfiguration(
                                     storageSlot,
                                     storageBinding);
            bool preflight = slotInterface &&
                             session.AssemblyBuild.HasStorageSlot &&
                             session.AssemblyBuild.MotherboardSeatState ==
                                 AssemblySeatState.Empty &&
                             session.AssemblyBuild.StorageSlotState ==
                                 StorageSlotState.EmptyOpen &&
                             session.TryGetStorageItem(
                                 out InventoryItemRecord looseStorage) &&
                             looseStorage.Id == session.StorageItemId &&
                             looseStorage.ProductId == session.StorageProductId &&
                             looseStorage.ContainerId == session.WorldFloorContainerId &&
                             CountCanonicalStorageProjections(
                                 session.StorageItemId.Value) == 1 &&
                             session.Inventory.SerializedItemCount == 7 &&
                             storageBinding.ValidateProjectionInvariant().IsSuccess;
            if (!preflight)
            {
                LogM2StorageSmokeFailure("smoke.preflight-mismatch");
                yield break;
            }

            long orderRevision = session.Orders.Revision;
            long offerRevision = session.RetailOffers.Revision;
            long basketRevision = session.RetailBaskets.Revision;
            long checkoutRevision = session.RetailCheckouts.Revision;
            long settlementRevision = session.CheckoutSettlements.Revision;
            long visitRevision = session.CustomerVisits.Revision;
            long consultationRevision = session.CustomerConsultations.Revision;
            long actionRevision = session.CustomerOfferActions.Revision;

            OperationResult motherboardPickup = playerCarry.TryPickup(
                motherboardBinding.PhysicalItem);
            MovePlayerToMotherboardSeat();
            OperationResult motherboardMode =
                playerCarry.TrySetMotherboardSeatMode(true);
            OperationResult motherboardAttach = playerCarry.TryConfirmMotherboardSeat();
            MovePlayerToMotherboardFastener();
            OperationResult motherboardSecure =
                playerCarry.TryOperateMotherboardFastener();
            if (motherboardPickup.IsFailure ||
                motherboardMode.IsFailure ||
                motherboardAttach.IsFailure ||
                motherboardSecure.IsFailure ||
                session.AssemblyBuild.MotherboardSeatState !=
                    AssemblySeatState.SeatedSecured)
            {
                LogM2StorageSmokeFailure("smoke.motherboard-preflight-failed");
                yield break;
            }

            OperationResult storagePickup = playerCarry.TryPickup(storageDevice);
            MovePlayerToM2StorageSlot();
            OperationResult storageMode = playerCarry.TrySetM2StorageSeatMode(true);
            M2StorageSlotEvaluation initialEvaluation = storageSlot.LastEvaluation;
            bool insertionAngle = storageMode.IsSuccess &&
                                  initialEvaluation.CanSeat &&
                                  Mathf.Abs(
                                      Quaternion.Angle(
                                          initialEvaluation.GuidedPose.rotation,
                                          initialEvaluation.SeatedPose.rotation) -
                                      M2StorageSlotSolver.GuidedInsertionAngleDegrees) <
                                      0.001f &&
                                  initialEvaluation.GuidedPose.position !=
                                      initialEvaluation.SeatedPose.position;
            long keyedAssemblyRevision = session.AssemblyBuild.Revision;
            long keyedInventoryRevision = session.Inventory.Revision;
            int keyedReceiptCount = session.AssemblyBuild.ReceiptCount;
            OperationResult rotateWrong =
                playerCarry.TryRotateM2StorageSeatPreviewClockwise();
            OperationResult wrongConfirm = playerCarry.TryConfirmM2StorageSeat();
            bool keyedOrientation = rotateWrong.IsSuccess &&
                                    wrongConfirm.IsFailure &&
                                    wrongConfirm.Error ==
                                        AssemblyFailures.M2OrientationMismatch &&
                                    playerCarry.CurrentM2StorageSlotStatus ==
                                        M2StorageSlotStatus.OrientationInvalid &&
                                    session.AssemblyBuild.Revision == keyedAssemblyRevision &&
                                    session.Inventory.Revision == keyedInventoryRevision &&
                                    session.AssemblyBuild.ReceiptCount == keyedReceiptCount;
            playerCarry.TryRotateM2StorageSeatPreviewClockwise();
            OperationResult storageSeat = playerCarry.TryConfirmM2StorageSeat();
            AssemblyOperationReceipt seatReceipt =
                session.AssemblyBuild.GetReceipts()[
                    session.AssemblyBuild.ReceiptCount - 1];
            bool seated = storagePickup.IsSuccess &&
                          keyedOrientation &&
                          insertionAngle &&
                          storageSeat.IsSuccess &&
                          seatReceipt.OperationKind ==
                              AssemblyOperationKind.SeatStorageDevice &&
                          seatReceipt.M2KeyOrientation == M2KeyOrientation.KeyAligned &&
                          session.AssemblyBuild.StorageSlotState ==
                              StorageSlotState.StorageDeviceSeatedUnsecured &&
                          ApproximatelySamePose(
                              new Pose(
                                  storageDevice.transform.position,
                                  storageDevice.transform.rotation),
                              storageSlot.SeatedPose) &&
                          storageBinding.ValidateProjectionInvariant().IsSuccess;
            if (!seated)
            {
                LogM2StorageSmokeFailure("smoke.storage-seat-failed");
                yield break;
            }

            long duplicateAssemblyRevision = session.AssemblyBuild.Revision;
            long duplicateInventoryRevision = session.Inventory.Revision;
            int duplicateReceiptCount = session.AssemblyBuild.ReceiptCount;
            OperationResult duplicateSeat = storageBinding.TryAttachAt(
                storageSlot.SeatedPose,
                M2KeyOrientation.KeyAligned);
            bool duplicateSeatBlocked = duplicateSeat.IsFailure &&
                                        duplicateSeat.Error.Code ==
                                            "assembly-storage.attach-authority-mismatch" &&
                                        session.AssemblyBuild.Revision ==
                                            duplicateAssemblyRevision &&
                                        session.Inventory.Revision ==
                                            duplicateInventoryRevision &&
                                        session.AssemblyBuild.ReceiptCount ==
                                            duplicateReceiptCount;

            MovePlayerToM2StorageSlot();
            long secureInventoryRevision = session.Inventory.Revision;
            OperationResult secure = playerCarry.TryOperateM2StorageCaptiveScrew();
            AssemblyOperationReceipt secureReceipt =
                session.AssemblyBuild.GetReceipts()[
                    session.AssemblyBuild.ReceiptCount - 1];
            bool captiveScrew = secure.IsSuccess &&
                                secureReceipt.OperationKind ==
                                    AssemblyOperationKind.SecureStorageDevice &&
                                session.AssemblyBuild.StorageSlotState ==
                                    StorageSlotState.StorageDeviceSecured &&
                                session.Inventory.Revision == secureInventoryRevision &&
                                storageBinding.ValidateProjectionInvariant().IsSuccess;
            long securedAssemblyRevision = session.AssemblyBuild.Revision;
            long securedInventoryRevision = session.Inventory.Revision;
            int securedReceiptCount = session.AssemblyBuild.ReceiptCount;
            OperationResult securedRemove = playerCarry.TryPickup(storageDevice);
            bool securedRemoveGate = securedRemove.IsFailure &&
                                     securedRemove.Error ==
                                         AssemblyFailures.StorageDeviceSecured &&
                                     session.AssemblyBuild.Revision ==
                                         securedAssemblyRevision &&
                                     session.Inventory.Revision ==
                                         securedInventoryRevision &&
                                     session.AssemblyBuild.ReceiptCount ==
                                         securedReceiptCount;

            MovePlayerToMotherboardFastener();
            OperationResult motherboardUnsecure =
                playerCarry.TryOperateMotherboardFastener();
            long hostGateAssemblyRevision = session.AssemblyBuild.Revision;
            long hostGateInventoryRevision = session.Inventory.Revision;
            int hostGateReceiptCount = session.AssemblyBuild.ReceiptCount;
            OperationResult hostDetach = playerCarry.TryPickup(
                motherboardBinding.PhysicalItem);
            bool hostDetachGate = motherboardUnsecure.IsSuccess &&
                                  hostDetach.IsFailure &&
                                  hostDetach.Error ==
                                      AssemblyFailures.StorageDeviceInstalled &&
                                  session.AssemblyBuild.Revision ==
                                      hostGateAssemblyRevision &&
                                  session.Inventory.Revision ==
                                      hostGateInventoryRevision &&
                                  session.AssemblyBuild.ReceiptCount ==
                                      hostGateReceiptCount;

            MovePlayerToM2StorageSlot();
            long unsecureInventoryRevision = session.Inventory.Revision;
            OperationResult unsecure = playerCarry.TryOperateM2StorageCaptiveScrew();
            AssemblyOperationReceipt unsecureReceipt =
                session.AssemblyBuild.GetReceipts()[
                    session.AssemblyBuild.ReceiptCount - 1];
            bool unsecureIsolated = unsecure.IsSuccess &&
                                    unsecureReceipt.OperationKind ==
                                        AssemblyOperationKind.UnsecureStorageDevice &&
                                    session.AssemblyBuild.StorageSlotState ==
                                        StorageSlotState.StorageDeviceSeatedUnsecured &&
                                    session.Inventory.Revision ==
                                        unsecureInventoryRevision;
            OperationResult remove = playerCarry.TryPickup(storageDevice);
            AssemblyOperationReceipt removeReceipt =
                session.AssemblyBuild.GetReceipts()[
                    session.AssemblyBuild.ReceiptCount - 1];
            bool removed = remove.IsSuccess &&
                           removeReceipt.OperationKind ==
                               AssemblyOperationKind.RemoveStorageDevice &&
                           playerCarry.HeldItem == storageDevice &&
                           storageBinding.IsAuthorityInHands &&
                           session.AssemblyBuild.StorageSlotState ==
                               StorageSlotState.EmptyOpen;
            OperationResult recovery = playerCarry.TryRecoverHeldItem();

            long finalAssemblyRevision = session.AssemblyBuild.Revision;
            int finalReceiptCount = session.AssemblyBuild.ReceiptCount;
            long finalInventoryRevision = session.Inventory.Revision;
            OperationResult<AssemblyOperationReceipt> delayedSeatReplay =
                session.SeatStorageDevice(
                    seatReceipt.OperationId,
                    seatReceipt.M2KeyOrientation,
                    seatReceipt.SourceAttachOperationId,
                    seatReceipt.SourceSecureOperationId,
                    seatReceipt.ExpectedAssemblyRevision);
            OperationResult<AssemblyOperationReceipt> delayedSecureReplay =
                session.SecureStorageDevice(
                    secureReceipt.OperationId,
                    secureReceipt.SourceStorageSeatOperationId,
                    secureReceipt.ExpectedAssemblyRevision);
            OperationResult<AssemblyOperationReceipt> delayedUnsecureReplay =
                session.UnsecureStorageDevice(
                    unsecureReceipt.OperationId,
                    unsecureReceipt.SourceStorageSeatOperationId,
                    unsecureReceipt.SourceStorageRetentionOperationId,
                    unsecureReceipt.ExpectedAssemblyRevision);
            OperationResult<AssemblyOperationReceipt> delayedRemoveReplay =
                session.RemoveStorageDevice(
                    removeReceipt.OperationId,
                    removeReceipt.SourceStorageSeatOperationId,
                    removeReceipt.ExpectedAssemblyRevision);
            bool replayStable = delayedSeatReplay.IsSuccess &&
                                ReferenceEquals(delayedSeatReplay.Value, seatReceipt) &&
                                delayedSecureReplay.IsSuccess &&
                                ReferenceEquals(
                                    delayedSecureReplay.Value,
                                    secureReceipt) &&
                                delayedUnsecureReplay.IsSuccess &&
                                ReferenceEquals(
                                    delayedUnsecureReplay.Value,
                                    unsecureReceipt) &&
                                delayedRemoveReplay.IsSuccess &&
                                ReferenceEquals(delayedRemoveReplay.Value, removeReceipt) &&
                                session.AssemblyBuild.Revision == finalAssemblyRevision &&
                                session.AssemblyBuild.ReceiptCount == finalReceiptCount &&
                                session.Inventory.Revision == finalInventoryRevision;
            bool recovered = recovery.IsSuccess &&
                             playerCarry.HeldItem == null &&
                             session.AssemblyBuild.StorageSlotState ==
                                 StorageSlotState.EmptyOpen &&
                             session.TryGetStorageItem(
                                 out InventoryItemRecord recoveredStorage) &&
                             recoveredStorage.Id == session.StorageItemId &&
                             recoveredStorage.ProductId == session.StorageProductId &&
                             recoveredStorage.ContainerId ==
                                 session.WorldFloorContainerId &&
                             storageDevice.GetInstanceID() == initialInstanceId &&
                             storageDevice.transform.parent == initialParent &&
                             ApproximatelySamePose(
                                 new Pose(
                                     storageDevice.transform.position,
                                     storageDevice.transform.rotation),
                                 initialPose) &&
                             storageDevice.Ownership == PhysicalItemOwnership.World &&
                             storageDevice.IsStablePlacement &&
                             CountCanonicalStorageProjections(
                                 session.StorageItemId.Value) == 1 &&
                             session.Inventory.SerializedItemCount == 7 &&
                             session.Inventory.GetContainerQuantity(
                                 session.HandsContainerId).Value == 0 &&
                             session.Inventory.GetContainerQuantity(
                                 session.StorageSlotContainerId).Value == 0 &&
                             storageBinding.ValidateProjectionInvariant().IsSuccess &&
                             session.ValidateInvariants().IsSuccess;
            bool authorityIsolated = session.Orders.Revision == orderRevision &&
                                     session.RetailOffers.Revision == offerRevision &&
                                     session.RetailBaskets.Revision == basketRevision &&
                                     session.RetailCheckouts.Revision == checkoutRevision &&
                                     session.CheckoutSettlements.Revision ==
                                         settlementRevision &&
                                     session.CustomerVisits.Revision == visitRevision &&
                                     session.CustomerConsultations.Revision ==
                                         consultationRevision &&
                                     session.CustomerOfferActions.Revision ==
                                         actionRevision &&
                                     session.AssemblyBuild.ProcessorSocketState ==
                                         ProcessorSocketState.EmptyOpen &&
                                     session.AssemblyBuild.MemorySlotState ==
                                         MemorySlotState.EmptyOpen;
            if (!slotInterface ||
                !insertionAngle ||
                !duplicateSeatBlocked ||
                !captiveScrew ||
                !securedRemoveGate ||
                !hostDetachGate ||
                !unsecureIsolated ||
                !removed ||
                !replayStable ||
                !recovered ||
                !authorityIsolated)
            {
                LogM2StorageSmokeFailure("smoke.final-contract-mismatch");
                yield break;
            }

            Debug.Log(
                "GARAGE_STORAGE_RUNTIME_SMOKE storage-flow=ok preflight=ok " +
                "slot-interface=ok keyed-orientation=ok insertion-angle=ok " +
                "captive-screw=ok duplicate-seat-blocked=ok " +
                "secured-remove-gate=ok host-detach-gate=ok replay=ok " +
                "authority-isolated=ok identity=stable recovery=ok");
            yield return new WaitForEndOfFrame();
        }

        private IEnumerator RunProcessorCoolerSmoke()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return null;

            GarageStockFlowSession session = stockFlow != null
                ? stockFlow.EnsureInitialized()
                : null;
            ProcessorCoolerRuntimeSmokeMarker smokeMarker = processorCooler != null
                ? processorCooler.GetComponent<ProcessorCoolerRuntimeSmokeMarker>()
                : null;
            if (playerMotor == null ||
                playerCarry == null ||
                session == null ||
                motherboardBinding == null ||
                motherboardSeat == null ||
                motherboardFastener == null ||
                processorSocket == null ||
                processorBinding == null ||
                processor == null ||
                processorCoolerSlot == null ||
                processorCoolerBinding == null ||
                processorCooler == null ||
                processorCoolerGeometry == null ||
                smokeMarker == null)
            {
                LogProcessorCoolerSmokeFailure("smoke.context-missing");
                yield break;
            }

            Pose initialCoolerPose = new Pose(
                processorCooler.transform.position,
                processorCooler.transform.rotation);
            Transform initialCoolerParent = processorCooler.transform.parent;
            int initialCoolerInstanceId = processorCooler.GetInstanceID();
            ProcessorCoolerRetentionTopology topology =
                session.AssemblyBuild.ProcessorCoolerRetentionTopology;
            bool slotInterface = processorCoolerSlot.IsConfigured &&
                                 processorCoolerSlot.SlotIdValue ==
                                     session.ProcessorCoolerSlotId.Value &&
                                 processorCoolerSlot.BracketIdValue ==
                                     session.ProcessorCoolerBracketId.Value &&
                                 processorCoolerBinding.Slot == processorCoolerSlot &&
                                 processorCoolerBinding.PhysicalItem ==
                                     processorCooler &&
                                 processorCoolerGeometry.IsCanonical &&
                                 smokeMarker.IsReady &&
                                 HasProcessorCoolerR27Runtime &&
                                 topology != null &&
                                 topology.IsValid &&
                                 topology.PhysicalOrder.Count == 4 &&
                                 topology.PhysicalOrder[0].Value ==
                                     GarageStockFlowSession
                                         .ProcessorCoolerRetentionPoint1IdValue &&
                                 topology.PhysicalOrder[1].Value ==
                                     GarageStockFlowSession
                                         .ProcessorCoolerRetentionPoint2IdValue &&
                                 topology.PhysicalOrder[2].Value ==
                                     GarageStockFlowSession
                                         .ProcessorCoolerRetentionPoint3IdValue &&
                                 topology.PhysicalOrder[3].Value ==
                                     GarageStockFlowSession
                                         .ProcessorCoolerRetentionPoint4IdValue;
            bool preflight = slotInterface &&
                             session.AssemblyBuild.HasProcessorCoolerSlot &&
                             session.AssemblyBuild.MotherboardSeatState ==
                                 AssemblySeatState.Empty &&
                             session.AssemblyBuild.ProcessorSocketState ==
                                 ProcessorSocketState.EmptyOpen &&
                             session.AssemblyBuild.ProcessorCoolerSlotState ==
                                 ProcessorCoolerSlotState.EmptyOpen &&
                             session.TryGetProcessorCoolerItem(
                                 out InventoryItemRecord looseCooler) &&
                             looseCooler.Id == session.ProcessorCoolerItemId &&
                             looseCooler.ProductId ==
                                 session.ProcessorCoolerProductId &&
                             looseCooler.ContainerId ==
                                 session.WorldFloorContainerId &&
                             looseCooler.StateFlags ==
                                 InventorySerializedItemStateFlags.None &&
                             CountCanonicalProcessorCoolerProjections(
                                 session.ProcessorCoolerItemId.Value) == 1 &&
                             session.Inventory.SerializedItemCount == 7 &&
                             processorCoolerBinding.ValidateProjectionInvariant()
                                 .IsSuccess &&
                             session.ValidateInvariants().IsSuccess;
            if (!preflight)
            {
                LogProcessorCoolerSmokeFailure("smoke.preflight-mismatch");
                yield break;
            }

            long orderRevision = session.Orders.Revision;
            long offerRevision = session.RetailOffers.Revision;
            long basketRevision = session.RetailBaskets.Revision;
            long checkoutRevision = session.RetailCheckouts.Revision;
            long settlementRevision = session.CheckoutSettlements.Revision;
            long visitRevision = session.CustomerVisits.Revision;
            long consultationRevision = session.CustomerConsultations.Revision;
            long actionRevision = session.CustomerOfferActions.Revision;

            OperationResult motherboardPickup = playerCarry.TryPickup(
                motherboardBinding.PhysicalItem);
            MovePlayerToMotherboardSeat();
            OperationResult motherboardMode =
                playerCarry.TrySetMotherboardSeatMode(true);
            OperationResult motherboardAttach =
                playerCarry.TryConfirmMotherboardSeat();
            MovePlayerToMotherboardFastener();
            OperationResult motherboardSecure =
                playerCarry.TryOperateMotherboardFastener();
            OperationResult processorPickup = playerCarry.TryPickup(processor);
            MovePlayerToProcessorSocket();
            OperationResult processorMode = playerCarry.TrySetProcessorSeatMode(true);
            OperationResult processorSeat = playerCarry.TryConfirmProcessorSeat();
            MovePlayerToProcessorSocket();
            OperationResult processorRetain =
                playerCarry.TryOperateProcessorRetention();
            if (motherboardPickup.IsFailure ||
                motherboardMode.IsFailure ||
                motherboardAttach.IsFailure ||
                motherboardSecure.IsFailure ||
                processorPickup.IsFailure ||
                processorMode.IsFailure ||
                processorSeat.IsFailure ||
                processorRetain.IsFailure ||
                session.AssemblyBuild.MotherboardSeatState !=
                    AssemblySeatState.SeatedSecured ||
                session.AssemblyBuild.ProcessorSocketState !=
                    ProcessorSocketState.ProcessorRetained)
            {
                LogProcessorCoolerSmokeFailure("smoke.host-preflight-failed");
                yield break;
            }

            OperationResult coolerPickup = playerCarry.TryPickup(processorCooler);
            MovePlayerToProcessorCoolerSlot();
            long seatAssemblyRevision = session.AssemblyBuild.Revision;
            long seatInventoryRevision = session.Inventory.Revision;
            int seatReceiptCount = session.AssemblyBuild.ReceiptCount;
            OperationResult coolerMode =
                playerCarry.TrySetProcessorCoolerSeatMode(true);
            ProcessorCoolerSlotEvaluation primaryEvaluation =
                processorCoolerSlot.LastEvaluation;
            bool primaryHalfTurn = coolerMode.IsSuccess &&
                                   primaryEvaluation.CanSeat &&
                                   primaryEvaluation.Orientation ==
                                       ProcessorCoolerMountOrientation.Primary;
            OperationResult rotate =
                playerCarry.TryRotateProcessorCoolerSeatPreview();
            ProcessorCoolerSlotEvaluation rotatedEvaluation =
                processorCoolerSlot.LastEvaluation;
            bool rotatedHalfTurn = rotate.IsSuccess &&
                                   rotatedEvaluation.CanSeat &&
                                   rotatedEvaluation.Orientation ==
                                       ProcessorCoolerMountOrientation.Rotated180 &&
                                   Quaternion.Angle(
                                       primaryEvaluation.Pose.rotation,
                                       rotatedEvaluation.Pose.rotation) > 179.9f;
            OperationResult coolerSeat =
                playerCarry.TryConfirmProcessorCoolerSeat();
            var receiptsAfterCoolerSeat = session.AssemblyBuild.GetReceipts();
            AssemblyOperationReceipt coolerSeatReceipt =
                receiptsAfterCoolerSeat[receiptsAfterCoolerSeat.Count - 1];
            bool timApplied = coolerPickup.IsSuccess &&
                              primaryHalfTurn &&
                              rotatedHalfTurn &&
                              coolerSeat.IsSuccess &&
                              coolerSeatReceipt.OperationKind ==
                                  AssemblyOperationKind.SeatProcessorCooler &&
                              coolerSeatReceipt.ProcessorCoolerMountOrientation ==
                                  ProcessorCoolerMountOrientation.Rotated180 &&
                              coolerSeatReceipt.PreviousProcessorCoolerTimState ==
                                  ProcessorCoolerTimState.PreAppliedUnused &&
                              coolerSeatReceipt.ResultingProcessorCoolerTimState ==
                                  ProcessorCoolerTimState.AppliedConsumed &&
                              session.AssemblyBuild.ProcessorCoolerSlotState ==
                                  ProcessorCoolerSlotState.CoolerSeatedUnsecured &&
                              session.AssemblyBuild.ProcessorCoolerTimState ==
                                  ProcessorCoolerTimState.AppliedConsumed &&
                              session.AssemblyBuild.Revision ==
                                  seatAssemblyRevision + 1 &&
                              session.AssemblyBuild.ReceiptCount ==
                                  seatReceiptCount + 1 &&
                              session.Inventory.Revision ==
                                  seatInventoryRevision + 1 &&
                              session.TryGetProcessorCoolerItem(
                                  out InventoryItemRecord seatedCooler) &&
                              seatedCooler.ContainerId ==
                                  session.ProcessorCoolerSlotContainerId &&
                              seatedCooler.StateFlags ==
                                  InventorySerializedItemStateFlags
                                      .PreAppliedConsumableConsumed &&
                              processorCoolerBinding.ValidateProjectionInvariant()
                                  .IsSuccess;
            if (!timApplied)
            {
                LogProcessorCoolerSmokeFailure("smoke.cooler-seat-failed");
                yield break;
            }

            long duplicateAssemblyRevision = session.AssemblyBuild.Revision;
            long duplicateInventoryRevision = session.Inventory.Revision;
            int duplicateReceiptCount = session.AssemblyBuild.ReceiptCount;
            OperationResult duplicateSeat = processorCoolerBinding.TryAttachAt(
                processorCoolerSlot.SnapPose,
                ProcessorCoolerMountOrientation.Rotated180,
                null,
                0);
            bool duplicateSeatBlocked = duplicateSeat.IsFailure &&
                                        duplicateSeat.Error.Code ==
                                            "assembly-cooler.attach-authority-mismatch" &&
                                        session.AssemblyBuild.Revision ==
                                            duplicateAssemblyRevision &&
                                        session.Inventory.Revision ==
                                            duplicateInventoryRevision &&
                                        session.AssemblyBuild.ReceiptCount ==
                                            duplicateReceiptCount;

            MovePlayerToProcessorCoolerSlot();
            OperationResult retain =
                playerCarry.TryOperateProcessorCoolerRetention();
            var receiptsAfterRetain = session.AssemblyBuild.GetReceipts();
            AssemblyOperationReceipt retainReceipt =
                receiptsAfterRetain[receiptsAfterRetain.Count - 1];
            bool crossOrder = retain.IsSuccess &&
                              retainReceipt.OperationKind ==
                                  AssemblyOperationKind.RetainProcessorCooler &&
                              retainReceipt.SourceProcessorCoolerSeatOperationId ==
                                  coolerSeatReceipt.OperationId &&
                              session.AssemblyBuild.ProcessorCoolerSlotState ==
                                  ProcessorCoolerSlotState.CoolerRetained &&
                              topology.CrossRetentionOrder.Count == 4 &&
                              topology.CrossRetentionOrder[0] == topology.Point1Id &&
                              topology.CrossRetentionOrder[1] == topology.Point3Id &&
                              topology.CrossRetentionOrder[2] == topology.Point2Id &&
                              topology.CrossRetentionOrder[3] == topology.Point4Id &&
                              topology.ReverseCrossRetentionOrder.Count == 4 &&
                              topology.ReverseCrossRetentionOrder[0] ==
                                  topology.Point4Id &&
                              topology.ReverseCrossRetentionOrder[1] ==
                                  topology.Point2Id &&
                              topology.ReverseCrossRetentionOrder[2] ==
                                  topology.Point3Id &&
                              topology.ReverseCrossRetentionOrder[3] ==
                                  topology.Point1Id &&
                              processorCoolerBinding.ValidateProjectionInvariant()
                                  .IsSuccess;

            long retainedGateAssemblyRevision = session.AssemblyBuild.Revision;
            long retainedGateInventoryRevision = session.Inventory.Revision;
            int retainedGateReceiptCount = session.AssemblyBuild.ReceiptCount;
            Pose retainedPose = new Pose(
                processorCooler.transform.position,
                processorCooler.transform.rotation);
            OperationResult retainedRemoval =
                playerCarry.TryPickup(processorCooler);
            bool retainedRemoveGate = retainedRemoval.IsFailure &&
                                      retainedRemoval.Error ==
                                          AssemblyFailures.ProcessorCoolerRetained &&
                                      playerCarry.HeldItem == null &&
                                      session.AssemblyBuild.Revision ==
                                          retainedGateAssemblyRevision &&
                                      session.Inventory.Revision ==
                                          retainedGateInventoryRevision &&
                                      session.AssemblyBuild.ReceiptCount ==
                                          retainedGateReceiptCount &&
                                      ApproximatelySamePose(
                                          new Pose(
                                              processorCooler.transform.position,
                                              processorCooler.transform.rotation),
                                          retainedPose);

            AssemblyBuildSnapshot retainedSnapshot =
                session.AssemblyBuild.GetSnapshot();
            OperationResult<AssemblyOperationReceipt> hostOpen =
                session.OpenProcessorRetention(
                    StableId<AssemblyOperationIdScope>.Parse(
                        "assembly.operation.smoke.cooler-host-open"),
                    retainedSnapshot.ProcessorSeatedByOperationId,
                    retainedSnapshot.ProcessorRetainedByOperationId,
                    retainedSnapshot.Revision);
            bool hostGates = hostOpen.IsFailure &&
                             hostOpen.Error ==
                                 AssemblyFailures.ProcessorCoolerInstalled &&
                             session.AssemblyBuild.Revision ==
                                 retainedSnapshot.Revision &&
                             session.AssemblyBuild.ReceiptCount ==
                                 retainedGateReceiptCount &&
                             session.Inventory.Revision ==
                                 retainedGateInventoryRevision &&
                             session.AssemblyBuild.ProcessorSocketState ==
                                 ProcessorSocketState.ProcessorRetained;
            if (!duplicateSeatBlocked ||
                !crossOrder ||
                !retainedRemoveGate ||
                !hostGates)
            {
                LogProcessorCoolerSmokeFailure("smoke.retention-contract-failed");
                yield break;
            }

            MovePlayerToProcessorCoolerSlot();
            OperationResult unretain =
                playerCarry.TryOperateProcessorCoolerRetention();
            var receiptsAfterUnretain = session.AssemblyBuild.GetReceipts();
            AssemblyOperationReceipt unretainReceipt =
                receiptsAfterUnretain[receiptsAfterUnretain.Count - 1];
            OperationResult remove = playerCarry.TryPickup(processorCooler);
            var receiptsAfterRemove = session.AssemblyBuild.GetReceipts();
            AssemblyOperationReceipt removeReceipt =
                receiptsAfterRemove[receiptsAfterRemove.Count - 1];
            bool removed = unretain.IsSuccess &&
                           unretainReceipt.OperationKind ==
                               AssemblyOperationKind.UnretainProcessorCooler &&
                           unretainReceipt.SourceProcessorCoolerSeatOperationId ==
                               coolerSeatReceipt.OperationId &&
                           unretainReceipt.SourceProcessorCoolerRetentionOperationId ==
                               retainReceipt.OperationId &&
                           remove.IsSuccess &&
                           removeReceipt.OperationKind ==
                               AssemblyOperationKind.RemoveProcessorCooler &&
                           playerCarry.HeldItem == processorCooler &&
                           processorCoolerBinding.IsAuthorityInHands &&
                           session.AssemblyBuild.ProcessorCoolerSlotState ==
                               ProcessorCoolerSlotState.EmptyOpen;
            if (!removed)
            {
                LogProcessorCoolerSmokeFailure("smoke.cooler-remove-failed");
                yield break;
            }

            OperationResult firstRecovery = playerCarry.TryRecoverHeldItem();
            OperationResult consumedPickup = playerCarry.TryPickup(processorCooler);
            MovePlayerToProcessorCoolerSlot();
            OperationResult consumedMode =
                playerCarry.TrySetProcessorCoolerSeatMode(true);
            long consumedAssemblyRevision = session.AssemblyBuild.Revision;
            long consumedInventoryRevision = session.Inventory.Revision;
            int consumedReceiptCount = session.AssemblyBuild.ReceiptCount;
            OperationResult consumedSeat =
                playerCarry.TryConfirmProcessorCoolerSeat();
            bool consumedBlocked = firstRecovery.IsSuccess &&
                                   consumedPickup.IsSuccess &&
                                   consumedMode.IsSuccess &&
                                   consumedSeat.IsFailure &&
                                   consumedSeat.Error ==
                                       AssemblyFailures.ProcessorCoolerTimConsumed &&
                                   playerCarry.HeldItem == processorCooler &&
                                   session.AssemblyBuild.Revision ==
                                       consumedAssemblyRevision &&
                                   session.Inventory.Revision ==
                                       consumedInventoryRevision &&
                                   session.AssemblyBuild.ReceiptCount ==
                                       consumedReceiptCount &&
                                   session.AssemblyBuild.ProcessorCoolerSlotState ==
                                       ProcessorCoolerSlotState.EmptyOpen;
            OperationResult finalRecovery = playerCarry.TryRecoverHeldItem();

            long finalAssemblyRevision = session.AssemblyBuild.Revision;
            int finalReceiptCount = session.AssemblyBuild.ReceiptCount;
            long finalInventoryRevision = session.Inventory.Revision;
            OperationResult<AssemblyOperationReceipt> delayedSeatReplay =
                session.SeatProcessorCooler(
                    coolerSeatReceipt.OperationId,
                    coolerSeatReceipt.ProcessorCoolerMountOrientation,
                    coolerSeatReceipt.SourceAttachOperationId,
                    coolerSeatReceipt.SourceSecureOperationId,
                    coolerSeatReceipt.SourceProcessorSeatOperationId,
                    coolerSeatReceipt.SourceProcessorRetentionOperationId,
                    coolerSeatReceipt.ExpectedAssemblyRevision);
            OperationResult<AssemblyOperationReceipt> delayedRetainReplay =
                session.RetainProcessorCooler(
                    retainReceipt.OperationId,
                    retainReceipt.SourceProcessorCoolerSeatOperationId,
                    retainReceipt.ExpectedAssemblyRevision);
            OperationResult<AssemblyOperationReceipt> delayedUnretainReplay =
                session.UnretainProcessorCooler(
                    unretainReceipt.OperationId,
                    unretainReceipt.SourceProcessorCoolerSeatOperationId,
                    unretainReceipt.SourceProcessorCoolerRetentionOperationId,
                    unretainReceipt.ExpectedAssemblyRevision);
            OperationResult<AssemblyOperationReceipt> delayedRemoveReplay =
                session.RemoveProcessorCooler(
                    removeReceipt.OperationId,
                    removeReceipt.SourceProcessorCoolerSeatOperationId,
                    removeReceipt.ExpectedAssemblyRevision);
            bool replayStable = delayedSeatReplay.IsSuccess &&
                                ReferenceEquals(
                                    delayedSeatReplay.Value,
                                    coolerSeatReceipt) &&
                                delayedRetainReplay.IsSuccess &&
                                ReferenceEquals(
                                    delayedRetainReplay.Value,
                                    retainReceipt) &&
                                delayedUnretainReplay.IsSuccess &&
                                ReferenceEquals(
                                    delayedUnretainReplay.Value,
                                    unretainReceipt) &&
                                delayedRemoveReplay.IsSuccess &&
                                ReferenceEquals(
                                    delayedRemoveReplay.Value,
                                    removeReceipt) &&
                                session.AssemblyBuild.Revision ==
                                    finalAssemblyRevision &&
                                session.AssemblyBuild.ReceiptCount ==
                                    finalReceiptCount &&
                                session.Inventory.Revision ==
                                    finalInventoryRevision;
            bool recovered = consumedBlocked &&
                             finalRecovery.IsSuccess &&
                             playerCarry.HeldItem == null &&
                             session.TryGetProcessorCoolerItem(
                                 out InventoryItemRecord recoveredCooler) &&
                             recoveredCooler.Id == session.ProcessorCoolerItemId &&
                             recoveredCooler.ProductId ==
                                 session.ProcessorCoolerProductId &&
                             recoveredCooler.ContainerId ==
                                 session.WorldFloorContainerId &&
                             recoveredCooler.StateFlags ==
                                 InventorySerializedItemStateFlags
                                     .PreAppliedConsumableConsumed &&
                             processorCooler.GetInstanceID() ==
                                 initialCoolerInstanceId &&
                             processorCooler.transform.parent ==
                                 initialCoolerParent &&
                             ApproximatelySamePose(
                                 new Pose(
                                     processorCooler.transform.position,
                                     processorCooler.transform.rotation),
                                 initialCoolerPose) &&
                             ApproximatelySamePose(
                                 new Pose(
                                     processorCooler.Body.position,
                                     processorCooler.Body.rotation),
                                 initialCoolerPose) &&
                             ApproximatelySamePose(
                                 new Pose(
                                     processorCooler.LastSafePosition,
                                     processorCooler.LastSafeRotation),
                                 initialCoolerPose) &&
                             processorCooler.Ownership ==
                                 PhysicalItemOwnership.World &&
                             processorCooler.IsStablePlacement &&
                             CountCanonicalProcessorCoolerProjections(
                                 session.ProcessorCoolerItemId.Value) == 1 &&
                             session.Inventory.SerializedItemCount == 7 &&
                             session.Inventory.GetContainerQuantity(
                                 session.HandsContainerId).Value == 0 &&
                             session.Inventory.GetContainerQuantity(
                                 session.ProcessorCoolerSlotContainerId).Value == 0 &&
                             session.AssemblyBuild.MotherboardSeatState ==
                                 AssemblySeatState.SeatedSecured &&
                             session.AssemblyBuild.ProcessorSocketState ==
                                 ProcessorSocketState.ProcessorRetained &&
                             session.AssemblyBuild.ProcessorCoolerSlotState ==
                                 ProcessorCoolerSlotState.EmptyOpen &&
                             processorCoolerBinding.ValidateProjectionInvariant()
                                 .IsSuccess &&
                             session.ValidateInvariants().IsSuccess;
            bool authorityIsolated = session.Orders.Revision == orderRevision &&
                                     session.RetailOffers.Revision == offerRevision &&
                                     session.RetailBaskets.Revision == basketRevision &&
                                     session.RetailCheckouts.Revision ==
                                         checkoutRevision &&
                                     session.CheckoutSettlements.Revision ==
                                         settlementRevision &&
                                     session.CustomerVisits.Revision == visitRevision &&
                                     session.CustomerConsultations.Revision ==
                                         consultationRevision &&
                                     session.CustomerOfferActions.Revision ==
                                         actionRevision &&
                                     session.AssemblyBuild.MemorySlotState ==
                                         MemorySlotState.EmptyOpen &&
                                     session.AssemblyBuild.StorageSlotState ==
                                         StorageSlotState.EmptyOpen;
            if (!replayStable || !recovered || !authorityIsolated)
            {
                LogProcessorCoolerSmokeFailure(
                    !replayStable
                        ? "smoke.delayed-replay-failed"
                        : !recovered
                            ? "smoke.recovery-failed"
                            : "smoke.authority-isolation-failed");
                yield break;
            }

            Debug.Log(
                "GARAGE_COOLER_RUNTIME_SMOKE cooler-flow=ok preflight=ok " +
                "socket-interface=ok keyed-orientation=ok tim=pre-applied " +
                "cross-order=ok duplicate-seat-blocked=ok " +
                "retained-remove-gate=ok host-gates=ok replay=ok " +
                "authority-isolated=ok identity=stable recovery=ok");
            yield return new WaitForEndOfFrame();
        }

        private static void LogMotherboardAssemblySmokeFailure(string code)
        {
            Debug.LogError(
                $"GARAGE_MOTHERBOARD_ASSEMBLY_RUNTIME_SMOKE assembly-flow=failed code={code}");
        }

        private static void LogProcessorSocketSmokeFailure(string code)
        {
            Debug.LogError(
                $"GARAGE_CPU_SOCKET_RUNTIME_SMOKE cpu-socket-flow=failed code={code}");
        }

        private static void LogDimmSlotSmokeFailure(string code)
        {
            Debug.LogError(
                $"GARAGE_DIMM_RUNTIME_SMOKE dimm-flow=failed code={code}");
        }

        private static void LogM2StorageSmokeFailure(string code)
        {
            Debug.LogError(
                $"GARAGE_STORAGE_RUNTIME_SMOKE storage-flow=failed code={code}");
        }

        private static void LogProcessorCoolerSmokeFailure(string code)
        {
            Debug.LogError(
                $"GARAGE_COOLER_RUNTIME_SMOKE cooler-flow=failed code={code}");
        }

        private static int CountCanonicalMotherboardProjections(string canonicalItemId)
        {
            int count = 0;
            foreach (PhysicalItemProjection item in FindObjectsByType<PhysicalItemProjection>(
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

        private static int CountCanonicalProcessorProjections(string canonicalItemId)
        {
            int count = 0;
            foreach (PhysicalItemProjection item in FindObjectsByType<PhysicalItemProjection>(
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

        private static int CountCanonicalMemoryProjections(string canonicalItemId)
        {
            int count = 0;
            foreach (PhysicalItemProjection item in FindObjectsByType<PhysicalItemProjection>(
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

        private static int CountCanonicalStorageProjections(string canonicalItemId)
        {
            int count = 0;
            foreach (PhysicalItemProjection item in FindObjectsByType<PhysicalItemProjection>(
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

        private static int CountCanonicalProcessorCoolerProjections(
            string canonicalItemId)
        {
            int count = 0;
            foreach (PhysicalItemProjection item in FindObjectsByType<PhysicalItemProjection>(
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

        private IEnumerator RunTransportCartSmoke()
        {
            yield return null;
            yield return new WaitForFixedUpdate();

            PhysicalItemProjection largeBox = null;
            foreach (PhysicalItemProjection item in FindObjectsByType<PhysicalItemProjection>(
                         FindObjectsSortMode.None))
            {
                if (item.CarryProfile == PhysicalCarryProfile.LargeBox)
                {
                    largeBox = item;
                    break;
                }
            }

            if (playerMotor == null || playerCarry == null || transportCart == null || largeBox == null)
            {
                Debug.LogError("GARAGE_CART_RUNTIME_SMOKE cart-flow=failed code=smoke.context-missing");
                yield break;
            }

            playerMotor.SetPaused(false);
            string itemIdentity = largeBox.ItemIdValue;
            OperationResult pickup = playerCarry.TryPickup(largeBox);
            if (pickup.IsFailure)
            {
                Debug.LogError(
                    $"GARAGE_CART_RUNTIME_SMOKE cart-flow=failed code={pickup.Error.Code}");
                yield break;
            }

            OperationResult load = playerCarry.TryLoadHeldItem(transportCart);
            if (load.IsFailure)
            {
                Debug.LogError(
                    $"GARAGE_CART_RUNTIME_SMOKE cart-flow=failed code={load.Error.Code}");
                yield break;
            }

            MovePlayerToCartHandle(transportCart, 1.35f);
            OperationResult beginDrive = playerCarry.TryBeginCartDrive(transportCart);
            if (beginDrive.IsFailure)
            {
                Debug.LogError(
                    $"GARAGE_CART_RUNTIME_SMOKE cart-flow=failed code={beginDrive.Error.Code}");
                yield break;
            }

            MovePlayerBy(transportCart.transform.forward * 0.18f);
            int interactableLayer = LayerMask.NameToLayer("Interactable");
            int playerLayer = LayerMask.NameToLayer("Player");
            OperationResult motion = transportCart.TryFollowDriver(
                1 << 0,
                (1 << 0) | (1 << interactableLayer) | (1 << playerLayer));
            if (motion.IsFailure)
            {
                Debug.LogError(
                    $"GARAGE_CART_RUNTIME_SMOKE cart-flow=failed code={motion.Error.Code}");
                yield break;
            }

            OperationResult endDrive = playerCarry.TryEndCartDrive();
            if (endDrive.IsFailure)
            {
                Debug.LogError(
                    $"GARAGE_CART_RUNTIME_SMOKE cart-flow=failed code={endDrive.Error.Code}");
                yield break;
            }

            MovePlayerToCartHandle(transportCart, 1.45f);
            Transform cameraPivot = playerMotor.transform.Find("CameraPivot");
            if (cameraPivot != null)
            {
                cameraPivot.localRotation = Quaternion.Euler(18f, 0f, 0f);
            }

            Physics.SyncTransforms();
            Debug.Log(
                $"GARAGE_CART_RUNTIME_SMOKE cart-flow=ok item={itemIdentity} " +
                $"loaded={(transportCart.Cargo == largeBox ? "ok" : "missing")} " +
                $"stable={(largeBox.IsMountedOnTransportCart ? "ok" : "missing")}");

            yield return new WaitForEndOfFrame();
        }

        private void MovePlayerToCartHandle(TransportCartProjection cart, float distance)
        {
            Vector3 handle = cart.transform.TransformPoint(new Vector3(0f, 0f, -0.60f));
            Vector3 playerPosition = handle - (cart.transform.forward * distance);
            playerPosition.y = 0.05f;
            SetPlayerPose(playerPosition, Quaternion.Euler(0f, cart.transform.eulerAngles.y, 0f));
        }

        private void MovePlayerToCheckoutStation(float distance)
        {
            Collider targetCollider = checkoutStation.InteractionCollider;
            Vector3 target = targetCollider.bounds.center;
            Vector3 approach = -targetCollider.transform.forward;
            approach.y = 0f;
            approach.Normalize();
            Vector3 playerPosition = target + (approach * distance);
            playerPosition.y = 0.05f;
            Vector3 horizontalLook = target - playerPosition;
            horizontalLook.y = 0f;
            SetPlayerPose(
                playerPosition,
                Quaternion.LookRotation(horizontalLook.normalized, Vector3.up));
            Transform cameraPivot = playerMotor.transform.Find("CameraPivot");
            if (cameraPivot != null)
            {
                cameraPivot.rotation = Quaternion.LookRotation(
                    target - cameraPivot.position,
                    Vector3.up);
            }

            Physics.SyncTransforms();
        }

        private void MovePlayerToMotherboardSeat()
        {
            Vector3 target = motherboardSeat.FocusCollider.bounds.center;
            Vector3 playerPosition = new Vector3(-0.95f, 0.05f, 3.15f);
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

        private void MovePlayerToMotherboardFastener()
        {
            Vector3 target = motherboardFastener.FocusCollider.bounds.center;
            Vector3 playerPosition = new Vector3(-0.95f, 0.05f, 3.15f);
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

        private void MovePlayerToProcessorSocket()
        {
            Vector3 target = processorSocket.FocusCollider.bounds.center;
            Vector3 playerPosition = new Vector3(-0.95f, 0.05f, 3.15f);
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

        private void MovePlayerToDimmSlot()
        {
            Vector3 target = dimmSlot.FocusCollider.bounds.center;
            Vector3 playerPosition = new Vector3(-0.95f, 0.05f, 3.15f);
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

        private void MovePlayerToM2StorageSlot()
        {
            Vector3 target = storageSlot.FocusCollider.bounds.center;
            Vector3 playerPosition = new Vector3(-0.95f, 0.05f, 3.15f);
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

        private void MovePlayerToProcessorCoolerSlot()
        {
            Vector3 target = processorCoolerSlot.FocusCollider.bounds.center;
            Vector3 playerPosition = new Vector3(-0.95f, 0.05f, 3.15f);
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

        private void MovePlayerToPhysicalItem(
            PhysicalItemProjection item,
            Vector3 approachDirection,
            float distance)
        {
            Vector3 target = item.Body != null
                ? item.Body.worldCenterOfMass
                : item.transform.position;
            Vector3 approach = approachDirection.normalized;
            Vector3 playerPosition = target + (approach * distance);
            playerPosition.y = 0.05f;
            Vector3 horizontalLook = target - playerPosition;
            horizontalLook.y = 0f;
            SetPlayerPose(
                playerPosition,
                Quaternion.LookRotation(horizontalLook.normalized, Vector3.up));
            Transform cameraPivot = playerMotor.transform.Find("CameraPivot");
            if (cameraPivot != null)
            {
                cameraPivot.rotation = Quaternion.LookRotation(
                    target - cameraPivot.position,
                    Vector3.up);
            }

            Physics.SyncTransforms();
        }

        private void MovePlayerBy(Vector3 delta)
        {
            SetPlayerPose(playerMotor.transform.position + delta, playerMotor.transform.rotation);
        }

        private void SetPlayerPose(Vector3 position, Quaternion rotation)
        {
            CharacterController controller = playerMotor.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
            }

            playerMotor.transform.SetPositionAndRotation(position, rotation);
            if (controller != null)
            {
                controller.enabled = true;
            }

            Physics.SyncTransforms();
        }

        private static bool HasCommandLineArgument(string argument)
        {
            return Array.Exists(
                Environment.GetCommandLineArgs(),
                candidate => string.Equals(candidate, argument, StringComparison.Ordinal));
        }

        private static bool ApproximatelySamePose(Pose left, Pose right)
        {
            return Vector3.SqrMagnitude(left.position - right.position) <= 0.000001f &&
                   Quaternion.Angle(left.rotation, right.rotation) <= 0.1f;
        }

    }
}
