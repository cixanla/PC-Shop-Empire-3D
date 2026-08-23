using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Assembly
{
    public readonly struct AssemblyCompatibilityResult
    {
        private AssemblyCompatibilityResult(bool isCompatible, Failure reason)
        {
            IsCompatible = isCompatible;
            Reason = reason;
        }

        public bool IsCompatible { get; }

        public Failure Reason { get; }

        public static AssemblyCompatibilityResult Compatible()
        {
            return new AssemblyCompatibilityResult(true, Failure.None);
        }

        public static AssemblyCompatibilityResult Incompatible(Failure reason)
        {
            return new AssemblyCompatibilityResult(false, reason);
        }
    }

    public static class AssemblyCompatibilityEvaluator
    {
        public static AssemblyCompatibilityResult EvaluateMotherboardSeat(
            PcComponentSpecification specification,
            MotherboardFormFactor supportedFormFactor)
        {
            if (specification == null)
            {
                return AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.UnknownComponentSpecification);
            }

            if (!PcComponentSpecification.IsValidMotherboardFormFactor(supportedFormFactor))
            {
                return AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.InvalidMotherboardFormFactor);
            }

            if (specification.Kind != PcComponentKind.Motherboard)
            {
                return AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.UnsupportedComponentKind);
            }

            return specification.MotherboardFormFactor == supportedFormFactor
                ? AssemblyCompatibilityResult.Compatible()
                : AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.MotherboardFormFactorMismatch);
        }

        public static AssemblyCompatibilityResult EvaluateProcessorSeat(
            PcComponentSpecification processorSpecification,
            PcComponentSpecification motherboardSpecification,
            CpuSocketFamily supportedSocketFamily)
        {
            if (processorSpecification == null || motherboardSpecification == null)
            {
                return AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.UnknownComponentSpecification);
            }

            if (!PcComponentSpecification.IsValidCpuSocketFamily(supportedSocketFamily))
            {
                return AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.InvalidCpuSocketFamily);
            }

            if (processorSpecification.Kind != PcComponentKind.Processor ||
                motherboardSpecification.Kind != PcComponentKind.Motherboard)
            {
                return AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.UnsupportedComponentKind);
            }

            return processorSpecification.CpuSocketFamily == supportedSocketFamily &&
                   motherboardSpecification.CpuSocketFamily == supportedSocketFamily
                ? AssemblyCompatibilityResult.Compatible()
                : AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.CpuSocketFamilyMismatch);
        }

        public static AssemblyCompatibilityResult EvaluateMemoryModuleSeat(
            PcComponentSpecification memoryModuleSpecification,
            PcComponentSpecification motherboardSpecification,
            DimmType supportedDimmType,
            DimmKeyOrientation orientation)
        {
            if (memoryModuleSpecification == null || motherboardSpecification == null)
            {
                return AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.UnknownComponentSpecification);
            }

            if (!PcComponentSpecification.IsValidDimmType(supportedDimmType))
            {
                return AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.InvalidDimmType);
            }

            if (orientation != DimmKeyOrientation.NotchAligned &&
                orientation != DimmKeyOrientation.Reversed)
            {
                return AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.InvalidDimmOrientation);
            }

            if (orientation != DimmKeyOrientation.NotchAligned)
            {
                return AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.DimmOrientationMismatch);
            }

            if (memoryModuleSpecification.Kind != PcComponentKind.MemoryModule ||
                motherboardSpecification.Kind != PcComponentKind.Motherboard)
            {
                return AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.UnsupportedComponentKind);
            }

            return memoryModuleSpecification.DimmType == supportedDimmType &&
                   motherboardSpecification.DimmType == supportedDimmType
                ? AssemblyCompatibilityResult.Compatible()
                : AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.DimmTypeMismatch);
        }

        public static AssemblyCompatibilityResult EvaluateStorageDeviceSeat(
            PcComponentSpecification storageDeviceSpecification,
            PcComponentSpecification motherboardSpecification,
            M2StorageType supportedM2StorageType,
            M2KeyOrientation orientation)
        {
            if (storageDeviceSpecification == null || motherboardSpecification == null)
            {
                return AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.UnknownComponentSpecification);
            }

            if (!PcComponentSpecification.IsValidM2StorageType(supportedM2StorageType))
            {
                return AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.InvalidM2StorageType);
            }

            if (orientation != M2KeyOrientation.KeyAligned &&
                orientation != M2KeyOrientation.Reversed)
            {
                return AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.InvalidM2Orientation);
            }

            if (orientation != M2KeyOrientation.KeyAligned)
            {
                return AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.M2OrientationMismatch);
            }

            if (storageDeviceSpecification.Kind != PcComponentKind.StorageDevice ||
                motherboardSpecification.Kind != PcComponentKind.Motherboard)
            {
                return AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.UnsupportedComponentKind);
            }

            return storageDeviceSpecification.M2StorageType == supportedM2StorageType &&
                   motherboardSpecification.M2StorageType == supportedM2StorageType
                ? AssemblyCompatibilityResult.Compatible()
                : AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.M2StorageTypeMismatch);
        }

        public static AssemblyCompatibilityResult EvaluateProcessorCoolerSeat(
            PcComponentSpecification processorCoolerSpecification,
            PcComponentSpecification motherboardSpecification,
            PcComponentSpecification processorSpecification,
            ProcessorCoolerType supportedProcessorCoolerType,
            CpuSocketFamily supportedSocketFamily,
            ProcessorCoolerMountOrientation orientation)
        {
            if (processorCoolerSpecification == null ||
                motherboardSpecification == null ||
                processorSpecification == null)
            {
                return AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.UnknownComponentSpecification);
            }

            if (!PcComponentSpecification.IsValidProcessorCoolerType(
                    supportedProcessorCoolerType))
            {
                return AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.InvalidProcessorCoolerType);
            }

            if (!PcComponentSpecification.IsValidCpuSocketFamily(supportedSocketFamily))
            {
                return AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.InvalidCpuSocketFamily);
            }

            if (orientation != ProcessorCoolerMountOrientation.Primary &&
                orientation != ProcessorCoolerMountOrientation.Rotated180)
            {
                return AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.InvalidProcessorCoolerOrientation);
            }

            if (processorCoolerSpecification.Kind != PcComponentKind.ProcessorCooler ||
                motherboardSpecification.Kind != PcComponentKind.Motherboard ||
                processorSpecification.Kind != PcComponentKind.Processor)
            {
                return AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.UnsupportedComponentKind);
            }

            if (processorCoolerSpecification.ProcessorCoolerType !=
                supportedProcessorCoolerType)
            {
                return AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.ProcessorCoolerTypeMismatch);
            }

            return processorCoolerSpecification.CpuSocketFamily == supportedSocketFamily &&
                   motherboardSpecification.CpuSocketFamily == supportedSocketFamily &&
                   processorSpecification.CpuSocketFamily == supportedSocketFamily
                ? AssemblyCompatibilityResult.Compatible()
                : AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.ProcessorCoolerSocketMismatch);
        }

        public static AssemblyCompatibilityResult EvaluateGraphicsCardSeat(
            PcComponentSpecification graphicsCardSpecification,
            PcComponentSpecification motherboardSpecification,
            GraphicsCardType supportedGraphicsCardType,
            GraphicsCardMountOrientation orientation)
        {
            if (graphicsCardSpecification == null || motherboardSpecification == null)
            {
                return AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.UnknownComponentSpecification);
            }

            if (!PcComponentSpecification.IsValidGraphicsCardType(
                    supportedGraphicsCardType))
            {
                return AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.InvalidGraphicsCardType);
            }

            if (orientation != GraphicsCardMountOrientation.Primary &&
                orientation != GraphicsCardMountOrientation.Rotated180)
            {
                return AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.InvalidGraphicsCardOrientation);
            }

            if (orientation != GraphicsCardMountOrientation.Primary)
            {
                return AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.GraphicsCardOrientationMismatch);
            }

            if (graphicsCardSpecification.Kind != PcComponentKind.GraphicsCard ||
                motherboardSpecification.Kind != PcComponentKind.Motherboard)
            {
                return AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.UnsupportedComponentKind);
            }

            return graphicsCardSpecification.GraphicsCardType ==
                       supportedGraphicsCardType &&
                   motherboardSpecification.GraphicsCardType ==
                       supportedGraphicsCardType
                ? AssemblyCompatibilityResult.Compatible()
                : AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.GraphicsCardTypeMismatch);
        }

        public static AssemblyCompatibilityResult EvaluatePowerSupplySeat(
            PcComponentSpecification powerSupplySpecification,
            PowerSupplyType supportedPowerSupplyType,
            PowerSupplyMountOrientation orientation)
        {
            if (powerSupplySpecification == null)
            {
                return AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.UnknownComponentSpecification);
            }

            if (!PcComponentSpecification.IsValidPowerSupplyType(
                    supportedPowerSupplyType))
            {
                return AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.InvalidPowerSupplyType);
            }

            if (orientation != PowerSupplyMountOrientation.FanToFilteredVent &&
                orientation != PowerSupplyMountOrientation.FanAwayFromFilteredVent)
            {
                return AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.InvalidPowerSupplyOrientation);
            }

            if (orientation != PowerSupplyMountOrientation.FanToFilteredVent)
            {
                return AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.PowerSupplyOrientationMismatch);
            }

            if (powerSupplySpecification.Kind != PcComponentKind.PowerSupply)
            {
                return AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.UnsupportedComponentKind);
            }

            return powerSupplySpecification.PowerSupplyType == supportedPowerSupplyType
                ? AssemblyCompatibilityResult.Compatible()
                : AssemblyCompatibilityResult.Incompatible(
                    AssemblyFailures.PowerSupplyTypeMismatch);
        }
    }
}
