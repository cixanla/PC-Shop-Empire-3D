using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Presentation.Interaction;

namespace PCShopEmpire3D.Tests.EditMode.Orders
{
    public sealed partial class CustomPcBuildKitAuthorityTests
    {
        [Test]
        public void FirmwareBaselineBindsExactPostAndReplaysWithoutGameplayMutation()
        {
            GarageStockFlowSession session = PreparePowerBudgetReadySession();
            PowerTestAttemptReceipt preflight = CompletePowerPreflight(session);
            PcPowerStateAuthority powerState = session.PowerState;
            PcPowerStateReceipt powerOn = powerState.TryPowerOn(
                session.PrototypePowerOnOperationId,
                preflight,
                0).Value;
            PcPostStartupReceipt postStartup = powerState
                .TryCompleteStartupSelfTest(
                    session.CreatePrototypePostStartupOperationId(powerOn),
                    powerOn,
                    powerState.Revision).Value;
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            long atx24Revision = session.AssemblyBuild.Atx24PowerCableRevision;
            long eps12vRevision = session.AssemblyBuild.Eps12vPowerCableRevision;
            long pcieRevision = session.AssemblyBuild.PcieGpuPowerCableRevision;
            StableId<PcFirmwareBaselineOperationIdScope> operationId =
                session.CreatePrototypeFirmwareBaselineOperationId(postStartup);

            OperationResult<PcFirmwareBaselineReceipt> saved =
                powerState.TrySaveFirmwareBaseline(
                    operationId,
                    postStartup,
                    powerState.Revision,
                    powerState.FirmwareBaselineRevision);
            OperationResult<PcFirmwareBaselineReceipt> replay =
                powerState.TrySaveFirmwareBaseline(
                    operationId,
                    postStartup,
                    powerState.Revision,
                    0);
            OperationResult<PcFirmwareBaselineReceipt> current =
                powerState.EvaluateCurrentFirmwareBaseline();

            Assert.That(saved.IsSuccess, Is.True, saved.Error.Code);
            Assert.That(saved.Value.Profile,
                Is.EqualTo(PcFirmwareBaselineProfile.OptimizedDefaults));
            Assert.That(saved.Value.Result,
                Is.EqualTo(PcFirmwareBaselineResult.SavedAndExited));
            Assert.That(saved.Value.SourcePostStartupReceipt,
                Is.SameAs(postStartup));
            Assert.That(saved.Value.SourcePowerOnReceipt, Is.SameAs(powerOn));
            Assert.That(saved.Value.PreflightReceipt, Is.SameAs(preflight));
            Assert.That(saved.Value.SourcePostStartupRevision, Is.EqualTo(1));
            Assert.That(saved.Value.ExpectedPowerStateRevision, Is.EqualTo(1));
            Assert.That(saved.Value.PowerStateRevision, Is.EqualTo(1));
            Assert.That(saved.Value.ExpectedRevision, Is.Zero);
            Assert.That(saved.Value.Revision, Is.EqualTo(1));
            Assert.That(replay.Value, Is.SameAs(saved.Value));
            Assert.That(current.Value, Is.SameAs(saved.Value));
            Assert.That(powerState.ActiveFirmwareBaselineReceipt,
                Is.SameAs(saved.Value));
            Assert.That(powerState.FirmwareBaselineRevision, Is.EqualTo(1));
            Assert.That(powerState.FirmwareBaselineReceiptCount, Is.EqualTo(1));
            Assert.That(powerState.Revision, Is.EqualTo(1));
            Assert.That(powerState.PostStartupRevision, Is.EqualTo(1));
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
            Assert.That(powerState.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void FirmwareBaselineRejectsForeignRevisionConflictAndDuplicateCycle()
        {
            GarageStockFlowSession session = PreparePowerBudgetReadySession();
            PowerTestAttemptReceipt preflight = CompletePowerPreflight(session);
            PcPowerStateAuthority powerState = session.PowerState;
            GarageStockFlowSession foreign = PreparePowerBudgetReadySession();
            PowerTestAttemptReceipt foreignPreflight =
                CompletePowerPreflight(foreign);
            PcPowerStateReceipt foreignPowerOn = foreign.PowerState.TryPowerOn(
                foreign.PrototypePowerOnOperationId,
                foreignPreflight,
                0).Value;
            PcPostStartupReceipt foreignPost = foreign.PowerState
                .TryCompleteStartupSelfTest(
                    foreign.CreatePrototypePostStartupOperationId(foreignPowerOn),
                    foreignPowerOn,
                    1).Value;
            StableId<PcFirmwareBaselineOperationIdScope> operationId =
                StableId<PcFirmwareBaselineOperationIdScope>.Parse(
                    "assembly.firmware-baseline.issue129.primary");

            Assert.That(powerState.TrySaveFirmwareBaseline(
                    default,
                    null,
                    0,
                    0).Error,
                Is.EqualTo(PcFirmwareBaselineFailures.InvalidOperationId));
            Assert.That(powerState.TrySaveFirmwareBaseline(
                    operationId,
                    null,
                    0,
                    0).Error,
                Is.EqualTo(PcFirmwareBaselineFailures.NotCurrent));

            PcPowerStateReceipt powerOn = powerState.TryPowerOn(
                session.PrototypePowerOnOperationId,
                preflight,
                0).Value;
            PcPostStartupReceipt postStartup = powerState
                .TryCompleteStartupSelfTest(
                    session.CreatePrototypePostStartupOperationId(powerOn),
                    powerOn,
                    1).Value;

            Assert.That(powerState.TrySaveFirmwareBaseline(
                    operationId,
                    foreignPost,
                    1,
                    0).Error,
                Is.EqualTo(
                    PcFirmwareBaselineFailures.InvalidPostStartupReceipt));
            Assert.That(powerState.TrySaveFirmwareBaseline(
                    operationId,
                    postStartup,
                    2,
                    0).Error,
                Is.EqualTo(
                    PcFirmwareBaselineFailures.PowerStateRevisionMismatch));
            Assert.That(powerState.TrySaveFirmwareBaseline(
                    operationId,
                    postStartup,
                    1,
                    1).Error,
                Is.EqualTo(PcFirmwareBaselineFailures.RevisionMismatch));

            PcFirmwareBaselineReceipt saved = powerState
                .TrySaveFirmwareBaseline(
                    operationId,
                    postStartup,
                    1,
                    0).Value;

            Assert.That(powerState.TrySaveFirmwareBaseline(
                    operationId,
                    postStartup,
                    1,
                    1).Error,
                Is.EqualTo(PcFirmwareBaselineFailures.OperationConflict));
            Assert.That(powerState.TrySaveFirmwareBaseline(
                    StableId<PcFirmwareBaselineOperationIdScope>.Parse(
                        "assembly.firmware-baseline.issue129.duplicate"),
                    postStartup,
                    1,
                    1).Error,
                Is.EqualTo(PcFirmwareBaselineFailures.AlreadyCompleted));

            PcPowerStateReceipt powerOff = powerState.TryPowerOff(
                session.PrototypePowerOffOperationId,
                powerOn,
                1).Value;
            OperationResult<PcFirmwareBaselineReceipt> historicalReplay =
                powerState.TrySaveFirmwareBaseline(
                    operationId,
                    postStartup,
                    1,
                    0);

            Assert.That(powerOff.ResultingState, Is.EqualTo(PcPowerState.Off));
            Assert.That(powerState.ActiveFirmwareBaselineReceipt, Is.Null);
            Assert.That(powerState.EvaluateCurrentFirmwareBaseline().Error,
                Is.EqualTo(PcFirmwareBaselineFailures.NotCurrent));
            Assert.That(historicalReplay.Value, Is.SameAs(saved));
            Assert.That(powerState.TrySaveFirmwareBaseline(
                    StableId<PcFirmwareBaselineOperationIdScope>.Parse(
                        "assembly.firmware-baseline.issue129.off-cycle"),
                    postStartup,
                    2,
                    1).Error,
                Is.EqualTo(PcFirmwareBaselineFailures.NotCurrent));
            Assert.That(powerState.FirmwareBaselineRevision, Is.EqualTo(1));
            Assert.That(powerState.FirmwareBaselineReceiptCount, Is.EqualTo(1));
            Assert.That(powerState.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void FirmwareBaselineHistorySupportsRepeatedPowerCycles()
        {
            GarageStockFlowSession session = PreparePowerBudgetReadySession();
            PowerTestAttemptReceipt preflight = CompletePowerPreflight(session);
            PcPowerStateAuthority powerState = session.PowerState;
            PcPowerStateReceipt firstPowerOn = powerState.TryPowerOn(
                session.PrototypePowerOnOperationId,
                preflight,
                0).Value;
            PcPostStartupReceipt firstPost = powerState
                .TryCompleteStartupSelfTest(
                    session.CreatePrototypePostStartupOperationId(firstPowerOn),
                    firstPowerOn,
                    1).Value;
            PcFirmwareBaselineReceipt firstFirmware = powerState
                .TrySaveFirmwareBaseline(
                    session.CreatePrototypeFirmwareBaselineOperationId(firstPost),
                    firstPost,
                    1,
                    0).Value;
            powerState.TryPowerOff(
                session.PrototypePowerOffOperationId,
                firstPowerOn,
                1);

            PcPowerStateReceipt secondPowerOn = powerState.TryPowerOn(
                session.CreatePrototypePowerStateOperationId(
                    PcPowerTransitionKind.PowerOn,
                    3),
                preflight,
                2).Value;
            PcPostStartupReceipt secondPost = powerState
                .TryCompleteStartupSelfTest(
                    session.CreatePrototypePostStartupOperationId(secondPowerOn),
                    secondPowerOn,
                    3).Value;
            PcFirmwareBaselineReceipt secondFirmware = powerState
                .TrySaveFirmwareBaseline(
                    session.CreatePrototypeFirmwareBaselineOperationId(secondPost),
                    secondPost,
                    3,
                    1).Value;
            powerState.TryPowerOff(
                session.CreatePrototypePowerStateOperationId(
                    PcPowerTransitionKind.PowerOff,
                    4),
                secondPowerOn,
                3);

            Assert.That(firstFirmware.Revision, Is.EqualTo(1));
            Assert.That(secondFirmware.Revision, Is.EqualTo(2));
            Assert.That(secondFirmware.SourcePostStartupReceipt,
                Is.SameAs(secondPost));
            Assert.That(secondFirmware.SourcePowerOnReceipt,
                Is.SameAs(secondPowerOn));
            Assert.That(powerState.State, Is.EqualTo(PcPowerState.Off));
            Assert.That(powerState.Revision, Is.EqualTo(4));
            Assert.That(powerState.PostStartupRevision, Is.EqualTo(2));
            Assert.That(powerState.FirmwareBaselineRevision, Is.EqualTo(2));
            Assert.That(powerState.FirmwareBaselineReceiptCount, Is.EqualTo(2));
            Assert.That(powerState.ActiveFirmwareBaselineReceipt, Is.Null);
            Assert.That(powerState.TryGetFirmwareBaselineReceipt(
                    firstFirmware.OperationId,
                    out PcFirmwareBaselineReceipt firstKnown),
                Is.True);
            Assert.That(firstKnown, Is.SameAs(firstFirmware));
            Assert.That(powerState.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }
    }
}
