using System.Collections.Generic;
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
        public void ValidationBindsCurrentLineageAndDeterministicMetricsWithoutUpstreamMutation()
        {
            GarageStockFlowSession session = PreparePowerBudgetReadySession();
            PcFictionalDriverInstallationReceipt driver =
                CompleteCurrentFictionalDriverInstallation(
                    session,
                    out PcFictionalOsInstallationReceipt operatingSystem,
                    out PcFirmwareBaselineReceipt firmware,
                    out PcPowerStateReceipt powerOn);
            PcValidationAuthority authority = session.Validation;
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            long atx24Revision = session.AssemblyBuild.Atx24PowerCableRevision;
            long eps12vRevision = session.AssemblyBuild.Eps12vPowerCableRevision;
            long pcieRevision = session.AssemblyBuild.PcieGpuPowerCableRevision;
            long preflightRevision = session.PowerTestAttempts.Revision;
            int preflightReceiptCount = session.PowerTestAttempts.ReceiptCount;
            long powerRevision = session.PowerState.Revision;
            int powerReceiptCount = session.PowerState.ReceiptCount;
            long postRevision = session.PowerState.PostStartupRevision;
            int postReceiptCount = session.PowerState.PostStartupReceiptCount;
            long firmwareRevision = session.PowerState.FirmwareBaselineRevision;
            int firmwareReceiptCount =
                session.PowerState.FirmwareBaselineReceiptCount;
            long osRevision = session.FictionalOsInstallation.Revision;
            int osReceiptCount = session.FictionalOsInstallation.ReceiptCount;
            long driverRevision = session.FictionalDriverInstallation.Revision;
            int driverReceiptCount =
                session.FictionalDriverInstallation.ReceiptCount;
            StableId<PcValidationOperationIdScope> operationId =
                session.CreatePrototypeValidationOperationId(
                    driver,
                    powerRevision,
                    authority.Revision);

            OperationResult<PcValidationReceipt> completed =
                authority.TryCompleteValidation(
                    operationId,
                    driver,
                    firmware,
                    powerRevision,
                    authority.Revision);
            OperationResult<PcValidationReceipt> replay =
                authority.TryCompleteValidation(
                    operationId,
                    driver,
                    firmware,
                    powerRevision,
                    0);
            OperationResult<PcValidationReceipt> conflict =
                authority.TryCompleteValidation(
                    operationId,
                    driver,
                    firmware,
                    powerRevision,
                    1);
            OperationResult<PcValidationReceipt> current =
                authority.EvaluateCurrentValidation();

            Assert.That(completed.IsSuccess, Is.True, completed.Error.Code);
            Assert.That(replay.Value, Is.SameAs(completed.Value));
            Assert.That(conflict.Error,
                Is.EqualTo(PcValidationFailures.OperationConflict));
            Assert.That(current.Value, Is.SameAs(completed.Value));
            Assert.That(completed.Value.Result,
                Is.EqualTo(PcValidationResult.PassedForQualityStage));
            Assert.That(completed.Value.StressResult,
                Is.EqualTo(PcStressResult.Stable));
            Assert.That(completed.Value.SourceDriverReceipt, Is.SameAs(driver));
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
            Assert.That(completed.Value.SourceElectricalReadiness,
                Is.SameAs(firmware.PreflightReceipt.Context.ElectricalReadiness));
            Assert.That(completed.Value.SourcePowerBudget,
                Is.SameAs(firmware.PreflightReceipt.Context.PowerBudget));
            Assert.That(completed.Value.PerformanceCatalogId,
                Is.EqualTo(session.PerformanceCatalog.CatalogId));
            Assert.That(completed.Value.ProfileId,
                Is.EqualTo(session.ValidationProfile.Id));
            Assert.That(completed.Value.StorageItemId,
                Is.EqualTo(session.AssemblyBuild.StorageItemId));
            Assert.That(completed.Value.StorageProductId,
                Is.EqualTo(session.AssemblyBuild.StorageProductId));
            Assert.That(completed.Value.BenchmarkScore,
                Is.EqualTo(
                    GarageStockFlowSession.PrototypeExpectedValidationBenchmarkScore));
            Assert.That(completed.Value.ProcessorScore,
                Is.EqualTo(
                    GarageStockFlowSession.PrototypeProcessorPerformanceScore));
            Assert.That(completed.Value.GraphicsCardScore,
                Is.EqualTo(
                    GarageStockFlowSession.PrototypeGraphicsCardPerformanceScore));
            Assert.That(completed.Value.StressSteps,
                Is.EqualTo(GarageStockFlowSession.PrototypeValidationStressSteps));
            Assert.That(completed.Value.ProcessorPeakTemperatureCelsius,
                Is.EqualTo(GarageStockFlowSession
                    .PrototypeExpectedProcessorPeakTemperatureCelsius));
            Assert.That(completed.Value.GraphicsCardPeakTemperatureCelsius,
                Is.EqualTo(GarageStockFlowSession
                    .PrototypeExpectedGraphicsPeakTemperatureCelsius));
            Assert.That(completed.Value.SystemPowerDrawWatts, Is.EqualTo(380));
            Assert.That(completed.Value.MinimumRecommendedPsuWatts,
                Is.EqualTo(500));
            Assert.That(completed.Value.InstalledPsuWatts, Is.EqualTo(550));
            Assert.That(completed.Value.PowerMarginWatts, Is.EqualTo(50));
            Assert.That(completed.Value.QualityTier,
                Is.EqualTo(PcQualityTier.Good));
            Assert.That(completed.Value.ExpectedPowerStateRevision,
                Is.EqualTo(powerRevision));
            Assert.That(completed.Value.PowerStateRevision,
                Is.EqualTo(powerRevision));
            Assert.That(completed.Value.ExpectedRevision, Is.Zero);
            Assert.That(completed.Value.Revision, Is.EqualTo(1));
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
            Assert.That(session.PowerTestAttempts.Revision,
                Is.EqualTo(preflightRevision));
            Assert.That(session.PowerTestAttempts.ReceiptCount,
                Is.EqualTo(preflightReceiptCount));
            Assert.That(session.PowerState.Revision, Is.EqualTo(powerRevision));
            Assert.That(session.PowerState.ReceiptCount,
                Is.EqualTo(powerReceiptCount));
            Assert.That(session.PowerState.PostStartupRevision,
                Is.EqualTo(postRevision));
            Assert.That(session.PowerState.PostStartupReceiptCount,
                Is.EqualTo(postReceiptCount));
            Assert.That(session.PowerState.FirmwareBaselineRevision,
                Is.EqualTo(firmwareRevision));
            Assert.That(session.PowerState.FirmwareBaselineReceiptCount,
                Is.EqualTo(firmwareReceiptCount));
            Assert.That(session.FictionalOsInstallation.Revision,
                Is.EqualTo(osRevision));
            Assert.That(session.FictionalOsInstallation.ReceiptCount,
                Is.EqualTo(osReceiptCount));
            Assert.That(session.FictionalDriverInstallation.Revision,
                Is.EqualTo(driverRevision));
            Assert.That(session.FictionalDriverInstallation.ReceiptCount,
                Is.EqualTo(driverReceiptCount));
            Assert.That(session.AssemblyBuild.EvaluateBenchmarkReadiness().IsSuccess,
                Is.True);
            Assert.That(authority.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ValidationAllowsRerunAndRequiresNewRunForNewPowerCycle()
        {
            GarageStockFlowSession session = PreparePowerBudgetReadySession();
            PcFictionalDriverInstallationReceipt driver =
                CompleteCurrentFictionalDriverInstallation(
                    session,
                    out _,
                    out PcFirmwareBaselineReceipt firstFirmware,
                    out PcPowerStateReceipt firstPowerOn);
            PcValidationAuthority authority = session.Validation;
            StableId<PcValidationOperationIdScope> firstOperation =
                session.CreatePrototypeValidationOperationId(
                    driver,
                    session.PowerState.Revision,
                    authority.Revision);
            PcValidationReceipt first = authority.TryCompleteValidation(
                firstOperation,
                driver,
                firstFirmware,
                session.PowerState.Revision,
                authority.Revision).Value;
            StableId<PcValidationOperationIdScope> secondOperation =
                session.CreatePrototypeValidationOperationId(
                    driver,
                    session.PowerState.Revision,
                    authority.Revision);
            PcValidationReceipt second = authority.TryCompleteValidation(
                secondOperation,
                driver,
                firstFirmware,
                session.PowerState.Revision,
                authority.Revision).Value;

            Assert.That(first.Revision, Is.EqualTo(1));
            Assert.That(second.Revision, Is.EqualTo(2));
            Assert.That(second.OperationId, Is.Not.EqualTo(first.OperationId));
            Assert.That(second.BenchmarkScore, Is.EqualTo(first.BenchmarkScore));
            Assert.That(second.ProcessorPeakTemperatureCelsius,
                Is.EqualTo(first.ProcessorPeakTemperatureCelsius));
            Assert.That(second.GraphicsCardPeakTemperatureCelsius,
                Is.EqualTo(first.GraphicsCardPeakTemperatureCelsius));
            Assert.That(authority.EvaluateCurrentValidation().Value,
                Is.SameAs(second));

            Assert.That(session.PowerState.TryPowerOff(
                    session.PrototypePowerOffOperationId,
                    firstPowerOn,
                    session.PowerState.Revision).IsSuccess,
                Is.True);
            Assert.That(authority.EvaluateCurrentValidation().Error,
                Is.EqualTo(PcValidationFailures.NotCurrent));
            Assert.That(authority.TryCompleteValidation(
                    firstOperation,
                    driver,
                    firstFirmware,
                    first.ExpectedPowerStateRevision,
                    first.ExpectedRevision).Value,
                Is.SameAs(first));
            Assert.That(authority.TryGetReceipt(
                    secondOperation,
                    out PcValidationReceipt historical),
                Is.True);
            Assert.That(historical, Is.SameAs(second));

            PowerTestAttemptReceipt preflight = firstFirmware.PreflightReceipt;
            PcPowerStateReceipt secondPowerOn = session.PowerState.TryPowerOn(
                session.CreatePrototypePowerStateOperationId(
                    PcPowerTransitionKind.PowerOn,
                    session.PowerState.Revision + 1L),
                preflight,
                session.PowerState.Revision).Value;
            PcPostStartupReceipt secondPost = session.PowerState
                .TryCompleteStartupSelfTest(
                    session.CreatePrototypePostStartupOperationId(secondPowerOn),
                    secondPowerOn,
                    session.PowerState.Revision).Value;
            PcFirmwareBaselineReceipt secondFirmware = session.PowerState
                .TrySaveFirmwareBaseline(
                    session.CreatePrototypeFirmwareBaselineOperationId(secondPost),
                    secondPost,
                    session.PowerState.Revision,
                    session.PowerState.FirmwareBaselineRevision).Value;

            Assert.That(authority.EvaluateCurrentValidation().Error,
                Is.EqualTo(PcValidationFailures.NotCurrent));
            StableId<PcValidationOperationIdScope> thirdOperation =
                session.CreatePrototypeValidationOperationId(
                    driver,
                    session.PowerState.Revision,
                    authority.Revision);
            PcValidationReceipt third = authority.TryCompleteValidation(
                thirdOperation,
                driver,
                secondFirmware,
                session.PowerState.Revision,
                authority.Revision).Value;

            Assert.That(third.Revision, Is.EqualTo(3));
            Assert.That(third.SourceDriverReceipt, Is.SameAs(driver));
            Assert.That(third.SourceFirmwareBaselineReceipt,
                Is.SameAs(secondFirmware));
            Assert.That(third.ExpectedPowerStateRevision,
                Is.EqualTo(secondPowerOn.Revision));
            Assert.That(authority.EvaluateCurrentValidation().Value,
                Is.SameAs(third));
            Assert.That(authority.Revision, Is.EqualTo(3));
            Assert.That(authority.ReceiptCount, Is.EqualTo(3));
            Assert.That(authority.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ValidationRejectsForeignStaleAndInvalidCommandsWithoutReceipt()
        {
            GarageStockFlowSession session = PreparePowerBudgetReadySession();
            PcFictionalDriverInstallationReceipt driver =
                CompleteCurrentFictionalDriverInstallation(
                    session,
                    out _,
                    out PcFirmwareBaselineReceipt firmware,
                    out PcPowerStateReceipt powerOn);
            GarageStockFlowSession foreign = PreparePowerBudgetReadySession();
            PcFictionalDriverInstallationReceipt foreignDriver =
                CompleteCurrentFictionalDriverInstallation(
                    foreign,
                    out _,
                    out PcFirmwareBaselineReceipt foreignFirmware,
                    out _);
            PcValidationAuthority authority = session.Validation;
            StableId<PcValidationOperationIdScope> operationId =
                StableId<PcValidationOperationIdScope>.Parse(
                    "assembly.validation.issue135.invalid-command");

            Assert.That(PcValidationAuthority.Create(
                    null,
                    session.PowerBudget,
                    session.PerformanceCatalog,
                    session.ValidationProfile).Error,
                Is.EqualTo(PcValidationFailures.ConfigurationMissing));
            Assert.That(PcValidationAuthority.Create(
                    session.FictionalDriverInstallation,
                    foreign.PowerBudget,
                    session.PerformanceCatalog,
                    session.ValidationProfile).Error,
                Is.EqualTo(PcValidationFailures.AuthorityMismatch));
            Assert.That(PcValidationAuthority.Create(
                    session.FictionalDriverInstallation,
                    session.PowerBudget,
                    foreign.PerformanceCatalog,
                    session.ValidationProfile).Error,
                Is.EqualTo(PcValidationFailures.CatalogMismatch));
            Assert.That(PcValidationProfile.Create(
                    default,
                    300,
                    22,
                    50,
                    90,
                    88,
                    25,
                    300,
                    380,
                    500).Error,
                Is.EqualTo(PcValidationFailures.ProfileInvalid));
            Assert.That(authority.TryCompleteValidation(
                    default,
                    driver,
                    firmware,
                    session.PowerState.Revision,
                    authority.Revision).Error,
                Is.EqualTo(PcValidationFailures.InvalidOperationId));
            Assert.That(authority.TryCompleteValidation(
                    operationId,
                    foreignDriver,
                    firmware,
                    session.PowerState.Revision,
                    authority.Revision).Error,
                Is.EqualTo(PcValidationFailures.InvalidDriverReceipt));
            Assert.That(authority.TryCompleteValidation(
                    operationId,
                    driver,
                    foreignFirmware,
                    session.PowerState.Revision,
                    authority.Revision).Error,
                Is.EqualTo(PcValidationFailures.InvalidFirmwareBaselineReceipt));
            Assert.That(authority.TryCompleteValidation(
                    operationId,
                    driver,
                    firmware,
                    session.PowerState.Revision + 1L,
                    authority.Revision).Error,
                Is.EqualTo(PcValidationFailures.PowerStateRevisionMismatch));
            Assert.That(authority.TryCompleteValidation(
                    operationId,
                    driver,
                    firmware,
                    session.PowerState.Revision,
                    authority.Revision + 1L).Error,
                Is.EqualTo(PcValidationFailures.RevisionMismatch));
            Assert.That(authority.Revision, Is.Zero);
            Assert.That(authority.ReceiptCount, Is.Zero);

            Assert.That(session.PowerState.TryPowerOff(
                    session.PrototypePowerOffOperationId,
                    powerOn,
                    session.PowerState.Revision).IsSuccess,
                Is.True);
            Assert.That(authority.TryCompleteValidation(
                    StableId<PcValidationOperationIdScope>.Parse(
                        "assembly.validation.issue135.off-cycle"),
                    driver,
                    firmware,
                    session.PowerState.Revision,
                    authority.Revision).Error,
                Is.EqualTo(PcValidationFailures.PowerStateRevisionMismatch));
            Assert.That(authority.Revision, Is.Zero);
            Assert.That(authority.ReceiptCount, Is.Zero);
            Assert.That(authority.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ValidationScoreThermalPowerAndOverflowBoundariesFailClosed()
        {
            GarageStockFlowSession session = PreparePowerBudgetReadySession();
            PcFictionalDriverInstallationReceipt driver =
                CompleteCurrentFictionalDriverInstallation(
                    session,
                    out _,
                    out PcFirmwareBaselineReceipt firmware,
                    out _);

            PcValidationAuthority missingProfile = CreateValidationAuthority(
                session,
                CreateValidationCatalog(
                    session,
                    "catalog.performance.issue135.missing-graphics",
                    includeGraphics: false),
                session.ValidationProfile);
            AssertValidationFailure(
                missingProfile,
                session,
                driver,
                firmware,
                "missing-graphics",
                PcValidationFailures.PerformanceProfileMissing);

            PcValidationAuthority lowScore = CreateValidationAuthority(
                session,
                session.PerformanceCatalog,
                CreateValidationProfile(
                    "score-low",
                    minimumBenchmarkScore: 402,
                    goodBenchmarkScore: 402));
            AssertValidationFailure(
                lowScore,
                session,
                driver,
                firmware,
                "score-low",
                PcValidationFailures.ScoreBelowMinimum);

            PcValidationAuthority powerMargin = CreateValidationAuthority(
                session,
                session.PerformanceCatalog,
                CreateValidationProfile(
                    "power-margin",
                    minimumPowerMarginWatts: 51));
            AssertValidationFailure(
                powerMargin,
                session,
                driver,
                firmware,
                "power-margin",
                PcValidationFailures.PowerMarginInsufficient);

            PcValidationAuthority processorThermal = CreateValidationAuthority(
                session,
                CreateValidationCatalog(
                    session,
                    "catalog.performance.issue135.processor-hot",
                    processorThermalLoadWatts: 200,
                    processorCoolerCapacityWatts: 100),
                session.ValidationProfile);
            AssertValidationFailure(
                processorThermal,
                session,
                driver,
                firmware,
                "processor-hot",
                PcValidationFailures.ProcessorThermalLimitExceeded);

            PcValidationAuthority graphicsThermal = CreateValidationAuthority(
                session,
                session.PerformanceCatalog,
                CreateValidationProfile(
                    "graphics-hot",
                    maximumGraphicsTemperatureCelsius: 63));
            AssertValidationFailure(
                graphicsThermal,
                session,
                driver,
                firmware,
                "graphics-hot",
                PcValidationFailures.GraphicsThermalLimitExceeded);

            PcValidationAuthority overflow = CreateValidationAuthority(
                session,
                CreateValidationCatalog(
                    session,
                    "catalog.performance.issue135.overflow",
                    processorThermalLoadWatts:
                        PcPerformanceSpecification.MaximumSupportedWatts,
                    processorCoolerCapacityWatts: 1),
                CreateValidationProfile(
                    "overflow",
                    thermalRiseScale:
                        PcValidationProfile.MaximumThermalRiseScale));
            AssertValidationFailure(
                overflow,
                session,
                driver,
                firmware,
                "overflow",
                PcValidationFailures.ArithmeticOverflow);

            Assert.That(session.Validation.ReceiptCount, Is.Zero);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ValidationQualityThresholdsAreInclusiveAndDeterministic()
        {
            GarageStockFlowSession session = PreparePowerBudgetReadySession();
            PcFictionalDriverInstallationReceipt driver =
                CompleteCurrentFictionalDriverInstallation(
                    session,
                    out _,
                    out PcFirmwareBaselineReceipt firmware,
                    out _);
            PcValidationAuthority standard = CreateValidationAuthority(
                session,
                session.PerformanceCatalog,
                CreateValidationProfile(
                    "quality-standard",
                    goodBenchmarkScore: 402));
            PcValidationAuthority good = CreateValidationAuthority(
                session,
                session.PerformanceCatalog,
                CreateValidationProfile("quality-good"));
            PcValidationAuthority excellent = CreateValidationAuthority(
                session,
                session.PerformanceCatalog,
                CreateValidationProfile(
                    "quality-excellent",
                    excellentBenchmarkScore: 401));

            PcValidationReceipt standardReceipt = CompleteValidation(
                standard,
                session,
                driver,
                firmware,
                "quality-standard");
            PcValidationReceipt goodReceipt = CompleteValidation(
                good,
                session,
                driver,
                firmware,
                "quality-good");
            PcValidationReceipt excellentReceipt = CompleteValidation(
                excellent,
                session,
                driver,
                firmware,
                "quality-excellent");

            Assert.That(standardReceipt.BenchmarkScore, Is.EqualTo(401));
            Assert.That(goodReceipt.BenchmarkScore,
                Is.EqualTo(standardReceipt.BenchmarkScore));
            Assert.That(excellentReceipt.BenchmarkScore,
                Is.EqualTo(standardReceipt.BenchmarkScore));
            Assert.That(standardReceipt.QualityTier,
                Is.EqualTo(PcQualityTier.Standard));
            Assert.That(goodReceipt.QualityTier,
                Is.EqualTo(PcQualityTier.Good));
            Assert.That(excellentReceipt.QualityTier,
                Is.EqualTo(PcQualityTier.Excellent));
            Assert.That(standard.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(good.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(excellent.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ValidationReceiptHistoryRejectsMetricAndLineageTamper()
        {
            GarageStockFlowSession session = PreparePowerBudgetReadySession();
            PcFictionalDriverInstallationReceipt driver =
                CompleteCurrentFictionalDriverInstallation(
                    session,
                    out _,
                    out PcFirmwareBaselineReceipt firmware,
                    out _);
            PcValidationAuthority authority = session.Validation;
            PcValidationReceipt receipt = CompleteValidation(
                authority,
                session,
                driver,
                firmware,
                "history");

            AssertValidationHistoryRejectsTamper(
                authority,
                receipt,
                "<StressSteps>k__BackingField",
                receipt.StressSteps + 1);
            AssertValidationHistoryRejectsTamper(
                authority,
                receipt,
                "<BenchmarkScore>k__BackingField",
                receipt.BenchmarkScore + 1);
            AssertValidationHistoryRejectsTamper(
                authority,
                receipt,
                "<SourcePowerBudget>k__BackingField",
                null);
            AssertValidationHistoryRejectsTamper(
                authority,
                receipt,
                "<ProfileId>k__BackingField",
                default(StableId<PcValidationProfileIdScope>));

            Assert.That(authority.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static PcFictionalDriverInstallationReceipt
            CompleteCurrentFictionalDriverInstallation(
                GarageStockFlowSession session,
                out PcFictionalOsInstallationReceipt operatingSystem,
                out PcFirmwareBaselineReceipt firmware,
                out PcPowerStateReceipt powerOn)
        {
            operatingSystem = CompleteCurrentFictionalOsInstallation(
                session,
                out firmware,
                out powerOn);
            PcFictionalDriverInstallationAuthority authority =
                session.FictionalDriverInstallation;
            return authority.TryCompleteInstallation(
                session.CreatePrototypeFictionalDriverInstallationOperationId(
                    operatingSystem),
                operatingSystem,
                firmware,
                session.AssemblyBuild.StorageItemId,
                session.PowerState.Revision,
                authority.Revision).Value;
        }

        private static PcValidationAuthority CreateValidationAuthority(
            GarageStockFlowSession session,
            PcPerformanceCatalog catalog,
            PcValidationProfile profile)
        {
            OperationResult<PcValidationAuthority> result =
                PcValidationAuthority.Create(
                    session.FictionalDriverInstallation,
                    session.PowerBudget,
                    catalog,
                    profile);
            Assert.That(result.IsSuccess, Is.True, result.Error.Code);
            return result.Value;
        }

        private static PcValidationReceipt CompleteValidation(
            PcValidationAuthority authority,
            GarageStockFlowSession session,
            PcFictionalDriverInstallationReceipt driver,
            PcFirmwareBaselineReceipt firmware,
            string suffix)
        {
            OperationResult<PcValidationReceipt> result =
                authority.TryCompleteValidation(
                    StableId<PcValidationOperationIdScope>.Parse(
                        "assembly.validation.issue135." + suffix),
                    driver,
                    firmware,
                    session.PowerState.Revision,
                    authority.Revision);
            Assert.That(result.IsSuccess, Is.True, result.Error.Code);
            return result.Value;
        }

        private static void AssertValidationFailure(
            PcValidationAuthority authority,
            GarageStockFlowSession session,
            PcFictionalDriverInstallationReceipt driver,
            PcFirmwareBaselineReceipt firmware,
            string suffix,
            Failure expected)
        {
            OperationResult<PcValidationReceipt> result =
                authority.TryCompleteValidation(
                    StableId<PcValidationOperationIdScope>.Parse(
                        "assembly.validation.issue135.failure." + suffix),
                    driver,
                    firmware,
                    session.PowerState.Revision,
                    authority.Revision);
            Assert.That(result.Error, Is.EqualTo(expected), suffix);
            Assert.That(authority.Revision, Is.Zero, suffix);
            Assert.That(authority.ReceiptCount, Is.Zero, suffix);
            Assert.That(authority.ValidateReceiptHistory().IsSuccess,
                Is.True,
                suffix);
        }

        private static PcPerformanceCatalog CreateValidationCatalog(
            GarageStockFlowSession session,
            string catalogId,
            int processorThermalLoadWatts =
                GarageStockFlowSession.PrototypeProcessorThermalLoadWatts,
            int processorCoolerCapacityWatts =
                GarageStockFlowSession.PrototypeProcessorCoolerCapacityWatts,
            int graphicsThermalLoadWatts =
                GarageStockFlowSession.PrototypeGraphicsCardThermalLoadWatts,
            int graphicsCoolingCapacityWatts =
                GarageStockFlowSession.PrototypeGraphicsCardCoolingCapacityWatts,
            bool includeGraphics = true)
        {
            var specifications = new List<PcPerformanceSpecification>
            {
                PerformanceSpecification(
                    session,
                    session.MotherboardProductId,
                    GarageStockFlowSession.PrototypeMotherboardPerformanceScore,
                    0,
                    0),
                PerformanceSpecification(
                    session,
                    session.ProcessorProductId,
                    GarageStockFlowSession.PrototypeProcessorPerformanceScore,
                    processorThermalLoadWatts,
                    0),
                PerformanceSpecification(
                    session,
                    session.MemoryProductId,
                    GarageStockFlowSession.PrototypeMemoryPerformanceScore,
                    0,
                    0),
                PerformanceSpecification(
                    session,
                    session.StorageProductId,
                    GarageStockFlowSession.PrototypeStoragePerformanceScore,
                    0,
                    0),
                PerformanceSpecification(
                    session,
                    session.ProcessorCoolerProductId,
                    GarageStockFlowSession
                        .PrototypeProcessorCoolerPerformanceScore,
                    0,
                    processorCoolerCapacityWatts),
                PerformanceSpecification(
                    session,
                    session.PowerSupplyProductId,
                    GarageStockFlowSession.PrototypePowerSupplyPerformanceScore,
                    0,
                    0)
            };
            if (includeGraphics)
            {
                specifications.Add(PerformanceSpecification(
                    session,
                    session.ProductId,
                    GarageStockFlowSession.PrototypeGraphicsCardPerformanceScore,
                    graphicsThermalLoadWatts,
                    graphicsCoolingCapacityWatts));
            }

            OperationResult<PcPerformanceCatalog> result =
                PcPerformanceCatalog.Create(
                    StableId<PcPerformanceCatalogIdScope>.Parse(catalogId),
                    session.Components,
                    specifications);
            Assert.That(result.IsSuccess, Is.True, result.Error.Code);
            return result.Value;
        }

        private static PcPerformanceSpecification PerformanceSpecification(
            GarageStockFlowSession session,
            StableId<ProductDefinitionIdScope> productId,
            int score,
            int thermalLoadWatts,
            int coolingCapacityWatts)
        {
            OperationResult<PcPerformanceSpecification> result =
                PcPerformanceSpecification.Create(
                    session.Components,
                    productId,
                    score,
                    thermalLoadWatts,
                    coolingCapacityWatts);
            Assert.That(result.IsSuccess, Is.True, result.Error.Code);
            return result.Value;
        }

        private static PcValidationProfile CreateValidationProfile(
            string suffix,
            int stressSteps =
                GarageStockFlowSession.PrototypeValidationStressSteps,
            int ambientTemperatureCelsius =
                GarageStockFlowSession.PrototypeValidationAmbientTemperatureCelsius,
            int thermalRiseScale =
                GarageStockFlowSession.PrototypeValidationThermalRiseScale,
            int maximumProcessorTemperatureCelsius =
                GarageStockFlowSession
                    .PrototypeValidationMaximumProcessorTemperatureCelsius,
            int maximumGraphicsTemperatureCelsius =
                GarageStockFlowSession
                    .PrototypeValidationMaximumGraphicsTemperatureCelsius,
            int minimumPowerMarginWatts =
                GarageStockFlowSession.PrototypeValidationMinimumPowerMarginWatts,
            int minimumBenchmarkScore =
                GarageStockFlowSession.PrototypeValidationMinimumBenchmarkScore,
            int goodBenchmarkScore =
                GarageStockFlowSession.PrototypeValidationGoodBenchmarkScore,
            int excellentBenchmarkScore =
                GarageStockFlowSession.PrototypeValidationExcellentBenchmarkScore)
        {
            OperationResult<PcValidationProfile> result = PcValidationProfile.Create(
                StableId<PcValidationProfileIdScope>.Parse(
                    "assembly.validation-profile.issue135." + suffix),
                stressSteps,
                ambientTemperatureCelsius,
                thermalRiseScale,
                maximumProcessorTemperatureCelsius,
                maximumGraphicsTemperatureCelsius,
                minimumPowerMarginWatts,
                minimumBenchmarkScore,
                goodBenchmarkScore,
                excellentBenchmarkScore);
            Assert.That(result.IsSuccess, Is.True, result.Error.Code);
            return result.Value;
        }

        private static void AssertValidationHistoryRejectsTamper(
            PcValidationAuthority authority,
            PcValidationReceipt receipt,
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
                Is.EqualTo(PcValidationFailures.ReceiptHistoryInvalid),
                fieldName);
            Assert.That(authority.TryCompleteValidation(
                    receipt.OperationId,
                    receipt.SourceDriverReceipt,
                    receipt.SourceFirmwareBaselineReceipt,
                    receipt.ExpectedPowerStateRevision,
                    receipt.ExpectedRevision).Error,
                Is.EqualTo(PcValidationFailures.ReceiptHistoryInvalid),
                fieldName + " replay must fail closed");
            field.SetValue(receipt, originalValue);
            Assert.That(authority.ValidateReceiptHistory().IsSuccess,
                Is.True,
                fieldName);
        }
    }
}
