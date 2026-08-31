using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Fulfillment;
using PCShopEmpire3D.Quality;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class GarageStockFlowSession
    {
        public const string PrototypeCustomPcPackageIdValue =
            "fulfillment.custom-pc-package.prototype-001";
        public const string PrototypeCustomPcPackageSealOperationIdValue =
            "fulfillment.custom-pc-package-seal.prototype-001";

        private CustomPcPackageAuthority _customPcPackages;

        public StableId<CustomPcPackageIdScope> PrototypeCustomPcPackageId =>
            StableId<CustomPcPackageIdScope>.Parse(
                PrototypeCustomPcPackageIdValue);

        public CustomPcPackageAuthority CustomPcPackages
        {
            get
            {
                OperationResult<CustomPcPackageAuthority> ensured =
                    EnsureCustomPcPackageAuthority();
                return ensured.TryGetValue(out CustomPcPackageAuthority authority)
                    ? authority
                    : null;
            }
        }

        public OperationResult<CustomPcPackageAuthority>
            EnsureCustomPcPackageAuthority()
        {
            if (_customPcPackages != null)
            {
                return OperationResult<CustomPcPackageAuthority>.Success(
                    _customPcPackages);
            }

            OperationResult<CustomPcQualityReleaseAuthority> quality =
                EnsureQualityReleaseAuthority();
            if (quality.IsFailure)
            {
                return OperationResult<CustomPcPackageAuthority>.Fail(
                    quality.Error);
            }

            OperationResult<CustomPcPackageAuthority> created =
                CustomPcPackageAuthority.Create(quality.Value);
            if (created.IsFailure)
            {
                return created;
            }

            _customPcPackages = created.Value;
            return OperationResult<CustomPcPackageAuthority>.Success(
                _customPcPackages);
        }

        public bool TryGetCustomPcPackageAuthority(
            out CustomPcPackageAuthority authority)
        {
            authority = _customPcPackages;
            return authority != null;
        }

        public bool TryGetPrototypeCustomPcPackage(
            out CustomPcPackageReceipt package)
        {
            package = null;
            return _customPcPackages != null &&
                   _customPcPackages.TryGetPackage(
                       PrototypeCustomPcPackageId,
                       out package);
        }

        public StableId<CustomPcPackageSealOperationIdScope>
            CreatePrototypeCustomPcPackageSealOperationId()
        {
            return StableId<CustomPcPackageSealOperationIdScope>.Parse(
                PrototypeCustomPcPackageSealOperationIdValue);
        }

        public StableId<CustomPcPackageCustodyOperationIdScope>
            CreatePrototypeCustomPcPackageCustodyOperationId(
                CustomPcPackageCustody source,
                CustomPcPackageCustody target,
                long expectedRevision)
        {
            return StableId<CustomPcPackageCustodyOperationIdScope>.Parse(
                "fulfillment.custom-pc-package-custody.prototype-001." +
                source.ToString().ToLowerInvariant() + "-to-" +
                target.ToString().ToLowerInvariant() + ".run-" +
                (expectedRevision + 1L));
        }
    }
}
