using System.Collections;
using System.Reflection;
using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Fulfillment;
using PCShopEmpire3D.Quality;

namespace PCShopEmpire3D.Tests.EditMode.Orders
{
    public sealed partial class CustomPcBuildKitAuthorityTests
    {
        [Test]
        public void CustomPcPackageSealsExactCurrentQualityOnceWithoutUpstreamMutation()
        {
            QualityFixture fixture = PrepareQualityFixture();
            CustomPcQualityReleaseReceipt quality = Release(
                fixture,
                "issue139-package-exact");
            CustomPcPackageAuthority packages =
                CustomPcPackageAuthority.Create(fixture.Authority).Value;
            long inventoryRevision = fixture.Session.Inventory.Revision;
            long workOrderRevision = fixture.Session.CustomPcWorkOrders.Revision;
            long assemblyRevision = fixture.Session.AssemblyBuild.Revision;
            long qualityRevision = fixture.Authority.Revision;
            StableId<CustomPcPackageIdScope> packageId = PackageId("exact");
            StableId<CustomPcPackageSealOperationIdScope> operationId =
                PackageSealOperationId("exact");

            OperationResult<CustomPcPackageReceipt> sealedPackage =
                packages.TrySealPackage(
                    packageId,
                    operationId,
                    quality,
                    packages.Revision);
            OperationResult<CustomPcPackageReceipt> replay =
                packages.TrySealPackage(
                    packageId,
                    operationId,
                    quality,
                    0);
            OperationResult<CustomPcPackageReceipt> conflict =
                packages.TrySealPackage(
                    PackageId("conflict"),
                    operationId,
                    quality,
                    packages.Revision);

            Assert.That(sealedPackage.IsSuccess, Is.True,
                sealedPackage.Error.Code);
            Assert.That(replay.Value, Is.SameAs(sealedPackage.Value));
            Assert.That(conflict.Error,
                Is.EqualTo(CustomPcPackageFailures.OperationConflict));
            Assert.That(sealedPackage.Value.PackageId, Is.EqualTo(packageId));
            Assert.That(sealedPackage.Value.SourceQualityReleaseReceipt,
                Is.SameAs(quality));
            Assert.That(sealedPackage.Value.WorkOrderId,
                Is.EqualTo(quality.WorkOrderId));
            Assert.That(sealedPackage.Value.WorkTicketId,
                Is.EqualTo(quality.WorkTicketId));
            Assert.That(sealedPackage.Value.CustomerBindingId,
                Is.EqualTo(quality.CustomerBindingId));
            Assert.That(sealedPackage.Value.InventoryClaimId,
                Is.EqualTo(quality.InventoryClaimId));
            Assert.That(sealedPackage.Value.BuildId,
                Is.EqualTo(quality.BuildId));
            Assert.That(sealedPackage.Value.ChassisId,
                Is.EqualTo(quality.ChassisId));
            Assert.That(sealedPackage.Value.State,
                Is.EqualTo(CustomPcPackageState.Sealed));
            Assert.That(packages.TryGetCurrentCustody(
                sealedPackage.Value,
                out CustomPcPackageCustody custody), Is.True);
            Assert.That(custody,
                Is.EqualTo(CustomPcPackageCustody.PackagingWorkbench));
            Assert.That(packages.PackageCount, Is.EqualTo(1));
            Assert.That(packages.CustodyReceiptCount, Is.Zero);
            Assert.That(packages.Revision, Is.EqualTo(1));
            Assert.That(fixture.Session.Inventory.Revision,
                Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Session.CustomPcWorkOrders.Revision,
                Is.EqualTo(workOrderRevision));
            Assert.That(fixture.Session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevision));
            Assert.That(fixture.Authority.Revision,
                Is.EqualTo(qualityRevision));
            Assert.That(packages.ValidateReceiptHistory().IsSuccess, Is.True);
        }

