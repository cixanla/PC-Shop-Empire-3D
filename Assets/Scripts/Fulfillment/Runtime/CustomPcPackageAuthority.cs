using System;
using System.Collections.Generic;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Quality;

namespace PCShopEmpire3D.Fulfillment
{
    /// <summary>
    /// Owns sealed-package identity and append-only physical custody only. The source
    /// quality authority remains authoritative for the customer job and assembled PC.
    /// </summary>
    public sealed class CustomPcPackageAuthority
    {
        private readonly CustomPcQualityReleaseAuthority _quality;
        private readonly Dictionary<
            StableId<CustomPcPackageIdScope>,
            CustomPcPackageReceipt> _packages =
                new Dictionary<
                    StableId<CustomPcPackageIdScope>,
                    CustomPcPackageReceipt>();
        private readonly Dictionary<
            StableId<CustomPcQualityReleaseOperationIdScope>,
            CustomPcPackageReceipt> _packagesByQualityRelease =
                new Dictionary<
                    StableId<CustomPcQualityReleaseOperationIdScope>,
                    CustomPcPackageReceipt>();
        private readonly Dictionary<
            StableId<CustomPcPackageSealOperationIdScope>,
            CustomPcPackageReceipt> _packagesByOperation =
                new Dictionary<
                    StableId<CustomPcPackageSealOperationIdScope>,
                    CustomPcPackageReceipt>();
        private readonly Dictionary<
            StableId<CustomPcPackageCustodyOperationIdScope>,
            CustomPcPackageCustodyReceipt> _custodyByOperation =
                new Dictionary<
                    StableId<CustomPcPackageCustodyOperationIdScope>,
                    CustomPcPackageCustodyReceipt>();
        private readonly Dictionary<
            StableId<CustomPcPackageIdScope>,
            CustomPcPackageCustody> _currentCustody =
                new Dictionary<
                    StableId<CustomPcPackageIdScope>,
                    CustomPcPackageCustody>();
        private readonly List<object> _history = new List<object>();

        private CustomPcPackageAuthority(
            CustomPcQualityReleaseAuthority quality)
        {
            _quality = quality;
        }

        public CustomPcQualityReleaseAuthority Quality => _quality;

        public long Revision { get; private set; }

        public int PackageCount => _packages.Count;

        public int CustodyReceiptCount => _custodyByOperation.Count;

        public static OperationResult<CustomPcPackageAuthority> Create(
            CustomPcQualityReleaseAuthority quality)
        {
            if (quality == null || quality.ValidateReceiptHistory().IsFailure)
            {
                return OperationResult<CustomPcPackageAuthority>.Fail(
                    CustomPcPackageFailures.ConfigurationMissing);
            }

            return OperationResult<CustomPcPackageAuthority>.Success(
                new CustomPcPackageAuthority(quality));
        }

        public OperationResult<CustomPcPackageReceipt> TrySealPackage(
            StableId<CustomPcPackageIdScope> packageId,
            StableId<CustomPcPackageSealOperationIdScope> operationId,
            CustomPcQualityReleaseReceipt sourceQualityReleaseReceipt,
            long expectedRevision)
        {
            if (packageId.IsEmpty)
            {
                return OperationResult<CustomPcPackageReceipt>.Fail(
                    CustomPcPackageFailures.InvalidPackageId);
            }

            if (operationId.IsEmpty)
            {
                return OperationResult<CustomPcPackageReceipt>.Fail(
                    CustomPcPackageFailures.InvalidOperationId);
            }

            OperationResult history = ValidateReceiptHistory();
            if (history.IsFailure)
            {
                return OperationResult<CustomPcPackageReceipt>.Fail(history.Error);
            }

            if (_packagesByOperation.TryGetValue(
                    operationId,
                    out CustomPcPackageReceipt replay))
            {
                return replay.MatchesCommand(
                        packageId,
                        operationId,
                        sourceQualityReleaseReceipt,
                        expectedRevision)
                    ? OperationResult<CustomPcPackageReceipt>.Success(replay)
                    : OperationResult<CustomPcPackageReceipt>.Fail(
                        CustomPcPackageFailures.OperationConflict);
            }

            if (expectedRevision != Revision)
            {
                return OperationResult<CustomPcPackageReceipt>.Fail(
                    CustomPcPackageFailures.RevisionMismatch);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult<CustomPcPackageReceipt>.Fail(
                    CustomPcPackageFailures.RevisionOverflow);
            }

            Failure sourceFailure = ValidateCurrentQualityRelease(
                sourceQualityReleaseReceipt);
            if (!sourceFailure.IsNone)
            {
                return OperationResult<CustomPcPackageReceipt>.Fail(sourceFailure);
            }

            if (_packages.ContainsKey(packageId) ||
                _packagesByQualityRelease.ContainsKey(
                    sourceQualityReleaseReceipt.OperationId))
            {
                return OperationResult<CustomPcPackageReceipt>.Fail(
                    CustomPcPackageFailures.PackageAlreadyExists);
            }

            long nextRevision = Revision + 1L;
            var receipt = new CustomPcPackageReceipt(
                this,
                packageId,
                operationId,
                sourceQualityReleaseReceipt,
                expectedRevision,
                nextRevision);
            _packages.Add(packageId, receipt);
            _packagesByQualityRelease.Add(
                sourceQualityReleaseReceipt.OperationId,
                receipt);
            _packagesByOperation.Add(operationId, receipt);
            _currentCustody.Add(
                packageId,
                CustomPcPackageCustody.PackagingWorkbench);
            _history.Add(receipt);
            Revision = nextRevision;
            return OperationResult<CustomPcPackageReceipt>.Success(receipt);
        }

