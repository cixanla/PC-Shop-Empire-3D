using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Assembly
{
    public sealed partial class AssemblyBuildAuthority
    {
        /// <summary>
        /// Evaluates the exact mechanical and cable prerequisites for a future power-on
        /// command without mutating any assembly, inventory or receipt state.
        /// </summary>
        public OperationResult<ElectricalReadinessSnapshot>
            EvaluateElectricalReadiness()
        {
            Failure blocker = EvaluateElectricalReadinessBlocker();
            if (!blocker.IsNone)
            {
                return OperationResult<ElectricalReadinessSnapshot>.Fail(blocker);
            }

            OperationResult invariants = ValidateInvariants();
            if (invariants.IsFailure ||
                !HasExactElectricalReadinessLineage())
            {
                return OperationResult<ElectricalReadinessSnapshot>.Fail(
                    ElectricalReadinessFailures.InvariantInvalid);
            }

            return OperationResult<ElectricalReadinessSnapshot>.Success(
                new ElectricalReadinessSnapshot(
                    BuildId,
                    ChassisId,
                    _motherboardItemId,
                    _processorItemId,
                    _memoryItemId,
                    _storageItemId,
                    _processorCoolerItemId,
                    _graphicsCardItemId,
                    _powerSupplyItemId,
                    _atx24PowerCableItemId,
                    _eps12vPowerCableItemId,
                    _pcieGpuPowerCableItemId,
                    _securedByOperationId,
                    _processorRetainedByOperationId,
                    _memoryRetainedByOperationId,
                    _storageSecuredByOperationId,
                    _processorCoolerRetainedByOperationId,
                    _graphicsCardRetainedByOperationId,
                    _powerSupplyRetainedByOperationId,
                    _atx24PowerCableRoutedByOperationId,
                    _eps12vPowerCableRoutedByOperationId,
                    _pcieGpuPowerCableRoutedByOperationId,
                    Revision,
                    Atx24PowerCableRevision,
                    Eps12vPowerCableRevision,
                    PcieGpuPowerCableRevision));
        }

        private Failure EvaluateElectricalReadinessBlocker()
        {
            if (_motherboardSeatState == AssemblySeatState.Empty)
            {
                return AssemblyFailures.MotherboardMissing;
            }

            if (_motherboardSeatState != AssemblySeatState.SeatedSecured)
            {
                return AssemblyFailures.MotherboardUnsecured;
            }

            if (!HasProcessorSocket ||
                !HasMemorySlot ||
                !HasStorageSlot ||
                !HasProcessorCoolerSlot ||
                !HasGraphicsCardSlot ||
                !HasPowerSupplyBay ||
                !HasAtx24PowerCableRoute ||
                !HasEps12vPowerCableRoute ||
                !HasPcieGpuPowerCableRoute)
            {
                return ElectricalReadinessFailures.ConfigurationUnsupported;
            }

            if (_processorSocketState == ProcessorSocketState.EmptyOpen)
            {
                return AssemblyFailures.ProcessorMissing;
            }

            if (_processorSocketState != ProcessorSocketState.ProcessorRetained)
            {
                return AssemblyFailures.ProcessorUnretained;
            }

            if (_memorySlotState == MemorySlotState.EmptyOpen)
            {
                return AssemblyFailures.MemoryMissing;
            }

            if (_memorySlotState != MemorySlotState.MemoryModuleRetained)
            {
                return AssemblyFailures.MemoryUnretained;
            }

            if (_storageSlotState == StorageSlotState.EmptyOpen)
            {
                return AssemblyFailures.StorageMissing;
            }

            if (_storageSlotState != StorageSlotState.StorageDeviceSecured)
            {
                return AssemblyFailures.StorageUnsecured;
            }

            if (_processorCoolerSlotState == ProcessorCoolerSlotState.EmptyOpen)
            {
                return AssemblyFailures.ProcessorCoolerMissing;
            }

            if (_processorCoolerSlotState != ProcessorCoolerSlotState.CoolerRetained ||
                _processorCoolerTimState != ProcessorCoolerTimState.AppliedConsumed)
            {
                return AssemblyFailures.ProcessorCoolerUnretained;
            }

            if (_graphicsCardSlotState == GraphicsCardSlotState.EmptyOpen)
            {
                return AssemblyFailures.GraphicsCardMissing;
            }

            if (_graphicsCardSlotState != GraphicsCardSlotState.GraphicsCardRetained)
            {
                return AssemblyFailures.GraphicsCardUnretained;
            }

            if (_powerSupplyBayState == PowerSupplyBayState.EmptyOpen)
            {
                return AssemblyFailures.PowerSupplyMissing;
            }

            if (_powerSupplyBayState != PowerSupplyBayState.PowerSupplyRetained)
            {
                return AssemblyFailures.PowerSupplyUnretained;
            }

            if (!IsAtx24PowerCableRouted)
            {
                return ElectricalReadinessFailures.Atx24PowerCableMissing;
            }

            if (!IsEps12vPowerCableRouted)
            {
                return ElectricalReadinessFailures.Eps12vPowerCableMissing;
            }

            return IsPcieGpuPowerCableRouted
                ? Failure.None
                : ElectricalReadinessFailures.PcieGpuPowerCableMissing;
        }

        private bool HasExactElectricalReadinessLineage()
        {
            return !BuildId.IsEmpty &&
                   !ChassisId.IsEmpty &&
                   !_motherboardItemId.IsEmpty &&
                   !_processorItemId.IsEmpty &&
                   !_memoryItemId.IsEmpty &&
                   !_storageItemId.IsEmpty &&
                   !_processorCoolerItemId.IsEmpty &&
                   !_graphicsCardItemId.IsEmpty &&
                   !_powerSupplyItemId.IsEmpty &&
                   !_atx24PowerCableItemId.IsEmpty &&
                   !_eps12vPowerCableItemId.IsEmpty &&
                   !_pcieGpuPowerCableItemId.IsEmpty &&
                   !_securedByOperationId.IsEmpty &&
                   !_processorRetainedByOperationId.IsEmpty &&
                   !_memoryRetainedByOperationId.IsEmpty &&
                   !_storageSecuredByOperationId.IsEmpty &&
                   !_processorCoolerRetainedByOperationId.IsEmpty &&
                   !_graphicsCardRetainedByOperationId.IsEmpty &&
                   !_powerSupplyRetainedByOperationId.IsEmpty &&
                   !_atx24PowerCableRoutedByOperationId.IsEmpty &&
                   !_eps12vPowerCableRoutedByOperationId.IsEmpty &&
                   !_pcieGpuPowerCableRoutedByOperationId.IsEmpty &&
                   Revision > 0 &&
                   Atx24PowerCableRevision > 0 &&
                   Eps12vPowerCableRevision > 0 &&
                   PcieGpuPowerCableRevision > 0;
        }
    }
}
