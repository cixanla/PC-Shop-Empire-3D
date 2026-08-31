using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.Quality;
using PCShopEmpire3D.Retail;

namespace PCShopEmpire3D.Fulfillment
{
    public sealed class CustomPcPackageIdScope : IStableIdScope
    {
    }

    public sealed class CustomPcPackageSealOperationIdScope : IStableIdScope
    {
    }

    public sealed class CustomPcPackageCustodyOperationIdScope : IStableIdScope
    {
    }

    public enum CustomPcPackageState
    {
        Sealed = 1
    }

    public enum CustomPcPackageCustody
    {
        PackagingWorkbench = 1,
        ActorHands = 2,
        WorldFloor = 3,
        TransportCart = 4,
        DispatchStaging = 5
    }

    /// <summary>
    /// Immutable proof that one exact quality-approved customer PC became one sealed
    /// package. Component inventory and Assembly remain authoritative and are never
    /// duplicated into a second stock item.
    /// </summary>
    public sealed class CustomPcPackageReceipt
    {
        private readonly CustomPcPackageAuthority _owner;

        internal CustomPcPackageReceipt(
            CustomPcPackageAuthority owner,
            StableId<CustomPcPackageIdScope> packageId,
            StableId<CustomPcPackageSealOperationIdScope> operationId,
            CustomPcQualityReleaseReceipt sourceQualityReleaseReceipt,
            long expectedRevision,
            long revision)
        {
            _owner = owner;
            PackageId = packageId;
            OperationId = operationId;
            SourceQualityReleaseReceipt = sourceQualityReleaseReceipt;
            ExpectedRevision = expectedRevision;
            Revision = revision;
        }

        public StableId<CustomPcPackageIdScope> PackageId { get; }

        public StableId<CustomPcPackageSealOperationIdScope> OperationId { get; }

        public CustomPcPackageState State => CustomPcPackageState.Sealed;

        public CustomPcPackageCustody InitialCustody =>
            CustomPcPackageCustody.PackagingWorkbench;

        public CustomPcQualityReleaseReceipt SourceQualityReleaseReceipt { get; }

        public StableId<CustomPcQualityReleaseOperationIdScope>
            SourceQualityReleaseOperationId =>
                SourceQualityReleaseReceipt.OperationId;

        public StableId<CustomPcBuildOrderIdScope> WorkOrderId =>
            SourceQualityReleaseReceipt.WorkOrderId;

        public StableId<CustomPcWorkTicketIdScope> WorkTicketId =>
            SourceQualityReleaseReceipt.WorkTicketId;

        public StableId<CustomerRetailIdentityBindingIdScope> CustomerBindingId =>
            SourceQualityReleaseReceipt.CustomerBindingId;

        public StableId<InventoryClaimIdScope> InventoryClaimId =>
            SourceQualityReleaseReceipt.InventoryClaimId;

        public StableId<PcBuildIdScope> BuildId =>
            SourceQualityReleaseReceipt.BuildId;

        public StableId<ChassisIdScope> ChassisId =>
            SourceQualityReleaseReceipt.ChassisId;

        public long ExpectedRevision { get; }

        public long Revision { get; }

        internal bool IsOwnedBy(CustomPcPackageAuthority owner)
        {
            return owner != null && ReferenceEquals(_owner, owner);
        }

        internal bool MatchesCommand(
            StableId<CustomPcPackageIdScope> packageId,
            StableId<CustomPcPackageSealOperationIdScope> operationId,
            CustomPcQualityReleaseReceipt sourceQualityReleaseReceipt,
            long expectedRevision)
        {
            return PackageId == packageId &&
                   OperationId == operationId &&
                   ReferenceEquals(
                       SourceQualityReleaseReceipt,
                       sourceQualityReleaseReceipt) &&
                   ExpectedRevision == expectedRevision;
        }
    }

    /// <summary>
    /// Append-only custody transition for the same sealed physical package.
    /// </summary>
    public sealed class CustomPcPackageCustodyReceipt
    {
        private readonly CustomPcPackageAuthority _owner;

        internal CustomPcPackageCustodyReceipt(
            CustomPcPackageAuthority owner,
            StableId<CustomPcPackageCustodyOperationIdScope> operationId,
            CustomPcPackageReceipt package,
            CustomPcPackageCustody source,
            CustomPcPackageCustody target,
            long expectedRevision,
            long revision)
        {
            _owner = owner;
            OperationId = operationId;
            Package = package;
            Source = source;
            Target = target;
            ExpectedRevision = expectedRevision;
            Revision = revision;
        }

        public StableId<CustomPcPackageCustodyOperationIdScope> OperationId { get; }

        public CustomPcPackageReceipt Package { get; }

        public StableId<CustomPcPackageIdScope> PackageId => Package.PackageId;

        public CustomPcPackageCustody Source { get; }

        public CustomPcPackageCustody Target { get; }

        public long ExpectedRevision { get; }

        public long Revision { get; }

        internal bool IsOwnedBy(CustomPcPackageAuthority owner)
        {
            return owner != null && ReferenceEquals(_owner, owner);
        }

        internal bool MatchesCommand(
            StableId<CustomPcPackageCustodyOperationIdScope> operationId,
            CustomPcPackageReceipt package,
            CustomPcPackageCustody source,
            CustomPcPackageCustody target,
            long expectedRevision)
        {
            return OperationId == operationId &&
                   ReferenceEquals(Package, package) &&
                   Source == source &&
                   Target == target &&
                   ExpectedRevision == expectedRevision;
        }
    }

    public static class CustomPcPackageFailures
    {
        public static readonly Failure ConfigurationMissing = Failure.FromCode(
            "fulfillment.custom-pc-package.configuration-missing");
        public static readonly Failure InvalidPackageId = Failure.FromCode(
            "fulfillment.custom-pc-package.package-id-invalid");
        public static readonly Failure InvalidOperationId = Failure.FromCode(
            "fulfillment.custom-pc-package.operation-id-invalid");
        public static readonly Failure QualityReleaseInvalid = Failure.FromCode(
            "fulfillment.custom-pc-package.quality-release-invalid");
        public static readonly Failure QualityReleaseNotCurrent = Failure.FromCode(
            "fulfillment.custom-pc-package.quality-release-not-current");
        public static readonly Failure PackageAlreadyExists = Failure.FromCode(
            "fulfillment.custom-pc-package.already-exists");
        public static readonly Failure PackageInvalid = Failure.FromCode(
            "fulfillment.custom-pc-package.package-invalid");
        public static readonly Failure CustodyInvalid = Failure.FromCode(
            "fulfillment.custom-pc-package.custody-invalid");
        public static readonly Failure CustodyTransitionInvalid = Failure.FromCode(
            "fulfillment.custom-pc-package.custody-transition-invalid");
        public static readonly Failure RevisionMismatch = Failure.FromCode(
            "fulfillment.custom-pc-package.revision-mismatch");
        public static readonly Failure RevisionOverflow = Failure.FromCode(
            "fulfillment.custom-pc-package.revision-overflow");
        public static readonly Failure OperationConflict = Failure.FromCode(
            "fulfillment.custom-pc-package.operation-conflict");
        public static readonly Failure ReceiptHistoryInvalid = Failure.FromCode(
            "fulfillment.custom-pc-package.receipt-history-invalid");
        public static readonly Failure NotCurrent = Failure.FromCode(
            "fulfillment.custom-pc-package.not-current");
    }
}
