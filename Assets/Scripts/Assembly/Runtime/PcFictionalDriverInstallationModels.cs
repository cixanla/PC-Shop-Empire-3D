using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Assembly
{
    public sealed class PcFictionalDriverInstallationOperationIdScope :
        IStableIdScope
    {
    }

    public enum PcFictionalDriverProfile
    {
        WorkshopDriverBundle = 1
    }

    public enum PcFictionalDriverInstallationResult
    {
        InstalledForBenchmarkStage = 1
    }

    /// <summary>
    /// Immutable evidence that the bounded fictional Workshop Driver Bundle was
    /// installed for one exact fictional OS and physical storage identity while a
    /// current power, POST and UEFI cycle was active. It is not a vendor driver,
    /// download, kernel service, reboot or benchmark result.
    /// </summary>
    public sealed class PcFictionalDriverInstallationReceipt
    {
        private readonly PcFictionalDriverInstallationAuthority _owner;

        internal PcFictionalDriverInstallationReceipt(
            PcFictionalDriverInstallationAuthority owner,
            StableId<PcFictionalDriverInstallationOperationIdScope> operationId,
            PcFictionalOsInstallationReceipt sourceOperatingSystemReceipt,
            PcFirmwareBaselineReceipt sourceFirmwareBaselineReceipt,
            StableId<AssemblyOperationIdScope>
                installationStorageSecureOperationId,
            long installationAssemblyRevision,
            long expectedPowerStateRevision,
            long expectedRevision,
            long revision)
        {
            _owner = owner;
            OperationId = operationId;
            SourceOperatingSystemReceipt = sourceOperatingSystemReceipt;
            SourceFirmwareBaselineReceipt = sourceFirmwareBaselineReceipt;
            InstallationStorageSecureOperationId =
                installationStorageSecureOperationId;
            InstallationAssemblyRevision = installationAssemblyRevision;
            ExpectedPowerStateRevision = expectedPowerStateRevision;
            ExpectedRevision = expectedRevision;
            Revision = revision;
        }

        public StableId<PcFictionalDriverInstallationOperationIdScope>
            OperationId { get; }

        public PcFictionalDriverProfile Profile =>
            PcFictionalDriverProfile.WorkshopDriverBundle;

        public PcFictionalDriverInstallationResult Result =>
            PcFictionalDriverInstallationResult.InstalledForBenchmarkStage;

        public PcFictionalOsInstallationReceipt SourceOperatingSystemReceipt
        {
            get;
        }

        public PcFirmwareBaselineReceipt SourceFirmwareBaselineReceipt
        {
            get;
        }

        public PcPostStartupReceipt SourcePostStartupReceipt =>
            SourceFirmwareBaselineReceipt?.SourcePostStartupReceipt;

        public PcPowerStateReceipt SourcePowerOnReceipt =>
            SourceFirmwareBaselineReceipt?.SourcePowerOnReceipt;

        public PowerTestAttemptReceipt PreflightReceipt =>
            SourceFirmwareBaselineReceipt?.PreflightReceipt;

        public PowerTestAttemptReceipt OperatingSystemPreflightReceipt =>
            SourceOperatingSystemReceipt?.PreflightReceipt;

        public StableId<ItemInstanceIdScope> StorageItemId =>
            SourceOperatingSystemReceipt != null
                ? SourceOperatingSystemReceipt.StorageItemId
                : default;

        public StableId<ProductDefinitionIdScope> StorageProductId =>
            SourceOperatingSystemReceipt != null
                ? SourceOperatingSystemReceipt.StorageProductId
                : default;

        public StableId<AssemblyOperationIdScope>
            InstallationStorageSecureOperationId { get; }

        public long InstallationAssemblyRevision { get; }

        public long SourceOperatingSystemRevision =>
            SourceOperatingSystemReceipt?.Revision ?? -1L;

        public long SourceFirmwareBaselineRevision =>
            SourceFirmwareBaselineReceipt?.Revision ?? -1L;

        public long ExpectedPowerStateRevision { get; }

        public long PowerStateRevision =>
            SourceFirmwareBaselineReceipt?.PowerStateRevision ?? -1L;

        public long ExpectedRevision { get; }

        public long Revision { get; }

        internal bool IsOwnedBy(
            PcFictionalDriverInstallationAuthority owner)
        {
            return owner != null && ReferenceEquals(_owner, owner);
        }

        internal bool MatchesCommand(
            StableId<PcFictionalDriverInstallationOperationIdScope> operationId,
            PcFictionalOsInstallationReceipt sourceOperatingSystemReceipt,
            PcFirmwareBaselineReceipt sourceFirmwareBaselineReceipt,
            StableId<ItemInstanceIdScope> expectedStorageItemId,
            long expectedPowerStateRevision,
            long expectedRevision)
        {
            return OperationId == operationId &&
                   ReferenceEquals(
                       SourceOperatingSystemReceipt,
                       sourceOperatingSystemReceipt) &&
                   ReferenceEquals(
                       SourceFirmwareBaselineReceipt,
                       sourceFirmwareBaselineReceipt) &&
                   StorageItemId == expectedStorageItemId &&
                   ExpectedPowerStateRevision == expectedPowerStateRevision &&
                   ExpectedRevision == expectedRevision;
        }
    }

    public static class PcFictionalDriverInstallationFailures
    {
        public static readonly Failure ConfigurationMissing = Failure.FromCode(
            "assembly.fictional-driver-installation.configuration-missing");
        public static readonly Failure AuthorityMismatch = Failure.FromCode(
            "assembly.fictional-driver-installation.authority-mismatch");
        public static readonly Failure InvalidOperationId = Failure.FromCode(
            "assembly.fictional-driver-installation.operation-id-invalid");
        public static readonly Failure InvalidOperatingSystemReceipt =
            Failure.FromCode(
                "assembly.fictional-driver-installation.os-receipt-invalid");
        public static readonly Failure InvalidFirmwareBaselineReceipt =
            Failure.FromCode(
                "assembly.fictional-driver-installation.firmware-receipt-invalid");
        public static readonly Failure InvalidStorageItem = Failure.FromCode(
            "assembly.fictional-driver-installation.storage-item-invalid");
        public static readonly Failure HardwareLineageNotCurrent =
            Failure.FromCode(
                "assembly.fictional-driver-installation.hardware-lineage-not-current");
        public static readonly Failure PowerStateRevisionMismatch =
            Failure.FromCode(
                "assembly.fictional-driver-installation.power-state-revision-mismatch");
        public static readonly Failure RevisionMismatch = Failure.FromCode(
            "assembly.fictional-driver-installation.revision-mismatch");
        public static readonly Failure RevisionOverflow = Failure.FromCode(
            "assembly.fictional-driver-installation.revision-overflow");
        public static readonly Failure OperationConflict = Failure.FromCode(
            "assembly.fictional-driver-installation.operation-conflict");
        public static readonly Failure AlreadyCompleted = Failure.FromCode(
            "assembly.fictional-driver-installation.already-completed");
        public static readonly Failure NotInstalled = Failure.FromCode(
            "assembly.fictional-driver-installation.not-installed");
        public static readonly Failure NotCurrent = Failure.FromCode(
            "assembly.fictional-driver-installation.not-current");
        public static readonly Failure ReceiptHistoryInvalid = Failure.FromCode(
            "assembly.fictional-driver-installation.receipt-history-invalid");
    }
}
