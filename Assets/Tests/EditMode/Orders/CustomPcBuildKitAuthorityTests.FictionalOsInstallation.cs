using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Presentation.Interaction;

namespace PCShopEmpire3D.Tests.EditMode.Orders
{
    public sealed partial class CustomPcBuildKitAuthorityTests
    {
        [Test]
        public void FictionalOsInstallationBindsExactFirmwareAndStorageAndPersists()
        {
            GarageStockFlowSession session = PreparePowerBudgetReadySession();
            PcFirmwareBaselineReceipt firmware =
                CompleteCurrentFirmwareBaseline(
                    session,
                    out PcPowerStateReceipt powerOn);
            PcFictionalOsInstallationAuthority authority =
                session.FictionalOsInstallation;
            StableId<ItemInstanceIdScope> storageItemId =
                session.AssemblyBuild.StorageItemId;
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            long atx24Revision = session.AssemblyBuild.Atx24PowerCableRevision;
            long eps12vRevision = session.AssemblyBuild.Eps12vPowerCableRevision;
            long pcieRevision = session.AssemblyBuild.PcieGpuPowerCableRevision;
            StableId<PcFictionalOsInstallationOperationIdScope> operationId =
                session.CreatePrototypeFictionalOsInstallationOperationId(
                    firmware);

            OperationResult<PcFictionalOsInstallationReceipt> completed =
                authority.TryCompleteInstallation(
                    operationId,
                    firmware,
                    storageItemId,
                    session.PowerState.Revision,
                    authority.Revision);
            OperationResult<PcFictionalOsInstallationReceipt> replay =
                authority.TryCompleteInstallation(
                    operationId,
                    firmware,
                    storageItemId,
                    session.PowerState.Revision,
                    0);
            OperationResult<PcFictionalOsInstallationReceipt> installed =
                authority.EvaluateInstalledOperatingSystem();

            Assert.That(completed.IsSuccess, Is.True, completed.Error.Code);
            Assert.That(completed.Value.Profile,
                Is.EqualTo(PcFictionalOsProfile.WorkshopStandard));
            Assert.That(completed.Value.Result,
                Is.EqualTo(
                    PcFictionalOsInstallationResult.InstalledForDriverStage));
            Assert.That(completed.Value.SourceFirmwareBaselineReceipt,
                Is.SameAs(firmware));
            Assert.That(completed.Value.SourcePostStartupReceipt,
                Is.SameAs(firmware.SourcePostStartupReceipt));
            Assert.That(completed.Value.SourcePowerOnReceipt,
                Is.SameAs(powerOn));
            Assert.That(completed.Value.PreflightReceipt,
                Is.SameAs(powerOn.PreflightReceipt));
            Assert.That(completed.Value.StorageItemId, Is.EqualTo(storageItemId));
            Assert.That(completed.Value.StorageProductId,
                Is.EqualTo(session.AssemblyBuild.StorageProductId));
            Assert.That(completed.Value.SourceStorageSecureOperationId,
                Is.EqualTo(
                    session.AssemblyBuild.StorageSecuredByOperationId));
            Assert.That(completed.Value.SourceAssemblyRevision,
                Is.EqualTo(assemblyRevision));
            Assert.That(completed.Value.StorageItemId,
                Is.EqualTo(powerOn.PreflightReceipt.Context
                    .ElectricalReadiness.StorageItemId));
            Assert.That(completed.Value.StorageProductId,
                Is.EqualTo(powerOn.PreflightReceipt.Context
                    .PowerBudget.StorageProductId));
            Assert.That(completed.Value.SourceStorageSecureOperationId,
                Is.EqualTo(powerOn.PreflightReceipt.Context
                    .ElectricalReadiness.StorageSecureOperationId));
            Assert.That(completed.Value.SourceAssemblyRevision,
                Is.EqualTo(powerOn.PreflightReceipt.Context
                    .ElectricalReadiness.AssemblyRevision));
            Assert.That(session.AssemblyBuild.TryGetReceipt(
                    completed.Value.SourceStorageSecureOperationId,
                    out AssemblyOperationReceipt sourceStorageSecureReceipt),
                Is.True);
            Assert.That(sourceStorageSecureReceipt.OperationKind,
                Is.EqualTo(AssemblyOperationKind.SecureStorageDevice));
            Assert.That(sourceStorageSecureReceipt.ItemId,
                Is.EqualTo(completed.Value.StorageItemId));
            Assert.That(sourceStorageSecureReceipt.ProductId,
                Is.EqualTo(completed.Value.StorageProductId));
            Assert.That(sourceStorageSecureReceipt.AssemblyRevision,
                Is.GreaterThan(0).And.LessThanOrEqualTo(
                    completed.Value.SourceAssemblyRevision));
            Assert.That(completed.Value.SourceFirmwareBaselineRevision,
                Is.EqualTo(firmware.Revision));
            Assert.That(completed.Value.ExpectedPowerStateRevision,
                Is.EqualTo(session.PowerState.Revision));
            Assert.That(completed.Value.PowerStateRevision,
                Is.EqualTo(session.PowerState.Revision));
            Assert.That(completed.Value.ExpectedRevision, Is.Zero);
            Assert.That(completed.Value.Revision, Is.EqualTo(1));
            Assert.That(replay.Value, Is.SameAs(completed.Value));
            Assert.That(installed.Value, Is.SameAs(completed.Value));
            Assert.That(authority.Revision, Is.EqualTo(1));
            Assert.That(authority.ReceiptCount, Is.EqualTo(1));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(session.AssemblyBuild.Atx24PowerCableRevision,
                Is.EqualTo(atx24Revision));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableRevision,
                Is.EqualTo(eps12vRevision));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableRevision,
                Is.EqualTo(pcieRevision));
            Assert.That(session.AssemblyBuild.EvaluateBenchmarkReadiness().IsSuccess,
                Is.True);

