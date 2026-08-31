using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Assembly
{
    public sealed class PcFictionalOsInstallationOperationIdScope : IStableIdScope
    {
    }

    public enum PcFictionalOsProfile
    {
        WorkshopStandard = 1
    }

    public enum PcFictionalOsInstallationResult
    {
        InstalledForDriverStage = 1
    }

    /// <summary>
    /// Immutable evidence that one exact physical storage item received the bounded
    /// fictional Workshop Standard OS while the source UEFI baseline was current.
    /// It is not a real operating system, disk image, boot, driver or benchmark result.
    /// </summary>
    public sealed class PcFictionalOsInstallationReceipt
    {
        private readonly PcFictionalOsInstallationAuthority _owner;

        internal PcFictionalOsInstallationReceipt(
            PcFictionalOsInstallationAuthority owner,
            StableId<PcFictionalOsInstallationOperationIdScope> operationId,
            PcFirmwareBaselineReceipt sourceFirmwareBaselineReceipt,
            StableId<ItemInstanceIdScope> storageItemId,
            StableId<ProductDefinitionIdScope> storageProductId,
            StableId<AssemblyOperationIdScope> sourceStorageSecureOperationId,
            long sourceAssemblyRevision,
            long expectedPowerStateRevision,
            long expectedRevision,
            long revision)
        {
            _owner = owner;
            OperationId = operationId;
            SourceFirmwareBaselineReceipt = sourceFirmwareBaselineReceipt;
            StorageItemId = storageItemId;
            StorageProductId = storageProductId;
            SourceStorageSecureOperationId = sourceStorageSecureOperationId;
            SourceAssemblyRevision = sourceAssemblyRevision;
            ExpectedPowerStateRevision = expectedPowerStateRevision;
            ExpectedRevision = expectedRevision;
            Revision = revision;
        }

        public StableId<PcFictionalOsInstallationOperationIdScope> OperationId
        {
            get;
        }

        public PcFictionalOsProfile Profile =>
            PcFictionalOsProfile.WorkshopStandard;

        public PcFictionalOsInstallationResult Result =>
            PcFictionalOsInstallationResult.InstalledForDriverStage;

        public PcFirmwareBaselineReceipt SourceFirmwareBaselineReceipt { get; }

        public PcPostStartupReceipt SourcePostStartupReceipt =>
            SourceFirmwareBaselineReceipt?.SourcePostStartupReceipt;

        public PcPowerStateReceipt SourcePowerOnReceipt =>
            SourceFirmwareBaselineReceipt?.SourcePowerOnReceipt;

        public PowerTestAttemptReceipt PreflightReceipt =>
            SourceFirmwareBaselineReceipt?.PreflightReceipt;

        public StableId<ItemInstanceIdScope> StorageItemId { get; }

        public StableId<ProductDefinitionIdScope> StorageProductId { get; }

        public StableId<AssemblyOperationIdScope> SourceStorageSecureOperationId
        {
            get;
        }

        public long SourceAssemblyRevision { get; }

        public long SourceFirmwareBaselineRevision =>
            SourceFirmwareBaselineReceipt?.Revision ?? -1L;

        public long ExpectedPowerStateRevision { get; }

        public long PowerStateRevision =>
            SourceFirmwareBaselineReceipt?.PowerStateRevision ?? -1L;

        public long ExpectedRevision { get; }

        public long Revision { get; }

        internal bool IsOwnedBy(PcFictionalOsInstallationAuthority owner)
        {
            return owner != null && ReferenceEquals(_owner, owner);
        }

        internal bool MatchesCommand(
            StableId<PcFictionalOsInstallationOperationIdScope> operationId,
            PcFirmwareBaselineReceipt sourceFirmwareBaselineReceipt,
            StableId<ItemInstanceIdScope> expectedStorageItemId,
            long expectedPowerStateRevision,
            long expectedRevision)
        {
            return OperationId == operationId &&
                   ReferenceEquals(
                       SourceFirmwareBaselineReceipt,
                       sourceFirmwareBaselineReceipt) &&
                   StorageItemId == expectedStorageItemId &&
                   ExpectedPowerStateRevision == expectedPowerStateRevision &&
                   ExpectedRevision == expectedRevision;
        }
    }

    public static class PcFictionalOsInstallationFailures
    {
        public static readonly Failure ConfigurationMissing = Failure.FromCode(
            "assembly.fictional-os-installation.configuration-missing");
        public static readonly Failure AuthorityMismatch = Failure.FromCode(
            "assembly.fictional-os-installation.authority-mismatch");
        public static readonly Failure InvalidOperationId = Failure.FromCode(
            "assembly.fictional-os-installation.operation-id-invalid");
        public static readonly Failure InvalidFirmwareBaselineReceipt = Failure.FromCode(
            "assembly.fictional-os-installation.firmware-baseline-receipt-invalid");
        public static readonly Failure InvalidStorageItem = Failure.FromCode(
            "assembly.fictional-os-installation.storage-item-invalid");
        public static readonly Failure StorageNotReady = Failure.FromCode(
            "assembly.fictional-os-installation.storage-not-ready");
        public static readonly Failure PowerStateRevisionMismatch = Failure.FromCode(
            "assembly.fictional-os-installation.power-state-revision-mismatch");
        public static readonly Failure RevisionMismatch = Failure.FromCode(
            "assembly.fictional-os-installation.revision-mismatch");
        public static readonly Failure RevisionOverflow = Failure.FromCode(
            "assembly.fictional-os-installation.revision-overflow");
        public static readonly Failure OperationConflict = Failure.FromCode(
            "assembly.fictional-os-installation.operation-conflict");
        public static readonly Failure AlreadyCompleted = Failure.FromCode(
            "assembly.fictional-os-installation.already-completed");
        public static readonly Failure NotInstalled = Failure.FromCode(
            "assembly.fictional-os-installation.not-installed");
        public static readonly Failure NotCurrent = Failure.FromCode(
            "assembly.fictional-os-installation.not-current");
        public static readonly Failure ReceiptHistoryInvalid = Failure.FromCode(
            "assembly.fictional-os-installation.receipt-history-invalid");
    }
}