        [Test]
        public void CustomPcPackageCustodyIsAppendOnlyReplaySafeAndTransitionBound()
        {
            QualityFixture fixture = PrepareQualityFixture();
            CustomPcQualityReleaseReceipt quality = Release(
                fixture,
                "issue139-custody");
            CustomPcPackageAuthority packages =
                CustomPcPackageAuthority.Create(fixture.Authority).Value;
            CustomPcPackageReceipt package = packages.TrySealPackage(
                PackageId("custody"),
                PackageSealOperationId("custody"),
                quality,
                packages.Revision).Value;

            Assert.That(packages.ValidateCustodyTransfer(
                package,
                CustomPcPackageCustody.PackagingWorkbench,
                CustomPcPackageCustody.WorldFloor,
                packages.Revision).Error,
                Is.EqualTo(CustomPcPackageFailures.CustodyTransitionInvalid));

            CustomPcPackageCustodyReceipt pickup = Transfer(
                packages,
                package,
                CustomPcPackageCustody.PackagingWorkbench,
                CustomPcPackageCustody.ActorHands,
                "pickup-workbench");
            Transfer(packages, package,
                CustomPcPackageCustody.ActorHands,
                CustomPcPackageCustody.WorldFloor,
                "drop-floor");
            Transfer(packages, package,
                CustomPcPackageCustody.WorldFloor,
                CustomPcPackageCustody.ActorHands,
                "pickup-floor");
            Transfer(packages, package,
                CustomPcPackageCustody.ActorHands,
                CustomPcPackageCustody.TransportCart,
                "load-cart");
            Transfer(packages, package,
                CustomPcPackageCustody.TransportCart,
                CustomPcPackageCustody.ActorHands,
                "unload-cart");
            Transfer(packages, package,
                CustomPcPackageCustody.ActorHands,
                CustomPcPackageCustody.DispatchStaging,
                "stage-dispatch");

            OperationResult<CustomPcPackageCustodyReceipt> replay =
                packages.TryTransferCustody(
                    pickup.OperationId,
                    package,
                    CustomPcPackageCustody.PackagingWorkbench,
                    CustomPcPackageCustody.ActorHands,
                    pickup.ExpectedRevision);
            OperationResult<CustomPcPackageCustodyReceipt> conflict =
                packages.TryTransferCustody(
                    pickup.OperationId,
                    package,
                    CustomPcPackageCustody.ActorHands,
                    CustomPcPackageCustody.WorldFloor,
                    packages.Revision);

            Assert.That(replay.Value, Is.SameAs(pickup));
            Assert.That(conflict.Error,
                Is.EqualTo(CustomPcPackageFailures.OperationConflict));
            Assert.That(packages.TryGetCurrentCustody(
                package,
                out CustomPcPackageCustody custody), Is.True);
            Assert.That(custody,
                Is.EqualTo(CustomPcPackageCustody.DispatchStaging));
            Assert.That(packages.PackageCount, Is.EqualTo(1));
            Assert.That(packages.CustodyReceiptCount, Is.EqualTo(6));
            Assert.That(packages.Revision, Is.EqualTo(7));
            Assert.That(packages.ValidateReceiptHistory().IsSuccess, Is.True);
        }

        [Test]
        public void CustomPcPackageRejectsForeignDuplicateAndStaleSealButCustodyStaysRecoverable()
        {
            QualityFixture fixture = PrepareQualityFixture();
            QualityFixture foreign = PrepareQualityFixture();
            CustomPcQualityReleaseReceipt quality = Release(
                fixture,
                "issue139-stale");
            CustomPcQualityReleaseReceipt foreignQuality = Release(
                foreign,
                "issue139-foreign");
            CustomPcPackageAuthority packages =
                CustomPcPackageAuthority.Create(fixture.Authority).Value;
            CustomPcPackageReceipt package = packages.TrySealPackage(
                PackageId("stale"),
                PackageSealOperationId("stale"),
                quality,
                0).Value;

            Assert.That(packages.TrySealPackage(
                    PackageId("duplicate"),
                    PackageSealOperationId("duplicate"),
                    quality,
                    packages.Revision).Error,
                Is.EqualTo(CustomPcPackageFailures.PackageAlreadyExists));
            Assert.That(packages.TrySealPackage(
                    PackageId("foreign"),
                    PackageSealOperationId("foreign"),
                    foreignQuality,
                    packages.Revision).Error,
                Is.EqualTo(CustomPcPackageFailures.QualityReleaseInvalid));

            PcPowerStateReceipt nextPowerOn = fixture.Session.PowerState.TryPowerOn(
                fixture.Session.CreatePrototypePowerStateOperationId(
                    PcPowerTransitionKind.PowerOn,
                    fixture.Session.PowerState.Revision + 1L),
                fixture.ValidationReceipt.PreflightReceipt,
                fixture.Session.PowerState.Revision).Value;

            Assert.That(nextPowerOn.ResultingState,
                Is.EqualTo(PcPowerState.Energized));
            Assert.That(packages.EvaluateCurrentPackage(package.PackageId).Error,
                Is.EqualTo(CustomPcPackageFailures.NotCurrent));
            CustomPcPackageCustodyReceipt recoveryPickup = Transfer(
                packages,
                package,
                CustomPcPackageCustody.PackagingWorkbench,
                CustomPcPackageCustody.ActorHands,
                "stale-recovery-pickup");
            Assert.That(recoveryPickup.Target,
                Is.EqualTo(CustomPcPackageCustody.ActorHands));
            Assert.That(packages.ValidateReceiptHistory().IsSuccess, Is.True);
        }