            session.PowerState.TryPowerOff(
                session.PrototypePowerOffOperationId,
                powerOn,
                session.PowerState.Revision);

            Assert.That(session.PowerState.State, Is.EqualTo(PcPowerState.Off));
            Assert.That(authority.EvaluateInstalledOperatingSystem().Value,
                Is.SameAs(completed.Value));
            Assert.That(authority.TryGetInstalledReceipt(
                    storageItemId,
                    out PcFictionalOsInstallationReceipt historical),
                Is.True);
            Assert.That(historical, Is.SameAs(completed.Value));
            Assert.That(authority.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void FictionalOsInstallationRejectsForeignStaleConflictAndDuplicate()
        {
            GarageStockFlowSession session = PreparePowerBudgetReadySession();
            PcFirmwareBaselineReceipt firmware =
                CompleteCurrentFirmwareBaseline(
                    session,
                    out PcPowerStateReceipt powerOn);
            PcFictionalOsInstallationAuthority authority =
                session.FictionalOsInstallation;
            GarageStockFlowSession foreign = PreparePowerBudgetReadySession();
            PcFirmwareBaselineReceipt foreignFirmware =
                CompleteCurrentFirmwareBaseline(
                    foreign,
                    out PcPowerStateReceipt _);
            StableId<ItemInstanceIdScope> storageItemId =
                session.AssemblyBuild.StorageItemId;
            StableId<PcFictionalOsInstallationOperationIdScope> operationId =
                StableId<PcFictionalOsInstallationOperationIdScope>.Parse(
                    "assembly.fictional-os.issue131.primary");

            Assert.That(PcFictionalOsInstallationAuthority.Create(
                    session.PowerState,
                    foreign.AssemblyBuild).Error,
                Is.EqualTo(PcFictionalOsInstallationFailures.AuthorityMismatch));
            Assert.That(authority.TryCompleteInstallation(
                    default,
                    null,
                    default,
                    0,
                    0).Error,
                Is.EqualTo(
                    PcFictionalOsInstallationFailures.InvalidOperationId));
            Assert.That(authority.TryCompleteInstallation(
                    operationId,
                    firmware,
                    default,
                    session.PowerState.Revision,
                    0).Error,
                Is.EqualTo(
                    PcFictionalOsInstallationFailures.InvalidStorageItem));
            Assert.That(authority.TryCompleteInstallation(
                    operationId,
                    foreignFirmware,
                    storageItemId,
                    session.PowerState.Revision,
                    0).Error,
                Is.EqualTo(PcFictionalOsInstallationFailures
                    .InvalidFirmwareBaselineReceipt));
            Assert.That(authority.TryCompleteInstallation(
                    operationId,
                    firmware,
                    storageItemId,
                    session.PowerState.Revision + 1,
                    0).Error,
                Is.EqualTo(PcFictionalOsInstallationFailures
                    .PowerStateRevisionMismatch));
            Assert.That(authority.TryCompleteInstallation(
                    operationId,
                    firmware,
                    storageItemId,
                    session.PowerState.Revision,
                    1).Error,
                Is.EqualTo(PcFictionalOsInstallationFailures.RevisionMismatch));
            Assert.That(authority.TryCompleteInstallation(
                    operationId,
                    firmware,
                    StableId<ItemInstanceIdScope>.Parse(
                        "inventory.item.issue131.foreign-storage"),
                    session.PowerState.Revision,
                    0).Error,
                Is.EqualTo(PcFictionalOsInstallationFailures.StorageNotReady));

            PcFictionalOsInstallationReceipt completed =
                authority.TryCompleteInstallation(
                    operationId,
                    firmware,
                    storageItemId,
                    session.PowerState.Revision,
                    0).Value;

            Assert.That(authority.TryCompleteInstallation(
                    operationId,
                    firmware,
                    storageItemId,
                    session.PowerState.Revision,
                    1).Error,
                Is.EqualTo(PcFictionalOsInstallationFailures.OperationConflict));
            Assert.That(authority.TryCompleteInstallation(
                    StableId<PcFictionalOsInstallationOperationIdScope>.Parse(
                        "assembly.fictional-os.issue131.duplicate"),
                    firmware,
                    storageItemId,
                    session.PowerState.Revision,
                    1).Error,
                Is.EqualTo(PcFictionalOsInstallationFailures.AlreadyCompleted));

            session.PowerState.TryPowerOff(
                session.PrototypePowerOffOperationId,
                powerOn,
                session.PowerState.Revision);

            Assert.That(authority.TryCompleteInstallation(
                    operationId,
                    firmware,
                    storageItemId,
                    1,
                    0).Value,
                Is.SameAs(completed));
            Assert.That(authority.TryCompleteInstallation(
                    StableId<PcFictionalOsInstallationOperationIdScope>.Parse(
                        "assembly.fictional-os.issue131.off-cycle"),
                    firmware,
                    storageItemId,
                    session.PowerState.Revision,
                    authority.Revision).Error,
                Is.EqualTo(PcFictionalOsInstallationFailures.NotCurrent));
            Assert.That(authority.EvaluateInstalledOperatingSystem().Value,
                Is.SameAs(completed));
            Assert.That(authority.Revision, Is.EqualTo(1));
            Assert.That(authority.ReceiptCount, Is.EqualTo(1));
            Assert.That(authority.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void FictionalOsInstallationRequiresCurrentSecuredStorageIdentity()
        {
            GarageStockFlowSession session = PreparePowerBudgetReadySession();
            PcFirmwareBaselineReceipt firmware =
                CompleteCurrentFirmwareBaseline(
                    session,
                    out PcPowerStateReceipt powerOn);
            PcFictionalOsInstallationAuthority authority =
                session.FictionalOsInstallation;
            StableId<ItemInstanceIdScope> storageItemId =
                session.AssemblyBuild.StorageItemId;
            StableId<AssemblyOperationIdScope> originalSeatOperationId =
                session.AssemblyBuild.StorageSeatedByOperationId;
            StableId<AssemblyOperationIdScope> originalSecureOperationId =
                session.AssemblyBuild.StorageSecuredByOperationId;

            PcFictionalOsInstallationReceipt receipt =
                authority.TryCompleteInstallation(
                    session.CreatePrototypeFictionalOsInstallationOperationId(
                        firmware),
                    firmware,
                    storageItemId,
                    session.PowerState.Revision,
                    authority.Revision).Value;
            session.PowerState.TryPowerOff(
                session.PrototypePowerOffOperationId,
                powerOn,
                session.PowerState.Revision);

            StableId<AssemblyOperationIdScope> unsecureOperationId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue131.unsecure-storage");
            Assert.That(session.UnsecureStorageDevice(
                    unsecureOperationId,
                    originalSeatOperationId,
                    originalSecureOperationId,
                    session.AssemblyBuild.Revision).IsSuccess,
                Is.True);
            StableId<AssemblyOperationIdScope> removeOperationId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue131.remove-storage");
            Assert.That(session.RemoveStorageDevice(
                    removeOperationId,
                    originalSeatOperationId,
                    session.AssemblyBuild.Revision).IsSuccess,
                Is.True);

            Assert.That(receipt.StorageItemId, Is.EqualTo(storageItemId));
            Assert.That(authority.EvaluateInstalledOperatingSystem().Error,
                Is.EqualTo(PcFictionalOsInstallationFailures.NotCurrent));
            Assert.That(authority.TryGetReceipt(
                    receipt.OperationId,
                    out PcFictionalOsInstallationReceipt known),
                Is.True);
            Assert.That(known, Is.SameAs(receipt));

            StableId<AssemblyOperationIdScope> reseatOperationId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue131.reseat-storage");
            Assert.That(session.SeatStorageDevice(
                    reseatOperationId,
                    M2KeyOrientation.KeyAligned,
                    session.AssemblyBuild.InstalledByOperationId,
                    session.AssemblyBuild.SecuredByOperationId,
                    session.AssemblyBuild.Revision).IsSuccess,
                Is.True);
            StableId<AssemblyOperationIdScope> resecureOperationId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue131.resecure-storage");
            Assert.That(session.SecureStorageDevice(
                    resecureOperationId,
                    reseatOperationId,
                    session.AssemblyBuild.Revision).IsSuccess,
                Is.True);

            Assert.That(session.AssemblyBuild.StorageItemId,
                Is.EqualTo(storageItemId));
            Assert.That(authority.EvaluateInstalledOperatingSystem().Value,
                Is.SameAs(receipt));
            Assert.That(authority.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void FictionalOsInstallationHistoryRejectsStorageLineageCorruption()
        {
            GarageStockFlowSession session = PreparePowerBudgetReadySession();
            PcFirmwareBaselineReceipt firmware =
                CompleteCurrentFirmwareBaseline(
                    session,
                    out PcPowerStateReceipt _);
            PcFictionalOsInstallationAuthority authority =
                session.FictionalOsInstallation;
            PcFictionalOsInstallationReceipt receipt = authority
                .TryCompleteInstallation(
                    session.CreatePrototypeFictionalOsInstallationOperationId(
                        firmware),
                    firmware,
                    session.AssemblyBuild.StorageItemId,
                    session.PowerState.Revision,
                    authority.Revision).Value;

            AssertFictionalOsHistoryRejectsTamper(
                authority,
                receipt,
                "<StorageItemId>k__BackingField",
                default(StableId<ItemInstanceIdScope>));
            AssertFictionalOsHistoryRejectsTamper(
                authority,
                receipt,
                "<StorageProductId>k__BackingField",
                System.Activator.CreateInstance(
                    receipt.StorageProductId.GetType()));
            AssertFictionalOsHistoryRejectsTamper(
                authority,
                receipt,
                "<SourceStorageSecureOperationId>k__BackingField",
                default(StableId<AssemblyOperationIdScope>));
            AssertFictionalOsHistoryRejectsTamper(
                authority,
                receipt,
                "<SourceAssemblyRevision>k__BackingField",
                receipt.SourceAssemblyRevision + 1L);

            Assert.That(authority.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void AssertFictionalOsHistoryRejectsTamper(
            PcFictionalOsInstallationAuthority authority,
            PcFictionalOsInstallationReceipt receipt,
            string fieldName,
            object corruptedValue)
        {
            System.Reflection.FieldInfo field = receipt.GetType().GetField(
                fieldName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, fieldName);
            object originalValue = field.GetValue(receipt);
            field.SetValue(receipt, corruptedValue);
            Assert.That(authority.ValidateReceiptHistory().Error,
                Is.EqualTo(
                    PcFictionalOsInstallationFailures.ReceiptHistoryInvalid),
                fieldName);
            field.SetValue(receipt, originalValue);
            Assert.That(authority.ValidateReceiptHistory().IsSuccess,
                Is.True,
                fieldName);
        }

        private static PcFirmwareBaselineReceipt CompleteCurrentFirmwareBaseline(
            GarageStockFlowSession session,
            out PcPowerStateReceipt powerOn)
        {
            PowerTestAttemptReceipt preflight = CompletePowerPreflight(session);
            PcPowerStateAuthority powerState = session.PowerState;
            powerOn = powerState.TryPowerOn(
                session.PrototypePowerOnOperationId,
                preflight,
                powerState.Revision).Value;
            PcPostStartupReceipt postStartup = powerState
                .TryCompleteStartupSelfTest(
                    session.CreatePrototypePostStartupOperationId(powerOn),
                    powerOn,
                    powerState.Revision).Value;
            return powerState.TrySaveFirmwareBaseline(
                session.CreatePrototypeFirmwareBaselineOperationId(postStartup),
                postStartup,
                powerState.Revision,
                powerState.FirmwareBaselineRevision).Value;
        }
    }
}
