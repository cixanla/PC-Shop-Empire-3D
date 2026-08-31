using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Presentation.Interaction;

namespace PCShopEmpire3D.Tests.EditMode.Orders
{
    public sealed partial class CustomPcBuildKitAuthorityTests
    {
        [Test]
        public void FictionalDriverBindsCurrentCycleOsAndHardwareAndPersists()
        {
            GarageStockFlowSession session = PreparePowerBudgetReadySession();
            PcFictionalOsInstallationReceipt operatingSystem =
                CompleteCurrentFictionalOsInstallation(
                    session,
                    out PcFirmwareBaselineReceipt firmware,
                    out PcPowerStateReceipt powerOn);
            PcFictionalDriverInstallationAuthority authority =
                session.FictionalDriverInstallation;
            StableId<ItemInstanceIdScope> storageItemId =
                session.AssemblyBuild.StorageItemId;
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            long atx24Revision =
                session.AssemblyBuild.Atx24PowerCableRevision;
            long eps12vRevision =
                session.AssemblyBuild.Eps12vPowerCableRevision;
            long pcieRevision =
                session.AssemblyBuild.PcieGpuPowerCableRevision;
            long powerRevision = session.PowerState.Revision;
            long postRevision = session.PowerState.PostStartupRevision;
            long firmwareRevision =
                session.PowerState.FirmwareBaselineRevision;
            long osRevision = session.FictionalOsInstallation.Revision;
            StableId<PcFictionalDriverInstallationOperationIdScope>
                operationId = session
                    .CreatePrototypeFictionalDriverInstallationOperationId(
                        operatingSystem);

            OperationResult<PcFictionalDriverInstallationReceipt> completed =
                authority.TryCompleteInstallation(
                    operationId,
                    operatingSystem,
                    firmware,
                    storageItemId,
                    powerRevision,
                    authority.Revision);
            OperationResult<PcFictionalDriverInstallationReceipt> replay =
                authority.TryCompleteInstallation(
                    operationId,
                    operatingSystem,
                    firmware,
                    storageItemId,
                    powerRevision,
                    0);
            OperationResult<PcFictionalDriverInstallationReceipt> installed =
                authority.EvaluateInstalledDrivers();

            Assert.That(completed.IsSuccess, Is.True, completed.Error.Code);
            Assert.That(completed.Value.Profile,
                Is.EqualTo(PcFictionalDriverProfile.WorkshopDriverBundle));
            Assert.That(completed.Value.Result,
                Is.EqualTo(PcFictionalDriverInstallationResult
                    .InstalledForBenchmarkStage));
            Assert.That(completed.Value.SourceOperatingSystemReceipt,
                Is.SameAs(operatingSystem));
            Assert.That(completed.Value.SourceFirmwareBaselineReceipt,
                Is.SameAs(firmware));
            Assert.That(completed.Value.SourcePostStartupReceipt,
                Is.SameAs(firmware.SourcePostStartupReceipt));
            Assert.That(completed.Value.SourcePowerOnReceipt,
                Is.SameAs(powerOn));
            Assert.That(completed.Value.PreflightReceipt,
                Is.SameAs(firmware.PreflightReceipt));
            Assert.That(completed.Value.OperatingSystemPreflightReceipt,
                Is.SameAs(operatingSystem.PreflightReceipt));
            Assert.That(completed.Value.StorageItemId,
                Is.EqualTo(storageItemId));
            Assert.That(completed.Value.StorageProductId,
                Is.EqualTo(session.AssemblyBuild.StorageProductId));
            Assert.That(completed.Value.InstallationStorageSecureOperationId,
                Is.EqualTo(
                    session.AssemblyBuild.StorageSecuredByOperationId));
            Assert.That(completed.Value.InstallationAssemblyRevision,
                Is.EqualTo(assemblyRevision));
            Assert.That(completed.Value.SourceOperatingSystemRevision,
                Is.EqualTo(operatingSystem.Revision));
            Assert.That(completed.Value.SourceFirmwareBaselineRevision,
                Is.EqualTo(firmware.Revision));
            Assert.That(completed.Value.ExpectedPowerStateRevision,
                Is.EqualTo(powerRevision));
            Assert.That(completed.Value.PowerStateRevision,
                Is.EqualTo(powerRevision));
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
            Assert.That(session.PowerState.Revision, Is.EqualTo(powerRevision));
            Assert.That(session.PowerState.PostStartupRevision,
                Is.EqualTo(postRevision));
            Assert.That(session.PowerState.FirmwareBaselineRevision,
                Is.EqualTo(firmwareRevision));
            Assert.That(session.FictionalOsInstallation.Revision,
                Is.EqualTo(osRevision));
            Assert.That(session.AssemblyBuild.EvaluateBenchmarkReadiness().Error,
                Is.EqualTo(AssemblyFailures.BuildIncomplete));

            Assert.That(session.PowerState.TryPowerOff(
                    session.PrototypePowerOffOperationId,
                    powerOn,
                    session.PowerState.Revision).IsSuccess,
                Is.True);
            Assert.That(authority.EvaluateInstalledDrivers().Value,
                Is.SameAs(completed.Value));

            StableId<AssemblyOperationIdScope> originalSeatOperationId =
                session.AssemblyBuild.StorageSeatedByOperationId;
            StableId<AssemblyOperationIdScope> originalSecureOperationId =
                session.AssemblyBuild.StorageSecuredByOperationId;
            Assert.That(session.UnsecureStorageDevice(
                    StableId<AssemblyOperationIdScope>.Parse(
                        "assembly.operation.issue133.unsecure-storage"),
                    originalSeatOperationId,
                    originalSecureOperationId,
                    session.AssemblyBuild.Revision).IsSuccess,
                Is.True);
            Assert.That(session.RemoveStorageDevice(
                    StableId<AssemblyOperationIdScope>.Parse(
                        "assembly.operation.issue133.remove-storage"),
                    originalSeatOperationId,
                    session.AssemblyBuild.Revision).IsSuccess,
                Is.True);
            Assert.That(authority.EvaluateInstalledDrivers().Error,
                Is.EqualTo(PcFictionalDriverInstallationFailures.NotCurrent));

            StableId<AssemblyOperationIdScope> reseatOperationId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue133.reseat-storage");
            Assert.That(session.SeatStorageDevice(
                    reseatOperationId,
                    M2KeyOrientation.KeyAligned,
                    session.AssemblyBuild.InstalledByOperationId,
                    session.AssemblyBuild.SecuredByOperationId,
                    session.AssemblyBuild.Revision).IsSuccess,
                Is.True);
            Assert.That(session.SecureStorageDevice(
                    StableId<AssemblyOperationIdScope>.Parse(
                        "assembly.operation.issue133.resecure-storage"),
                    reseatOperationId,
                    session.AssemblyBuild.Revision).IsSuccess,
                Is.True);

            Assert.That(authority.EvaluateInstalledDrivers().Value,
                Is.SameAs(completed.Value));
            Assert.That(authority.TryGetInstalledReceipt(
                    storageItemId,
                    out PcFictionalDriverInstallationReceipt historical),
                Is.True);
            Assert.That(historical, Is.SameAs(completed.Value));
            Assert.That(authority.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void FictionalDriverRejectsForeignStaleConflictAndDuplicate()
        {
            GarageStockFlowSession session = PreparePowerBudgetReadySession();
            PcFictionalOsInstallationReceipt operatingSystem =
                CompleteCurrentFictionalOsInstallation(
                    session,
                    out PcFirmwareBaselineReceipt firmware,
                    out PcPowerStateReceipt powerOn);
            PcFictionalDriverInstallationAuthority authority =
                session.FictionalDriverInstallation;
            GarageStockFlowSession foreign = PreparePowerBudgetReadySession();
            PcFictionalOsInstallationReceipt foreignOperatingSystem =
                CompleteCurrentFictionalOsInstallation(
                    foreign,
                    out PcFirmwareBaselineReceipt foreignFirmware,
                    out PcPowerStateReceipt _);
            StableId<ItemInstanceIdScope> storageItemId =
                session.AssemblyBuild.StorageItemId;
            StableId<PcFictionalDriverInstallationOperationIdScope>
                operationId = StableId<
                    PcFictionalDriverInstallationOperationIdScope>.Parse(
                    "assembly.fictional-driver.issue133.primary");

            Assert.That(PcFictionalDriverInstallationAuthority.Create(null).Error,
                Is.EqualTo(
                    PcFictionalDriverInstallationFailures.ConfigurationMissing));
            Assert.That(authority.TryCompleteInstallation(
                    default,
                    null,
                    null,
                    default,
                    0,
                    0).Error,
                Is.EqualTo(
                    PcFictionalDriverInstallationFailures.InvalidOperationId));
            Assert.That(authority.TryCompleteInstallation(
                    operationId,
                    operatingSystem,
                    firmware,
                    default,
                    session.PowerState.Revision,
                    0).Error,
                Is.EqualTo(
                    PcFictionalDriverInstallationFailures.InvalidStorageItem));
            Assert.That(authority.TryCompleteInstallation(
                    operationId,
                    operatingSystem,
                    firmware,
                    storageItemId,
                    session.PowerState.Revision + 1L,
                    0).Error,
                Is.EqualTo(PcFictionalDriverInstallationFailures
                    .PowerStateRevisionMismatch));
            Assert.That(authority.TryCompleteInstallation(
                    operationId,
                    operatingSystem,
                    firmware,
                    storageItemId,
                    session.PowerState.Revision,
                    1).Error,
                Is.EqualTo(
                    PcFictionalDriverInstallationFailures.RevisionMismatch));
            Assert.That(authority.TryCompleteInstallation(
                    operationId,
                    foreignOperatingSystem,
                    firmware,
                    storageItemId,
                    session.PowerState.Revision,
                    0).Error,
                Is.EqualTo(PcFictionalDriverInstallationFailures
                    .InvalidOperatingSystemReceipt));
            Assert.That(authority.TryCompleteInstallation(
                    operationId,
                    operatingSystem,
                    foreignFirmware,
                    storageItemId,
                    session.PowerState.Revision,
                    0).Error,
                Is.EqualTo(PcFictionalDriverInstallationFailures
                    .InvalidFirmwareBaselineReceipt));
            Assert.That(authority.TryCompleteInstallation(
                    operationId,
                    operatingSystem,
                    firmware,
                    StableId<ItemInstanceIdScope>.Parse(
                        "inventory.item.issue133.foreign-storage"),
                    session.PowerState.Revision,
                    0).Error,
                Is.EqualTo(PcFictionalDriverInstallationFailures.NotCurrent));

            PcFictionalDriverInstallationReceipt completed = authority
                .TryCompleteInstallation(
                    operationId,
                    operatingSystem,
                    firmware,
                    storageItemId,
                    session.PowerState.Revision,
                    0).Value;

            Assert.That(authority.TryCompleteInstallation(
                    operationId,
                    operatingSystem,
                    firmware,
                    storageItemId,
                    session.PowerState.Revision,
                    1).Error,
                Is.EqualTo(
                    PcFictionalDriverInstallationFailures.OperationConflict));
            Assert.That(authority.TryCompleteInstallation(
                    StableId<
                        PcFictionalDriverInstallationOperationIdScope>.Parse(
                        "assembly.fictional-driver.issue133.duplicate"),
                    operatingSystem,
                    firmware,
                    storageItemId,
                    session.PowerState.Revision,
                    1).Error,
                Is.EqualTo(
                    PcFictionalDriverInstallationFailures.AlreadyCompleted));

            Assert.That(session.PowerState.TryPowerOff(
                    session.PrototypePowerOffOperationId,
                    powerOn,
                    session.PowerState.Revision).IsSuccess,
                Is.True);
            Assert.That(authority.TryCompleteInstallation(
                    operationId,
                    operatingSystem,
                    firmware,
                    storageItemId,
                    1,
                    0).Value,
                Is.SameAs(completed));
            Assert.That(authority.TryCompleteInstallation(
                    StableId<
                        PcFictionalDriverInstallationOperationIdScope>.Parse(
                        "assembly.fictional-driver.issue133.off-cycle"),
                    operatingSystem,
                    firmware,
                    storageItemId,
                    session.PowerState.Revision,
                    authority.Revision).Error,
                Is.EqualTo(PcFictionalDriverInstallationFailures
                    .PowerStateRevisionMismatch));
            Assert.That(authority.EvaluateInstalledDrivers().Value,
                Is.SameAs(completed));
            Assert.That(authority.Revision, Is.EqualTo(1));
            Assert.That(authority.ReceiptCount, Is.EqualTo(1));
            Assert.That(authority.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void FictionalDriverBindsLaterCurrentFirmwareNotOsInstallCycle()
        {
            GarageStockFlowSession session = PreparePowerBudgetReadySession();
            PcFictionalOsInstallationReceipt operatingSystem =
                CompleteCurrentFictionalOsInstallation(
                    session,
                    out PcFirmwareBaselineReceipt osInstallFirmware,
                    out PcPowerStateReceipt osInstallPowerOn);
            PcPowerStateAuthority powerState = session.PowerState;

            Assert.That(powerState.TryPowerOff(
                    session.PrototypePowerOffOperationId,
                    osInstallPowerOn,
                    powerState.Revision).IsSuccess,
                Is.True);
            PcPowerStateReceipt driverCyclePowerOn = powerState.TryPowerOn(
                session.CreatePrototypePowerStateOperationId(
                    PcPowerTransitionKind.PowerOn,
                    powerState.Revision + 1L),
                osInstallFirmware.PreflightReceipt,
                powerState.Revision).Value;
            PcPostStartupReceipt driverCyclePost = powerState
                .TryCompleteStartupSelfTest(
                    session.CreatePrototypePostStartupOperationId(
                        driverCyclePowerOn),
                    driverCyclePowerOn,
                    powerState.Revision).Value;
            PcFirmwareBaselineReceipt driverCycleFirmware = powerState
                .TrySaveFirmwareBaseline(
                    session.CreatePrototypeFirmwareBaselineOperationId(
                        driverCyclePost),
                    driverCyclePost,
                    powerState.Revision,
                    powerState.FirmwareBaselineRevision).Value;
            PcFictionalDriverInstallationAuthority authority =
                session.FictionalDriverInstallation;
            StableId<PcFictionalDriverInstallationOperationIdScope>
                operationId = session
                    .CreatePrototypeFictionalDriverInstallationOperationId(
                        operatingSystem);

            Assert.That(authority.TryCompleteInstallation(
                    operationId,
                    operatingSystem,
                    osInstallFirmware,
                    session.AssemblyBuild.StorageItemId,
                    powerState.Revision,
                    authority.Revision).Error,
                Is.EqualTo(PcFictionalDriverInstallationFailures.NotCurrent));

            OperationResult<PcFictionalDriverInstallationReceipt> completed =
                authority.TryCompleteInstallation(
                    operationId,
                    operatingSystem,
                    driverCycleFirmware,
                    session.AssemblyBuild.StorageItemId,
                    powerState.Revision,
                    authority.Revision);

            Assert.That(completed.IsSuccess, Is.True, completed.Error.Code);
            Assert.That(completed.Value.SourceOperatingSystemReceipt,
                Is.SameAs(operatingSystem));
            Assert.That(completed.Value.SourceFirmwareBaselineReceipt,
                Is.SameAs(driverCycleFirmware));
            Assert.That(completed.Value.SourceFirmwareBaselineReceipt,
                Is.Not.SameAs(operatingSystem.SourceFirmwareBaselineReceipt));
            Assert.That(completed.Value.SourcePowerOnReceipt,
                Is.SameAs(driverCyclePowerOn));
            Assert.That(completed.Value.SourcePostStartupReceipt,
                Is.SameAs(driverCyclePost));
            Assert.That(completed.Value.PreflightReceipt,
                Is.SameAs(operatingSystem.PreflightReceipt));
            Assert.That(authority.EvaluateInstalledDrivers().Value,
                Is.SameAs(completed.Value));
            Assert.That(authority.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void
            FictionalDriverRejectsHardwareAndCableDriftBeforeCompletionButPersistsAfterward()
        {
            GarageStockFlowSession session = PreparePowerBudgetReadySession();
            PcFictionalOsInstallationReceipt operatingSystem =
                CompleteCurrentFictionalOsInstallation(
                    session,
                    out PcFirmwareBaselineReceipt firmware,
                    out PcPowerStateReceipt _);
            PcFictionalDriverInstallationAuthority authority =
                session.FictionalDriverInstallation;
            StableId<PcFictionalDriverInstallationOperationIdScope>
                operationId = session
                    .CreatePrototypeFictionalDriverInstallationOperationId(
                        operatingSystem);
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            long atx24Revision =
                session.AssemblyBuild.Atx24PowerCableRevision;
            long eps12vRevision =
                session.AssemblyBuild.Eps12vPowerCableRevision;
            long pcieRevision =
                session.AssemblyBuild.PcieGpuPowerCableRevision;
            long powerRevision = session.PowerState.Revision;
            long postRevision = session.PowerState.PostStartupRevision;
            long firmwareRevision =
                session.PowerState.FirmwareBaselineRevision;
            long osRevision = session.FictionalOsInstallation.Revision;

            AssertFictionalDriverCompletionRejectsAssemblyDrift(
                session,
                authority,
                operationId,
                operatingSystem,
                firmware,
                "_graphicsCardProductId",
                StableId<ProductDefinitionIdScope>.Parse(
                    "catalog.product.issue133.foreign-gpu"));
            AssertFictionalDriverCompletionRejectsAssemblyDrift(
                session,
                authority,
                operationId,
                operatingSystem,
                firmware,
                "_graphicsCardRetainedByOperationId",
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue133.foreign-gpu-retain"));
            AssertFictionalDriverCompletionRejectsAssemblyDrift(
                session,
                authority,
                operationId,
                operatingSystem,
                firmware,
                "<PcieGpuPowerCableRevision>k__BackingField",
                pcieRevision + 1L);

            Assert.That(authority.Revision, Is.Zero);
            Assert.That(authority.ReceiptCount, Is.Zero);
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
            Assert.That(session.PowerState.Revision, Is.EqualTo(powerRevision));
            Assert.That(session.PowerState.PostStartupRevision,
                Is.EqualTo(postRevision));
            Assert.That(session.PowerState.FirmwareBaselineRevision,
                Is.EqualTo(firmwareRevision));
            Assert.That(session.FictionalOsInstallation.Revision,
                Is.EqualTo(osRevision));

            OperationResult<PcFictionalDriverInstallationReceipt> completed =
                authority.TryCompleteInstallation(
                    operationId,
                    operatingSystem,
                    firmware,
                    session.AssemblyBuild.StorageItemId,
                    powerRevision,
                    authority.Revision);
            Assert.That(completed.IsSuccess, Is.True, completed.Error.Code);

            System.Reflection.FieldInfo graphicsProductField = typeof(
                AssemblyBuildAuthority).GetField(
                "_graphicsCardProductId",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic);
            Assert.That(graphicsProductField, Is.Not.Null);
            object originalGraphicsProduct = graphicsProductField.GetValue(
                session.AssemblyBuild);
            try
            {
                graphicsProductField.SetValue(
                    session.AssemblyBuild,
                    StableId<ProductDefinitionIdScope>.Parse(
                        "catalog.product.issue133.post-install-gpu"));
                Assert.That(authority.EvaluateInstalledDrivers().Value,
                    Is.SameAs(completed.Value),
                    "Installed drivers belong to the exact current OS/storage, " +
                    "not mutable non-storage hardware.");
            }
            finally
            {
                graphicsProductField.SetValue(
                    session.AssemblyBuild,
                    originalGraphicsProduct);
            }

            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(session.PowerState.Revision, Is.EqualTo(powerRevision));
            Assert.That(session.FictionalOsInstallation.Revision,
                Is.EqualTo(osRevision));
            Assert.That(authority.Revision, Is.EqualTo(1));
            Assert.That(authority.ReceiptCount, Is.EqualTo(1));
            Assert.That(authority.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void FictionalDriverHistoryRejectsLineageCorruption()
        {
            GarageStockFlowSession session = PreparePowerBudgetReadySession();
            PcFictionalOsInstallationReceipt operatingSystem =
                CompleteCurrentFictionalOsInstallation(
                    session,
                    out PcFirmwareBaselineReceipt firmware,
                    out PcPowerStateReceipt _);
            PcFictionalDriverInstallationAuthority authority =
                session.FictionalDriverInstallation;
            PcFictionalDriverInstallationReceipt receipt = authority
                .TryCompleteInstallation(
                    session
                        .CreatePrototypeFictionalDriverInstallationOperationId(
                            operatingSystem),
                    operatingSystem,
                    firmware,
                    session.AssemblyBuild.StorageItemId,
                    session.PowerState.Revision,
                    authority.Revision).Value;

            AssertFictionalDriverHistoryRejectsTamper(
                authority,
                receipt,
                "<InstallationStorageSecureOperationId>k__BackingField",
                default(StableId<AssemblyOperationIdScope>));
            AssertFictionalDriverHistoryRejectsTamper(
                authority,
                receipt,
                "<InstallationAssemblyRevision>k__BackingField",
                receipt.InstallationAssemblyRevision + 1L);
            AssertFictionalDriverHistoryRejectsTamper(
                authority,
                receipt,
                "<SourceOperatingSystemReceipt>k__BackingField",
                null);
            AssertFictionalDriverHistoryRejectsTamper(
                authority,
                receipt,
                "<SourceFirmwareBaselineReceipt>k__BackingField",
                null);

            Assert.That(authority.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void AssertFictionalDriverHistoryRejectsTamper(
            PcFictionalDriverInstallationAuthority authority,
            PcFictionalDriverInstallationReceipt receipt,
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
                    PcFictionalDriverInstallationFailures
                        .ReceiptHistoryInvalid),
                fieldName);
            field.SetValue(receipt, originalValue);
            Assert.That(authority.ValidateReceiptHistory().IsSuccess,
                Is.True,
                fieldName);
        }

        private static void AssertFictionalDriverCompletionRejectsAssemblyDrift(
            GarageStockFlowSession session,
            PcFictionalDriverInstallationAuthority authority,
            StableId<PcFictionalDriverInstallationOperationIdScope> operationId,
            PcFictionalOsInstallationReceipt operatingSystem,
            PcFirmwareBaselineReceipt firmware,
            string fieldName,
            object corruptedValue)
        {
            System.Reflection.FieldInfo field = typeof(AssemblyBuildAuthority)
                .GetField(
                    fieldName,
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, fieldName);
            object originalValue = field.GetValue(session.AssemblyBuild);
            try
            {
                field.SetValue(session.AssemblyBuild, corruptedValue);
                OperationResult<PcFictionalDriverInstallationReceipt> rejected =
                    authority.TryCompleteInstallation(
                        operationId,
                        operatingSystem,
                        firmware,
                        session.AssemblyBuild.StorageItemId,
                        session.PowerState.Revision,
                        authority.Revision);
                Assert.That(rejected.Error,
                    Is.EqualTo(PcFictionalDriverInstallationFailures.NotCurrent)
                        .Or.EqualTo(PcFictionalDriverInstallationFailures
                            .HardwareLineageNotCurrent),
                    fieldName);
                Assert.That(authority.Revision, Is.Zero, fieldName);
                Assert.That(authority.ReceiptCount, Is.Zero, fieldName);
            }
            finally
            {
                field.SetValue(session.AssemblyBuild, originalValue);
            }
        }

        private static PcFictionalOsInstallationReceipt
            CompleteCurrentFictionalOsInstallation(
                GarageStockFlowSession session,
                out PcFirmwareBaselineReceipt firmware,
                out PcPowerStateReceipt powerOn)
        {
            firmware = CompleteCurrentFirmwareBaseline(session, out powerOn);
            PcFictionalOsInstallationAuthority authority =
                session.FictionalOsInstallation;
            return authority.TryCompleteInstallation(
                session.CreatePrototypeFictionalOsInstallationOperationId(
                    firmware),
                firmware,
                session.AssemblyBuild.StorageItemId,
                session.PowerState.Revision,
                authority.Revision).Value;
        }
    }
}
