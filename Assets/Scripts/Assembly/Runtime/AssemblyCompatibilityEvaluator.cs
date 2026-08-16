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
    }
}