        public OperationResult<CustomPcPackageCustodyReceipt> TryTransferCustody(
            StableId<CustomPcPackageCustodyOperationIdScope> operationId,
            CustomPcPackageReceipt package,
            CustomPcPackageCustody source,
            CustomPcPackageCustody target,
            long expectedRevision)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<CustomPcPackageCustodyReceipt>.Fail(
                    CustomPcPackageFailures.InvalidOperationId);
            }

            OperationResult history = ValidateReceiptHistory();
            if (history.IsFailure)
            {
                return OperationResult<CustomPcPackageCustodyReceipt>.Fail(
                    history.Error);
            }

            if (_custodyByOperation.TryGetValue(
                    operationId,
                    out CustomPcPackageCustodyReceipt replay))
            {
                return replay.MatchesCommand(
                        operationId,
                        package,
                        source,
                        target,
                        expectedRevision)
                    ? OperationResult<CustomPcPackageCustodyReceipt>.Success(replay)
                    : OperationResult<CustomPcPackageCustodyReceipt>.Fail(
                        CustomPcPackageFailures.OperationConflict);
            }

            OperationResult validation = ValidateCustodyTransfer(
                package,
                source,
                target,
                expectedRevision);
            if (validation.IsFailure)
            {
                return OperationResult<CustomPcPackageCustodyReceipt>.Fail(
                    validation.Error);
            }

