using System;
using System.Collections.Generic;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Fulfillment;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public static class CustomPcPackagePhysicalFailures
    {
        public static readonly Failure ConfigurationMissing = Failure.FromCode(
            "fulfillment.custom-pc-package-physical.configuration-missing");
        public static readonly Failure SourceProjectionInvalid = Failure.FromCode(
            "fulfillment.custom-pc-package-physical.source-projection-invalid");
        public static readonly Failure PackageNotSealed = Failure.FromCode(
            "fulfillment.custom-pc-package-physical.package-not-sealed");
        public static readonly Failure PackageReceiptInvalid = Failure.FromCode(
            "fulfillment.custom-pc-package-physical.package-receipt-invalid");
        public static readonly Failure PhysicalStateMismatch = Failure.FromCode(
            "fulfillment.custom-pc-package-physical.state-mismatch");
        public static readonly Failure PhysicalRollbackFailed = Failure.FromCode(
            "fulfillment.custom-pc-package-physical.rollback-failed");
    }

    /// <summary>
    /// Binds the one immutable package receipt to one LargeBox projection. The ten
    /// assembled source projections are hidden only after the authoritative seal
    /// exists, preventing a second physical representation of the same customer PC.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PhysicalItemProjection))]
    public sealed class CustomPcPackagePhysicalBinding : MonoBehaviour
    {
        public const int RequiredSourceProjectionCount = 10;

        [SerializeField] private GarageStockFlowRuntime stockFlow;
        [SerializeField] private PhysicalItemProjection packageItem;
        [SerializeField] private Transform packagingAnchor;
        [SerializeField] private Transform dispatchAnchor;
        [SerializeField] private TextMesh packageLabel;
        [SerializeField] private PhysicalItemProjection[] sourceProjections =
            Array.Empty<PhysicalItemProjection>();

        private CustomPcPackageReceipt _packageReceipt;

        public GarageStockFlowRuntime StockFlow => stockFlow;

        public PhysicalItemProjection PackageItem => packageItem;

        public Transform PackagingAnchor => packagingAnchor;

        public Transform DispatchAnchor => dispatchAnchor;

        public TextMesh PackageLabel => packageLabel;

        public IReadOnlyList<PhysicalItemProjection> SourceProjections =>
            sourceProjections;

        public CustomPcPackageReceipt PackageReceipt => _packageReceipt;

        public bool IsSealedProjection => _packageReceipt != null &&
                                          packageItem != null &&
                                          packageItem.gameObject.activeSelf;

        public Pose DispatchPose => dispatchAnchor != null
            ? new Pose(dispatchAnchor.position, dispatchAnchor.rotation)
            : default;

        public void Configure(
            GarageStockFlowRuntime garageStockFlow,
            PhysicalItemProjection physicalPackage,
            Transform packageWorkbenchAnchor,
            Transform packageDispatchAnchor,
            TextMesh identityLabel,
            PhysicalItemProjection[] assembledSourceProjections)
        {
            stockFlow = garageStockFlow != null
                ? garageStockFlow
                : throw new ArgumentNullException(nameof(garageStockFlow));
            packageItem = physicalPackage != null
                ? physicalPackage
                : throw new ArgumentNullException(nameof(physicalPackage));
            packagingAnchor = packageWorkbenchAnchor != null
                ? packageWorkbenchAnchor
                : throw new ArgumentNullException(nameof(packageWorkbenchAnchor));
            dispatchAnchor = packageDispatchAnchor != null
                ? packageDispatchAnchor
                : throw new ArgumentNullException(nameof(packageDispatchAnchor));
            packageLabel = identityLabel != null
                ? identityLabel
                : throw new ArgumentNullException(nameof(identityLabel));
            sourceProjections = assembledSourceProjections != null
                ? (PhysicalItemProjection[])assembledSourceProjections.Clone()
                : throw new ArgumentNullException(
                    nameof(assembledSourceProjections));
            OperationResult contract = ValidateContract();
            if (contract.IsFailure)
            {
                throw new ArgumentException(contract.Error.Code);
            }

            _packageReceipt = null;
            RefreshLabel();
        }

        public OperationResult ValidateContract()
        {
            if (stockFlow == null || packageItem == null ||
                packagingAnchor == null || dispatchAnchor == null ||
                packageLabel == null || packageItem.gameObject != gameObject)
            {
                return OperationResult.Fail(
                    CustomPcPackagePhysicalFailures.ConfigurationMissing);
            }

            if (sourceProjections == null ||
                sourceProjections.Length != RequiredSourceProjectionCount)
            {
                return OperationResult.Fail(
                    CustomPcPackagePhysicalFailures.SourceProjectionInvalid);
            }

            var unique = new HashSet<PhysicalItemProjection>();
            foreach (PhysicalItemProjection source in sourceProjections)
            {
                if (source == null || source == packageItem || !unique.Add(source))
                {
                    return OperationResult.Fail(
                        CustomPcPackagePhysicalFailures.SourceProjectionInvalid);
                }
            }

            return OperationResult.Success();
        }

        public OperationResult ValidateSealProjection()
        {
            OperationResult contract = ValidateContract();
            if (contract.IsFailure)
            {
                return contract;
            }

            GarageStockFlowSession session = ResolveSession();
            if (session == null || session.TryGetPrototypeCustomPcPackage(out _))
            {
                return OperationResult.Fail(
                    CustomPcPackagePhysicalFailures.PhysicalStateMismatch);
            }

            if (packageItem.CarryProfile != PhysicalCarryProfile.LargeBox ||
                packageItem.ItemIdValue !=
                    GarageStockFlowSession.PrototypeCustomPcPackageIdValue ||
                packageItem.Ownership != PhysicalItemOwnership.World)
            {
                return OperationResult.Fail(
                    CustomPcPackagePhysicalFailures.PhysicalStateMismatch);
            }

            foreach (PhysicalItemProjection source in sourceProjections)
            {
                if (!source.gameObject.activeSelf)
                {
                    return OperationResult.Fail(
                        CustomPcPackagePhysicalFailures.SourceProjectionInvalid);
                }
            }

            return OperationResult.Success();
        }

        public OperationResult ActivateSealedPackage(
            CustomPcPackageReceipt receipt)
        {
            OperationResult contract = ValidateContract();
            if (contract.IsFailure)
            {
                return contract;
            }

            GarageStockFlowSession session = ResolveSession();
            if (session == null || receipt == null ||
                receipt.PackageId != session.PrototypeCustomPcPackageId ||
                !session.TryGetCustomPcPackageAuthority(
                    out CustomPcPackageAuthority authority) ||
                !authority.TryGetPackage(receipt.PackageId, out var owned) ||
                !ReferenceEquals(owned, receipt))
            {
                return OperationResult.Fail(
                    CustomPcPackagePhysicalFailures.PackageReceiptInvalid);
            }

            _packageReceipt = receipt;
            transform.SetPositionAndRotation(
                packagingAnchor.position,
                packagingAnchor.rotation);
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            OperationResult stable = packageItem.SynchronizeStableWorldPose(
                new Pose(packagingAnchor.position, packagingAnchor.rotation));
            if (stable.IsFailure)
            {
                return stable;
            }

            foreach (PhysicalItemProjection source in sourceProjections)
            {
                source.gameObject.SetActive(false);
            }

            RefreshLabel();
            Physics.SyncTransforms();
            return OperationResult.Success();
        }

        public OperationResult SyncProjectionToAuthority()
        {
            GarageStockFlowSession session = ResolveSession();
            if (session == null ||
                !session.TryGetPrototypeCustomPcPackage(
                    out CustomPcPackageReceipt package))
            {
                return OperationResult.Fail(
                    CustomPcPackagePhysicalFailures.PackageNotSealed);
            }

            if (_packageReceipt == null || !gameObject.activeSelf)
            {
                return ActivateSealedPackage(package);
            }

            if (!ReferenceEquals(_packageReceipt, package))
            {
                return OperationResult.Fail(
                    CustomPcPackagePhysicalFailures.PackageReceiptInvalid);
            }

            RefreshLabel();
            return OperationResult.Success();
        }

        public OperationResult PrepareCustodyTransition(
            CustomPcPackageCustody target,
            out CustomPcPackageCustody source,
            out long expectedRevision)
        {
            source = default;
            expectedRevision = -1L;
            OperationResult sync = SyncProjectionToAuthority();
            if (sync.IsFailure)
            {
                return sync;
            }

            GarageStockFlowSession session = ResolveSession();
            if (session == null ||
                !session.TryGetCustomPcPackageAuthority(
                    out CustomPcPackageAuthority authority) ||
                !authority.TryGetCurrentCustody(_packageReceipt, out source))
            {
                return OperationResult.Fail(
                    CustomPcPackagePhysicalFailures.PackageReceiptInvalid);
            }

            expectedRevision = authority.Revision;
            return authority.ValidateCustodyTransfer(
                _packageReceipt,
                source,
                target,
                expectedRevision);
        }

        public OperationResult CommitCustodyTransition(
            CustomPcPackageCustody source,
            CustomPcPackageCustody target,
            long expectedRevision)
        {
            GarageStockFlowSession session = ResolveSession();
            if (session == null || _packageReceipt == null ||
                !session.TryGetCustomPcPackageAuthority(
                    out CustomPcPackageAuthority authority))
            {
                return OperationResult.Fail(
                    CustomPcPackagePhysicalFailures.PackageReceiptInvalid);
            }

            OperationResult<CustomPcPackageCustodyReceipt> result =
                authority.TryTransferCustody(
                    session.CreatePrototypeCustomPcPackageCustodyOperationId(
                        source,
                        target,
                        expectedRevision),
                    _packageReceipt,
                    source,
                    target,
                    expectedRevision);
            RefreshLabel();
            return result.IsSuccess
                ? OperationResult.Success()
                : OperationResult.Fail(result.Error);
        }

        public bool TryGetCurrentCustody(out CustomPcPackageCustody custody)
        {
            custody = default;
            GarageStockFlowSession session = ResolveSession();
            return session != null && _packageReceipt != null &&
                   session.TryGetCustomPcPackageAuthority(
                       out CustomPcPackageAuthority authority) &&
                   authority.TryGetCurrentCustody(_packageReceipt, out custody);
        }

        private GarageStockFlowSession ResolveSession()
        {
            return stockFlow != null ? stockFlow.EnsureInitialized() : null;
        }

        private void RefreshLabel()
        {
            if (packageLabel == null)
            {
                return;
            }

            if (_packageReceipt == null)
            {
                packageLabel.text = "CUSTOM PC\nKALİTE MÜHRÜ BEKLENİYOR";
                return;
            }

            string custody = TryGetCurrentCustody(out var current)
                ? current.ToString().ToUpperInvariant()
                : "CUSTODY UNKNOWN";
            packageLabel.text =
                "CUSTOM PC • DEMO-GAMING-001\n" +
                $"SEALED • REV {_packageReceipt.Revision} • {custody}";
        }
    }
}
