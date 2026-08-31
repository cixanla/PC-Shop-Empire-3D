using System;
using System.Collections.Generic;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.Retail;

namespace PCShopEmpire3D.Quality
{
    /// <summary>
    /// Joins an exact customer work order to immutable validation and safe-shutdown
    /// evidence. It owns only quality-release decisions; every upstream aggregate
    /// remains authoritative for its own state.
    /// </summary>
    public sealed class CustomPcQualityReleaseAuthority
    {
        private readonly CustomPcWorkOrderAuthority _workOrders;
        private readonly PcValidationAuthority _validation;
        private readonly Dictionary<
            StableId<CustomPcQualityReleaseOperationIdScope>,
            CustomPcQualityReleaseReceipt> _receipts =
                new Dictionary<
                    StableId<CustomPcQualityReleaseOperationIdScope>,
                    CustomPcQualityReleaseReceipt>();
        private readonly List<CustomPcQualityReleaseReceipt> _receiptsByRevision =
            new List<CustomPcQualityReleaseReceipt>();

        private CustomPcQualityReleaseAuthority(
            CustomPcWorkOrderAuthority workOrders,
            PcValidationAuthority validation)
        {
            _workOrders = workOrders;
            _validation = validation;
        }

        public CustomPcWorkOrderAuthority WorkOrders => _workOrders;

        public PcValidationAuthority Validation => _validation;

        public long Revision { get; private set; }

        public int ReceiptCount => _receipts.Count;

        public static OperationResult<CustomPcQualityReleaseAuthority> Create(
            CustomPcWorkOrderAuthority workOrders,
            PcValidationAuthority validation)
        {
            if (workOrders == null || workOrders.Inventory == null ||
                validation == null || validation.AssemblyBuild == null ||
                validation.PowerState == null ||
                !ReferenceEquals(
                    validation.PowerState.AssemblyBuild,
                    validation.AssemblyBuild))
            {
                return OperationResult<CustomPcQualityReleaseAuthority>.Fail(
                    CustomPcQualityReleaseFailures.ConfigurationMissing);
            }

            return OperationResult<CustomPcQualityReleaseAuthority>.Success(
                new CustomPcQualityReleaseAuthority(workOrders, validation));
        }

        public OperationResult<CustomPcQualityReleaseReceipt> TryReleaseForPackaging(
            StableId<CustomPcQualityReleaseOperationIdScope> operationId,
            CustomPcBuildOrderRecord workOrder,
            CustomPcWorkTicketRecord workTicket,
            PcValidationReceipt sourceValidationReceipt,
            PcPowerStateReceipt sourcePowerOffReceipt,
            long expectedRevision)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<CustomPcQualityReleaseReceipt>.Fail(
                    CustomPcQualityReleaseFailures.InvalidOperationId);
            }

            OperationResult history = ValidateReceiptHistory();
            if (history.IsFailure)
            {
                return OperationResult<CustomPcQualityReleaseReceipt>.Fail(
                    history.Error);
            }

            if (_receipts.TryGetValue(
                    operationId,
                    out CustomPcQualityReleaseReceipt replay))
            {
                return replay.MatchesCommand(
                        operationId,
                        workOrder,
                        workTicket,
                        sourceValidationReceipt,
                        sourcePowerOffReceipt,
                        expectedRevision)
                    ? OperationResult<CustomPcQualityReleaseReceipt>.Success(replay)
                    : OperationResult<CustomPcQualityReleaseReceipt>.Fail(
                        CustomPcQualityReleaseFailures.OperationConflict);
            }

            if (expectedRevision != Revision)
            {
                return OperationResult<CustomPcQualityReleaseReceipt>.Fail(
                    CustomPcQualityReleaseFailures.RevisionMismatch);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult<CustomPcQualityReleaseReceipt>.Fail(
                    CustomPcQualityReleaseFailures.RevisionOverflow);
            }

            Failure invalid = ValidateCurrentCommand(
                workOrder,
                workTicket,
                sourceValidationReceipt,
                sourcePowerOffReceipt);
            if (!invalid.IsNone)
            {
                return OperationResult<CustomPcQualityReleaseReceipt>.Fail(invalid);
            }