            long nextRevision = Revision + 1L;
            var receipt = new CustomPcPackageCustodyReceipt(
                this,
                operationId,
                package,
                source,
                target,
                expectedRevision,
                nextRevision);
            _custodyByOperation.Add(operationId, receipt);
            _currentCustody[package.PackageId] = target;
            _history.Add(receipt);
            Revision = nextRevision;
            return OperationResult<CustomPcPackageCustodyReceipt>.Success(receipt);
        }

        /// <summary>
        /// Side-effect-free gate used before moving the physical package. A subsequent
        /// transfer with the same revision is deterministic on the single write lane.
        /// </summary>
        public OperationResult ValidateCustodyTransfer(
            CustomPcPackageReceipt package,
            CustomPcPackageCustody source,
            CustomPcPackageCustody target,
            long expectedRevision)
        {
            OperationResult history = ValidateReceiptHistory();
            if (history.IsFailure)
            {
                return history;
            }

            if (expectedRevision != Revision)
            {
                return OperationResult.Fail(
                    CustomPcPackageFailures.RevisionMismatch);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult.Fail(
                    CustomPcPackageFailures.RevisionOverflow);
            }

            if (!OwnsPackage(package))
            {
                return OperationResult.Fail(
                    CustomPcPackageFailures.PackageInvalid);
            }

            if (!_currentCustody.TryGetValue(
                    package.PackageId,
                    out CustomPcPackageCustody current) ||
                current != source)
            {
                return OperationResult.Fail(
                    CustomPcPackageFailures.CustodyInvalid);
            }

            return IsAllowedTransition(source, target)
                ? OperationResult.Success()
                : OperationResult.Fail(
                    CustomPcPackageFailures.CustodyTransitionInvalid);
        }

        public OperationResult<CustomPcPackageReceipt> EvaluateCurrentPackage(
            StableId<CustomPcPackageIdScope> packageId)
        {
            OperationResult history = ValidateReceiptHistory();
            if (history.IsFailure)
            {
                return OperationResult<CustomPcPackageReceipt>.Fail(history.Error);
            }

            if (!_packages.TryGetValue(
                    packageId,
                    out CustomPcPackageReceipt package) ||
                !ValidateCurrentQualityRelease(
                    package.SourceQualityReleaseReceipt).IsNone)
            {
                return OperationResult<CustomPcPackageReceipt>.Fail(
                    CustomPcPackageFailures.NotCurrent);
            }

            return OperationResult<CustomPcPackageReceipt>.Success(package);
        }

        public bool TryGetPackage(
            StableId<CustomPcPackageIdScope> packageId,
            out CustomPcPackageReceipt package)
        {
            return _packages.TryGetValue(packageId, out package);
        }

        public bool TryGetPackageForQualityRelease(
            CustomPcQualityReleaseReceipt qualityRelease,
            out CustomPcPackageReceipt package)
        {
            package = null;
            return qualityRelease != null &&
                   _packagesByQualityRelease.TryGetValue(
                       qualityRelease.OperationId,
                       out package);
        }

        public bool TryGetCurrentCustody(
            CustomPcPackageReceipt package,
            out CustomPcPackageCustody custody)
        {
            custody = default;
            return OwnsPackage(package) &&
                   _currentCustody.TryGetValue(package.PackageId, out custody);
        }

        public bool TryGetCustodyReceipt(
            StableId<CustomPcPackageCustodyOperationIdScope> operationId,
            out CustomPcPackageCustodyReceipt receipt)
        {
            return _custodyByOperation.TryGetValue(operationId, out receipt);
        }

        public OperationResult ValidateReceiptHistory()
        {
            if (_quality == null ||
                _quality.ValidateReceiptHistory().IsFailure ||
                Revision != _history.Count ||
                _packages.Count != _packagesByOperation.Count ||
                _packages.Count != _packagesByQualityRelease.Count ||
                _custodyByOperation.Count != _history.Count - _packages.Count ||
                _currentCustody.Count != _packages.Count)
            {
                return OperationResult.Fail(
                    CustomPcPackageFailures.ReceiptHistoryInvalid);
            }

            var reconstructedCustody = new Dictionary<
                StableId<CustomPcPackageIdScope>,
                CustomPcPackageCustody>();
            for (int index = 0; index < _history.Count; index++)
            {
                long revision = index + 1L;
                object entry = _history[index];
                if (entry is CustomPcPackageReceipt package)
                {
                    if (!ValidateHistoricalPackage(package, revision).IsNone ||
                        reconstructedCustody.ContainsKey(package.PackageId))
                    {
                        return OperationResult.Fail(
                            CustomPcPackageFailures.ReceiptHistoryInvalid);
                    }

                    reconstructedCustody.Add(
                        package.PackageId,
                        CustomPcPackageCustody.PackagingWorkbench);
                    continue;
                }

                if (entry is not CustomPcPackageCustodyReceipt custody ||
                    !ValidateHistoricalCustody(
                        custody,
                        revision,
                        reconstructedCustody).IsNone)
                {
                    return OperationResult.Fail(
                        CustomPcPackageFailures.ReceiptHistoryInvalid);
                }

                reconstructedCustody[custody.PackageId] = custody.Target;
            }

            foreach (KeyValuePair<
                         StableId<CustomPcPackageIdScope>,
                         CustomPcPackageCustody> current in _currentCustody)
            {
                if (!reconstructedCustody.TryGetValue(
                        current.Key,
                        out CustomPcPackageCustody reconstructed) ||
                    reconstructed != current.Value)
                {
                    return OperationResult.Fail(
                        CustomPcPackageFailures.ReceiptHistoryInvalid);
                }
            }

            return OperationResult.Success();
        }

        private Failure ValidateCurrentQualityRelease(
            CustomPcQualityReleaseReceipt receipt)
        {
            Failure historical = ValidateHistoricalQualityRelease(receipt);
            if (!historical.IsNone)
            {
                return historical;
            }

            OperationResult<CustomPcQualityReleaseReceipt> current =
                _quality.EvaluateCurrentRelease();
            return current.IsSuccess && ReferenceEquals(current.Value, receipt)
                ? Failure.None
                : CustomPcPackageFailures.QualityReleaseNotCurrent;
        }

        private Failure ValidateHistoricalQualityRelease(
            CustomPcQualityReleaseReceipt receipt)
        {
            return receipt != null &&
                   receipt.Result ==
                       CustomPcQualityReleaseResult.ReadyForPackaging &&
                   _quality.TryGetReceipt(
                       receipt.OperationId,
                       out CustomPcQualityReleaseReceipt owned) &&
                   ReferenceEquals(owned, receipt)
                ? Failure.None
                : CustomPcPackageFailures.QualityReleaseInvalid;
        }

        private Failure ValidateHistoricalPackage(
            CustomPcPackageReceipt package,
            long expectedRevision)
        {
            if (package == null || !package.IsOwnedBy(this) ||
                package.PackageId.IsEmpty || package.OperationId.IsEmpty ||
                package.State != CustomPcPackageState.Sealed ||
                package.InitialCustody !=
                    CustomPcPackageCustody.PackagingWorkbench ||
                package.ExpectedRevision != expectedRevision - 1L ||
                package.Revision != expectedRevision ||
                !ValidateHistoricalQualityRelease(
                    package.SourceQualityReleaseReceipt).IsNone ||
                !_packages.TryGetValue(
                    package.PackageId,
                    out CustomPcPackageReceipt mappedPackage) ||
                !ReferenceEquals(mappedPackage, package) ||
                !_packagesByOperation.TryGetValue(
                    package.OperationId,
                    out CustomPcPackageReceipt mappedOperation) ||
                !ReferenceEquals(mappedOperation, package) ||
                !_packagesByQualityRelease.TryGetValue(
                    package.SourceQualityReleaseOperationId,
                    out CustomPcPackageReceipt mappedQuality) ||
                !ReferenceEquals(mappedQuality, package))
            {
                return CustomPcPackageFailures.ReceiptHistoryInvalid;
            }

            return Failure.None;
        }

        private Failure ValidateHistoricalCustody(
            CustomPcPackageCustodyReceipt custody,
            long expectedRevision,
            IReadOnlyDictionary<
                StableId<CustomPcPackageIdScope>,
                CustomPcPackageCustody> reconstructedCustody)
        {
            if (custody == null || !custody.IsOwnedBy(this) ||
                custody.OperationId.IsEmpty ||
                custody.ExpectedRevision != expectedRevision - 1L ||
                custody.Revision != expectedRevision ||
                !OwnsPackage(custody.Package) ||
                !_custodyByOperation.TryGetValue(
                    custody.OperationId,
                    out CustomPcPackageCustodyReceipt mapped) ||
                !ReferenceEquals(mapped, custody) ||
                !reconstructedCustody.TryGetValue(
                    custody.PackageId,
                    out CustomPcPackageCustody current) ||
                current != custody.Source ||
                !IsAllowedTransition(custody.Source, custody.Target))
            {
                return CustomPcPackageFailures.ReceiptHistoryInvalid;
            }

            return Failure.None;
        }

        private bool OwnsPackage(CustomPcPackageReceipt package)
        {
            return package != null && package.IsOwnedBy(this) &&
                   _packages.TryGetValue(
                       package.PackageId,
                       out CustomPcPackageReceipt owned) &&
                   ReferenceEquals(owned, package);
        }

        private static bool IsAllowedTransition(
            CustomPcPackageCustody source,
            CustomPcPackageCustody target)
        {
            return source switch
            {
                CustomPcPackageCustody.PackagingWorkbench =>
                    target == CustomPcPackageCustody.ActorHands,
                CustomPcPackageCustody.ActorHands =>
                    target == CustomPcPackageCustody.WorldFloor ||
                    target == CustomPcPackageCustody.TransportCart ||
                    target == CustomPcPackageCustody.DispatchStaging,
                CustomPcPackageCustody.WorldFloor =>
                    target == CustomPcPackageCustody.ActorHands,
                CustomPcPackageCustody.TransportCart =>
                    target == CustomPcPackageCustody.ActorHands,
                CustomPcPackageCustody.DispatchStaging =>
                    target == CustomPcPackageCustody.ActorHands,
                _ => false
            };
        }
    }
}