        [Test]
        public void CustomPcPackageHistoryTamperFailsClosedBeforeReplay()
        {
            QualityFixture fixture = PrepareQualityFixture();
            CustomPcQualityReleaseReceipt quality = Release(
                fixture,
                "issue139-history");
            CustomPcPackageAuthority packages =
                CustomPcPackageAuthority.Create(fixture.Authority).Value;
            CustomPcPackageReceipt package = packages.TrySealPackage(
                PackageId("history"),
                PackageSealOperationId("history"),
                quality,
                0).Value;
            CustomPcPackageCustodyReceipt pickup = Transfer(
                packages,
                package,
                CustomPcPackageCustody.PackagingWorkbench,
                CustomPcPackageCustody.ActorHands,
                "history-pickup");
            FieldInfo historyField = typeof(CustomPcPackageAuthority).GetField(
                "_history",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(historyField, Is.Not.Null);
            var history = (IList)historyField.GetValue(packages);
            object removed = history[history.Count - 1];
            history.RemoveAt(history.Count - 1);

            Assert.That(packages.ValidateReceiptHistory().Error,
                Is.EqualTo(CustomPcPackageFailures.ReceiptHistoryInvalid));
            Assert.That(packages.TryTransferCustody(
                    pickup.OperationId,
                    package,
                    pickup.Source,
                    pickup.Target,
                    pickup.ExpectedRevision).Error,
                Is.EqualTo(CustomPcPackageFailures.ReceiptHistoryInvalid));

            history.Add(removed);
            Assert.That(packages.ValidateReceiptHistory().IsSuccess, Is.True);
        }

        private static CustomPcPackageCustodyReceipt Transfer(
            CustomPcPackageAuthority packages,
            CustomPcPackageReceipt package,
            CustomPcPackageCustody source,
            CustomPcPackageCustody target,
            string suffix)
        {
            long revision = packages.Revision;
            OperationResult preflight = packages.ValidateCustodyTransfer(
                package,
                source,
                target,
                revision);
            Assert.That(preflight.IsSuccess, Is.True, preflight.Error.Code);
            OperationResult<CustomPcPackageCustodyReceipt> result =
                packages.TryTransferCustody(
                    PackageCustodyOperationId(suffix),
                    package,
                    source,
                    target,
                    revision);
            Assert.That(result.IsSuccess, Is.True, result.Error.Code);
            return result.Value;
        }

        private static StableId<CustomPcPackageIdScope> PackageId(string suffix)
        {
            return StableId<CustomPcPackageIdScope>.Parse(
                "fulfillment.custom-pc-package.issue139." + suffix);
        }

        private static StableId<CustomPcPackageSealOperationIdScope>
            PackageSealOperationId(string suffix)
        {
            return StableId<CustomPcPackageSealOperationIdScope>.Parse(
                "fulfillment.custom-pc-package-seal.issue139." + suffix);
        }

        private static StableId<CustomPcPackageCustodyOperationIdScope>
            PackageCustodyOperationId(string suffix)
        {
            return StableId<CustomPcPackageCustodyOperationIdScope>.Parse(
                "fulfillment.custom-pc-package-custody.issue139." + suffix);
        }
    }
}