            long nextRevision = Revision + 1L;
            var receipt = new CustomPcQualityReleaseReceipt(
                this,
                operationId,
                workOrder,
                workTicket,
                sourceValidationReceipt,
                sourcePowerOffReceipt,
                sourceValidationReceipt.SourceElectricalReadiness,
                expectedRevision,
                nextRevision);
            _receipts.Add(operationId, receipt);
            _receiptsByRevision.Add(receipt);
            Revision = nextRevision;
            return OperationResult<CustomPcQualityReleaseReceipt>.Success(receipt);
        }

        public OperationResult<CustomPcQualityReleaseReceipt>
            EvaluateCurrentRelease()
        {
            OperationResult history = ValidateReceiptHistory();
            if (history.IsFailure)
            {
                return OperationResult<CustomPcQualityReleaseReceipt>.Fail(
                    history.Error);
            }

            if (_receiptsByRevision.Count == 0)
            {
                return OperationResult<CustomPcQualityReleaseReceipt>.Fail(
                    CustomPcQualityReleaseFailures.NotCurrent);
            }

            CustomPcQualityReleaseReceipt receipt =
                _receiptsByRevision[_receiptsByRevision.Count - 1];
            return ValidateCurrentContext(receipt).IsNone
                ? OperationResult<CustomPcQualityReleaseReceipt>.Success(receipt)
                : OperationResult<CustomPcQualityReleaseReceipt>.Fail(
                    CustomPcQualityReleaseFailures.NotCurrent);
        }

        public bool TryGetReceipt(
            StableId<CustomPcQualityReleaseOperationIdScope> operationId,
            out CustomPcQualityReleaseReceipt receipt)
        {
            return _receipts.TryGetValue(operationId, out receipt);
        }

        public OperationResult ValidateReceiptHistory()
        {
            if (ValidateUpstream().IsFailure ||
                Revision != _receipts.Count ||
                _receipts.Count != _receiptsByRevision.Count)
            {
                return OperationResult.Fail(
                    CustomPcQualityReleaseFailures.ReceiptHistoryInvalid);
            }

            for (int index = 0; index < _receiptsByRevision.Count; index++)
            {
                CustomPcQualityReleaseReceipt receipt =
                    _receiptsByRevision[index];
                long revision = index + 1L;
                if (receipt == null || !receipt.IsOwnedBy(this) ||
                    receipt.OperationId.IsEmpty ||
                    receipt.ExpectedRevision != revision - 1L ||
                    receipt.Revision != revision ||
                    receipt.Result !=
                        CustomPcQualityReleaseResult.ReadyForPackaging ||
                    !_receipts.TryGetValue(
                        receipt.OperationId,
                        out CustomPcQualityReleaseReceipt mapped) ||
                    !ReferenceEquals(mapped, receipt) ||
                    !ReferenceEquals(
                        receipt.SourceElectricalReadiness,
                        receipt.SourceValidationReceipt
                            ?.SourceElectricalReadiness) ||
                    !ValidateHistoricalLineage(receipt).IsNone)
                {
                    return OperationResult.Fail(
                        CustomPcQualityReleaseFailures.ReceiptHistoryInvalid);
                }
            }

            return OperationResult.Success();
        }

        private OperationResult ValidateUpstream()
        {
            if (_workOrders == null || _validation == null ||
                _workOrders.ValidateInvariants().IsFailure ||
                _validation.ValidateReceiptHistory().IsFailure ||
                _validation.PowerState.ValidateReceiptHistory().IsFailure ||
                _validation.AssemblyBuild.ValidateInvariants().IsFailure)
            {
                return OperationResult.Fail(
                    CustomPcQualityReleaseFailures.ReceiptHistoryInvalid);
            }

            return OperationResult.Success();
        }

        private Failure ValidateCurrentCommand(
            CustomPcBuildOrderRecord workOrder,
            CustomPcWorkTicketRecord workTicket,
            PcValidationReceipt validationReceipt,
            PcPowerStateReceipt powerOffReceipt)
        {
            var candidate = new QualityReleaseLineage(
                workOrder,
                workTicket,
                validationReceipt,
                powerOffReceipt);
            Failure historical = ValidateHistoricalLineage(candidate);
            return !historical.IsNone
                ? historical
                : ValidateCurrentContext(candidate);
        }

        private Failure ValidateHistoricalLineage(
            CustomPcQualityReleaseReceipt receipt)
        {
            return receipt == null
                ? CustomPcQualityReleaseFailures.ReceiptHistoryInvalid
                : ValidateHistoricalLineage(new QualityReleaseLineage(
                    receipt.WorkOrder,
                    receipt.WorkTicket,
                    receipt.SourceValidationReceipt,
                    receipt.SourcePowerOffReceipt));
        }

        private Failure ValidateHistoricalLineage(QualityReleaseLineage lineage)
        {
            if (lineage.WorkOrder == null ||
                !_workOrders.TryGetWorkOrder(
                    lineage.WorkOrder.Id,
                    out CustomPcBuildOrderRecord ownedOrder) ||
                !ReferenceEquals(ownedOrder, lineage.WorkOrder))
            {
                return CustomPcQualityReleaseFailures.InvalidWorkOrder;
            }

            if (lineage.WorkTicket == null ||
                !_workOrders.TryGetWorkTicket(
                    lineage.WorkTicket.Id,
                    out CustomPcWorkTicketRecord ownedTicket) ||
                !ReferenceEquals(ownedTicket, lineage.WorkTicket) ||
                !ReferenceEquals(
                    lineage.WorkTicket.BuildOrder,
                    lineage.WorkOrder) ||
                lineage.WorkTicket.Id != lineage.WorkOrder.WorkTicketId)
            {
                return CustomPcQualityReleaseFailures.InvalidWorkTicket;
            }

            PcValidationReceipt validationReceipt = lineage.ValidationReceipt;
            if (validationReceipt == null ||
                !_validation.TryGetReceipt(
                    validationReceipt.OperationId,
                    out PcValidationReceipt ownedValidation) ||
                !ReferenceEquals(ownedValidation, validationReceipt))
            {
                return CustomPcQualityReleaseFailures.ValidationReceiptInvalid;
            }

            if (validationReceipt.Result !=
                    PcValidationResult.PassedForQualityStage ||
                validationReceipt.StressResult != PcStressResult.Stable ||
                validationReceipt.SourceElectricalReadiness == null ||
                validationReceipt.SourcePowerBudget == null ||
                !ReferenceEquals(
                    validationReceipt.SourcePowerBudget.ElectricalReadiness,
                    validationReceipt.SourceElectricalReadiness))
            {
                return CustomPcQualityReleaseFailures.ValidationNotPassed;
            }

            PcPowerStateReceipt powerOffReceipt = lineage.PowerOffReceipt;
            if (powerOffReceipt == null ||
                !_validation.PowerState.TryGetReceipt(
                    powerOffReceipt.OperationId,
                    out PcPowerStateReceipt ownedPowerOff) ||
                !ReferenceEquals(ownedPowerOff, powerOffReceipt) ||
                powerOffReceipt.TransitionKind != PcPowerTransitionKind.PowerOff ||
                powerOffReceipt.ResultingState != PcPowerState.Off ||
                !ReferenceEquals(
                    powerOffReceipt.SourcePowerOnReceipt,
                    validationReceipt.SourcePowerOnReceipt) ||
                !ReferenceEquals(
                    powerOffReceipt.PreflightReceipt,
                    validationReceipt.PreflightReceipt))
            {
                return CustomPcQualityReleaseFailures.SafePowerOffMissing;
            }

            return MatchesWorkOrderLineage(
                    lineage.WorkOrder,
                    validationReceipt)
                ? Failure.None
                : CustomPcQualityReleaseFailures.WorkOrderLineageMismatch;
        }

        private Failure ValidateCurrentContext(
            CustomPcQualityReleaseReceipt receipt)
        {
            return receipt == null
                ? CustomPcQualityReleaseFailures.NotCurrent
                : ValidateCurrentContext(new QualityReleaseLineage(
                    receipt.WorkOrder,
                    receipt.WorkTicket,
                    receipt.SourceValidationReceipt,
                    receipt.SourcePowerOffReceipt));
        }

        private Failure ValidateCurrentContext(QualityReleaseLineage lineage)
        {
            PcPowerStateAuthority powerState = _validation.PowerState;
            if (lineage.PowerOffReceipt == null ||
                powerState.State != PcPowerState.Off ||
                powerState.IsEnergized ||
                powerState.Revision != lineage.PowerOffReceipt.Revision ||
                _validation.AssemblyBuild.IsElectricallyEnergized)
            {
                return CustomPcQualityReleaseFailures.SafePowerOffMissing;
            }

            OperationResult<ElectricalReadinessSnapshot> current =
                _validation.AssemblyBuild.EvaluateElectricalReadiness();
            return current.IsSuccess &&
                   MatchesElectricalReadiness(
                       current.Value,
                       lineage.ValidationReceipt?.SourceElectricalReadiness)
                ? Failure.None
                : CustomPcQualityReleaseFailures.AssemblyDrift;
        }

        private bool MatchesWorkOrderLineage(
            CustomPcBuildOrderRecord workOrder,
            PcValidationReceipt validationReceipt)
        {
            ElectricalReadinessSnapshot electrical =
                validationReceipt?.SourceElectricalReadiness;
            PcPowerBudgetSnapshot budget = validationReceipt?.SourcePowerBudget;
            if (workOrder?.Lines == null || electrical == null || budget == null ||
                workOrder.Lines.Count !=
                    CustomPcQuoteAuthority.GraphicsFirstGamingLineCount)
            {
                return false;
            }

            bool motherboard = false;
            bool processor = false;
            bool memory = false;
            bool storage = false;
            bool cooler = false;
            bool graphics = false;
            bool powerSupply = false;
            bool atx24 = false;
            bool eps12v = false;
            bool pcie = false;

            foreach (CustomPcBuildOrderLineSnapshot line in workOrder.Lines)
            {
                if (!HasOwnedSerializedReservation(workOrder, line))
                {
                    return false;
                }

                switch (line.ComponentKind)
                {
                    case PcComponentKind.Motherboard:
                        if (motherboard || !MatchesLine(
                                line,
                                electrical.MotherboardItemId,
                                budget.MotherboardProductId)) return false;
                        motherboard = true;
                        break;
                    case PcComponentKind.Processor:
                        if (processor || !MatchesLine(
                                line,
                                electrical.ProcessorItemId,
                                budget.ProcessorProductId)) return false;
                        processor = true;
                        break;
                    case PcComponentKind.MemoryModule:
                        if (memory || !MatchesLine(
                                line,
                                electrical.MemoryItemId,
                                budget.MemoryProductId)) return false;
                        memory = true;
                        break;
                    case PcComponentKind.StorageDevice:
                        if (storage || !MatchesLine(
                                line,
                                electrical.StorageItemId,
                                budget.StorageProductId)) return false;
                        storage = true;
                        break;
                    case PcComponentKind.ProcessorCooler:
                        if (cooler || !MatchesLine(
                                line,
                                electrical.ProcessorCoolerItemId,
                                budget.ProcessorCoolerProductId)) return false;
                        cooler = true;
                        break;
                    case PcComponentKind.GraphicsCard:
                        if (graphics || !MatchesLine(
                                line,
                                electrical.GraphicsCardItemId,
                                budget.GraphicsCardProductId)) return false;
                        graphics = true;
                        break;
                    case PcComponentKind.PowerSupply:
                        if (powerSupply || !MatchesLine(
                                line,
                                electrical.PowerSupplyItemId,
                                budget.PowerSupplyProductId)) return false;
                        powerSupply = true;
                        break;
                    case PcComponentKind.PowerCable:
                        if (line.PowerCableType ==
                            PowerCableType.ModularAtx24SplitPsuToMotherboard)
                        {
                            if (atx24 || !MatchesCableLine(
                                    line,
                                    electrical.Atx24PowerCableItemId)) return false;
                            atx24 = true;
                        }
                        else if (line.PowerCableType ==
                                 PowerCableType.ModularEps12v8PinPsuToMotherboard)
                        {
                            if (eps12v || !MatchesCableLine(
                                    line,
                                    electrical.Eps12vPowerCableItemId)) return false;
                            eps12v = true;
                        }
                        else if (line.PowerCableType ==
                                 PowerCableType.ModularPcie8PinPsuToGraphicsCard)
                        {
                            if (pcie || !MatchesCableLine(
                                    line,
                                    electrical.PcieGpuPowerCableItemId)) return false;
                            pcie = true;
                        }
                        else
                        {
                            return false;
                        }
                        break;
                    default:
                        return false;
                }
            }

            return motherboard && processor && memory && storage && cooler &&
                   graphics && powerSupply && atx24 && eps12v && pcie;
        }

        private bool HasOwnedSerializedReservation(
            CustomPcBuildOrderRecord workOrder,
            CustomPcBuildOrderLineSnapshot line)
        {
            return line != null && !line.LineId.IsEmpty &&
                   !line.ProductId.IsEmpty && !line.ItemId.IsEmpty &&
                   !line.ReservationId.IsEmpty &&
                   _workOrders.Inventory.TryGetSerializedItem(
                       line.ItemId,
                       out InventoryItemRecord item) &&
                   item.ProductId == line.ProductId &&
                   _workOrders.Inventory.TryGetReservation(
                       line.ReservationId,
                       out InventoryReservation reservation) &&
                   reservation.TargetKind ==
                       InventoryReservationTargetKind.SerializedItem &&
                   reservation.ItemId == line.ItemId &&
                   reservation.ClaimId == workOrder.InventoryClaimId &&
                   reservation.Quantity == 1;
        }

        private static bool MatchesLine(
            CustomPcBuildOrderLineSnapshot line,
            StableId<ItemInstanceIdScope> itemId,
            StableId<ProductDefinitionIdScope> productId)
        {
            return line.ItemId == itemId && line.ProductId == productId;
        }

        private static bool MatchesCableLine(
            CustomPcBuildOrderLineSnapshot line,
            StableId<ItemInstanceIdScope> itemId)
        {
            return line.ItemId == itemId;
        }

        private static bool MatchesElectricalReadiness(
            ElectricalReadinessSnapshot left,
            ElectricalReadinessSnapshot right)
        {
            return left != null && right != null &&
                   left.BuildId == right.BuildId &&
                   left.ChassisId == right.ChassisId &&
                   left.MotherboardItemId == right.MotherboardItemId &&
                   left.ProcessorItemId == right.ProcessorItemId &&
                   left.MemoryItemId == right.MemoryItemId &&
                   left.StorageItemId == right.StorageItemId &&
                   left.ProcessorCoolerItemId == right.ProcessorCoolerItemId &&
                   left.GraphicsCardItemId == right.GraphicsCardItemId &&
                   left.PowerSupplyItemId == right.PowerSupplyItemId &&
                   left.Atx24PowerCableItemId == right.Atx24PowerCableItemId &&
                   left.Eps12vPowerCableItemId == right.Eps12vPowerCableItemId &&
                   left.PcieGpuPowerCableItemId == right.PcieGpuPowerCableItemId &&
                   left.MotherboardSecureOperationId ==
                       right.MotherboardSecureOperationId &&
                   left.ProcessorRetainOperationId ==
                       right.ProcessorRetainOperationId &&
                   left.MemoryRetainOperationId ==
                       right.MemoryRetainOperationId &&
                   left.StorageSecureOperationId ==
                       right.StorageSecureOperationId &&
                   left.ProcessorCoolerRetainOperationId ==
                       right.ProcessorCoolerRetainOperationId &&
                   left.GraphicsCardRetainOperationId ==
                       right.GraphicsCardRetainOperationId &&
                   left.PowerSupplyRetainOperationId ==
                       right.PowerSupplyRetainOperationId &&
                   left.Atx24RouteOperationId == right.Atx24RouteOperationId &&
                   left.Eps12vRouteOperationId == right.Eps12vRouteOperationId &&
                   left.PcieGpuRouteOperationId == right.PcieGpuRouteOperationId &&
                   left.AssemblyRevision == right.AssemblyRevision &&
                   left.Atx24PowerCableRevision ==
                       right.Atx24PowerCableRevision &&
                   left.Eps12vPowerCableRevision ==
                       right.Eps12vPowerCableRevision &&
                   left.PcieGpuPowerCableRevision ==
                       right.PcieGpuPowerCableRevision;
        }

        private readonly struct QualityReleaseLineage
        {
            internal QualityReleaseLineage(
                CustomPcBuildOrderRecord workOrder,
                CustomPcWorkTicketRecord workTicket,
                PcValidationReceipt validationReceipt,
                PcPowerStateReceipt powerOffReceipt)
            {
                WorkOrder = workOrder;
                WorkTicket = workTicket;
                ValidationReceipt = validationReceipt;
                PowerOffReceipt = powerOffReceipt;
            }

            internal CustomPcBuildOrderRecord WorkOrder { get; }

            internal CustomPcWorkTicketRecord WorkTicket { get; }

            internal PcValidationReceipt ValidationReceipt { get; }

            internal PcPowerStateReceipt PowerOffReceipt { get; }
        }
    }
}
