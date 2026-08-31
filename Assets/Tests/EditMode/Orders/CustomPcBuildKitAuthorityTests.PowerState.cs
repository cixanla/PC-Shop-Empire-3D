using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Presentation.Interaction;

namespace PCShopEmpire3D.Tests.EditMode.Orders
{
    public sealed partial class CustomPcBuildKitAuthorityTests
    {
        [Test]
        public void PowerOnBindsCurrentPreflightAndBlocksLiveAssemblyMaintenance()
        {
            GarageStockFlowSession session = PreparePowerBudgetReadySession();
            PowerTestAttemptReceipt preflight = CompletePowerPreflight(session);
            PcPowerStateAuthority powerState = session.PowerState;
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            long atx24Revision = session.AssemblyBuild.Atx24PowerCableRevision;
            long eps12vRevision = session.AssemblyBuild.Eps12vPowerCableRevision;
            long pcieRevision = session.AssemblyBuild.PcieGpuPowerCableRevision;

            OperationResult<PcPowerStateReceipt> powerOn =
                powerState.TryPowerOn(
                    session.PrototypePowerOnOperationId,
                    preflight,
                    powerState.Revision);
            OperationResult<PcieGpuPowerCableOperationReceipt> blockedCable =
                session.UnroutePcieGpuPowerCable(
                    StableId<AssemblyOperationIdScope>.Parse(
                        "assembly.operation.issue125.blocked-unroute-pcie-gpu"),
                    session.AssemblyBuild.PcieGpuPowerCableRoutedByOperationId,
                    session.AssemblyBuild.PcieGpuPowerCableRevision);
            OperationResult<AssemblyOperationReceipt> blockedMotherboard =
                session.UnsecureMotherboardFastener(
                    StableId<AssemblyOperationIdScope>.Parse(
                        "assembly.operation.issue125.blocked-unsecure-mainboard"),
                    session.AssemblyBuild.InstalledByOperationId,
                    session.AssemblyBuild.SecuredByOperationId,
                    session.AssemblyBuild.Revision);

            Assert.That(powerOn.IsSuccess, Is.True, powerOn.Error.Code);
            Assert.That(powerOn.Value.TransitionKind,
                Is.EqualTo(PcPowerTransitionKind.PowerOn));
            Assert.That(powerOn.Value.ResultingState,
                Is.EqualTo(PcPowerState.Energized));
            Assert.That(powerOn.Value.ExpectedRevision, Is.Zero);
            Assert.That(powerOn.Value.Revision, Is.EqualTo(1));
            Assert.That(powerOn.Value.PreflightReceipt, Is.SameAs(preflight));
            Assert.That(powerOn.Value.SourcePowerOnReceipt, Is.Null);
            Assert.That(powerState.State, Is.EqualTo(PcPowerState.Energized));
            Assert.That(powerState.IsEnergized, Is.True);
            Assert.That(powerState.ActivePowerOnReceipt, Is.SameAs(powerOn.Value));
            Assert.That(session.AssemblyBuild.IsElectricallyEnergized, Is.True);
            Assert.That(blockedCable.Error,
                Is.EqualTo(AssemblyFailures.ElectricalPowerOnMaintenanceBlocked));
            Assert.That(blockedMotherboard.Error,
                Is.EqualTo(AssemblyFailures.ElectricalPowerOnMaintenanceBlocked));
            Assert.That(powerState.EvaluateCurrentPowerOn().Value,
                Is.SameAs(powerOn.Value));
            Assert.That(powerState.ValidateReceiptHistory().IsSuccess, Is.True);
            AssertPowerBudgetDidNotMutate(
                session,
                inventoryRevision,
                buildKitRevision,
                assemblyRevision,
                atx24Revision,
                eps12vRevision,
                pcieRevision);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void PowerOffIsExplicitReplaySafeAndReleasesMaintenanceInterlock()
        {
            GarageStockFlowSession session = PreparePowerBudgetReadySession();
            PowerTestAttemptReceipt preflight = CompletePowerPreflight(session);
            PcPowerStateAuthority powerState = session.PowerState;
            PcPowerStateReceipt powerOn = powerState.TryPowerOn(
                session.PrototypePowerOnOperationId,
                preflight,
                0).Value;

            OperationResult<PcPowerStateReceipt> powerOnReplay =
                powerState.TryPowerOn(
                    session.PrototypePowerOnOperationId,
                    preflight,
                    0);
            OperationResult<PcPowerStateReceipt> conflictingPowerOnReplay =
                powerState.TryPowerOn(
                    session.PrototypePowerOnOperationId,
                    preflight,
                    1);
            OperationResult<PcPowerStateReceipt> secondPowerOn =
                powerState.TryPowerOn(
                    session.CreatePrototypePowerStateOperationId(
                        PcPowerTransitionKind.PowerOn,
                        2),
                    preflight,
                    1);
            OperationResult<PcPowerStateReceipt> powerOff =
                powerState.TryPowerOff(
                    session.PrototypePowerOffOperationId,
                    powerOn,
                    1);
            OperationResult<PcPowerStateReceipt> powerOffReplay =
                powerState.TryPowerOff(
                    session.PrototypePowerOffOperationId,
                    powerOn,
                    1);

            Assert.That(powerOnReplay.Value, Is.SameAs(powerOn));
            Assert.That(conflictingPowerOnReplay.Error,
                Is.EqualTo(PcPowerStateFailures.OperationConflict));
            Assert.That(secondPowerOn.Error,
                Is.EqualTo(PcPowerStateFailures.AlreadyEnergized));
            Assert.That(powerOff.IsSuccess, Is.True, powerOff.Error.Code);
            Assert.That(powerOff.Value.TransitionKind,
                Is.EqualTo(PcPowerTransitionKind.PowerOff));
            Assert.That(powerOff.Value.ResultingState,
                Is.EqualTo(PcPowerState.Off));
            Assert.That(powerOff.Value.PreflightReceipt, Is.SameAs(preflight));
            Assert.That(powerOff.Value.SourcePowerOnReceipt, Is.SameAs(powerOn));
            Assert.That(powerOffReplay.Value, Is.SameAs(powerOff.Value));
            Assert.That(powerState.State, Is.EqualTo(PcPowerState.Off));
            Assert.That(powerState.ActivePowerOnReceipt, Is.Null);
            Assert.That(session.AssemblyBuild.IsElectricallyEnergized, Is.False);
            Assert.That(powerState.EvaluateCurrentPowerOn().Error,
                Is.EqualTo(PcPowerStateFailures.NotEnergized));

            Assert.That(session.UnroutePcieGpuPowerCable(
                    StableId<AssemblyOperationIdScope>.Parse(
                        "assembly.operation.issue125.after-off-unroute-pcie-gpu"),
                    session.AssemblyBuild.PcieGpuPowerCableRoutedByOperationId,
                    session.AssemblyBuild.PcieGpuPowerCableRevision).IsSuccess,
                Is.True);
            Assert.That(powerState.TryPowerOn(
                    session.CreatePrototypePowerStateOperationId(
                        PcPowerTransitionKind.PowerOn,
                        3),
                    preflight,
                    2).Error,
                Is.EqualTo(PcPowerStateFailures.PreflightStale));
            Assert.That(powerState.Revision, Is.EqualTo(2));
            Assert.That(powerState.TryPowerOn(
                    session.PrototypePowerOnOperationId,
                    preflight,
                    0).Value,
                Is.SameAs(powerOn));
            Assert.That(powerState.TryPowerOff(
                    session.PrototypePowerOffOperationId,
                    powerOn,
                    1).Value,
                Is.SameAs(powerOff.Value));
            Assert.That(powerState.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void PowerStateRejectsInvalidStaleForeignAndDuplicateAuthorityCommands()
        {
            GarageStockFlowSession session = PreparePowerBudgetReadySession();
            PowerTestAttemptReceipt preflight = CompletePowerPreflight(session);
            PcPowerStateAuthority powerState = session.PowerState;
            GarageStockFlowSession foreign = PreparePowerBudgetReadySession();
            PowerTestAttemptReceipt foreignPreflight =
                CompletePowerPreflight(foreign);
            long inventoryRevision = session.Inventory.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;

            Assert.That(powerState.TryPowerOn(
                    default,
                    preflight,
                    0).Error,
                Is.EqualTo(PcPowerStateFailures.InvalidOperationId));
            Assert.That(powerState.TryPowerOn(
                    session.PrototypePowerOnOperationId,
                    foreignPreflight,
                    0).Error,
                Is.EqualTo(PcPowerStateFailures.InvalidPreflightReceipt));
            Assert.That(powerState.TryPowerOn(
                    session.PrototypePowerOnOperationId,
                    preflight,
                    1).Error,
                Is.EqualTo(PcPowerStateFailures.RevisionMismatch));
            Assert.That(PcPowerStateAuthority.Create(
                    session.PowerTestAttempts,
                    foreign.AssemblyBuild).Error,
                Is.EqualTo(PcPowerStateFailures.AuthorityMismatch));
            Assert.That(PcPowerStateAuthority.Create(
                    session.PowerTestAttempts,
                    session.AssemblyBuild).Error,
                Is.EqualTo(PcPowerStateFailures.AlreadyBound));
            Assert.That(powerState.TryPowerOff(
                    session.PrototypePowerOffOperationId,
                    null,
                    0).Error,
                Is.EqualTo(PcPowerStateFailures.AlreadyOff));
            Assert.That(powerState.Revision, Is.Zero);
            Assert.That(powerState.ReceiptCount, Is.Zero);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(powerState.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void SessionPowerStateInitializationFailsClosedWhenAssemblyIsAlreadyBound()
        {
            GarageStockFlowSession session = PreparePowerBudgetReadySession();
            PowerTestAttemptAuthority attempts = session.PowerTestAttempts;
            OperationResult<PcPowerStateAuthority> external =
                PcPowerStateAuthority.Create(attempts, session.AssemblyBuild);

            Assert.That(external.IsSuccess, Is.True, external.Error.Code);
            Assert.That(session.TryGetPowerState(out _), Is.False);

            OperationResult<PcPowerStateAuthority> ensured =
                session.EnsurePowerStateAuthority();

            Assert.That(ensured.Error,
                Is.EqualTo(PcPowerStateFailures.AlreadyBound));
            Assert.That(session.PowerState, Is.Null,
                "A supported authority conflict must not escape as Value access exception.");
            Assert.That(session.TryGetPowerState(out _), Is.False);
            Assert.That(external.Value.State, Is.EqualTo(PcPowerState.Off));
            Assert.That(session.AssemblyBuild.IsElectricallyEnergized, Is.False);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void UnchangedPreflightSupportsMultipleExplicitPowerCycles()
        {
            GarageStockFlowSession session = PreparePowerBudgetReadySession();
            PowerTestAttemptReceipt preflight = CompletePowerPreflight(session);
            PcPowerStateAuthority powerState = session.PowerState;
            PcPowerStateReceipt firstPowerOn = powerState.TryPowerOn(
                session.PrototypePowerOnOperationId,
                preflight,
                0).Value;
            PcPowerStateReceipt firstPowerOff = powerState.TryPowerOff(
                session.PrototypePowerOffOperationId,
                firstPowerOn,
                1).Value;
            StableId<PcPowerStateOperationIdScope> secondPowerOnId =
                session.CreatePrototypePowerStateOperationId(
                    PcPowerTransitionKind.PowerOn,
                    3);
            StableId<PcPowerStateOperationIdScope> secondPowerOffId =
                session.CreatePrototypePowerStateOperationId(
                    PcPowerTransitionKind.PowerOff,
                    4);
            PcPowerStateReceipt secondPowerOn = powerState.TryPowerOn(
                secondPowerOnId,
                preflight,
                2).Value;
            PcPowerStateReceipt secondPowerOff = powerState.TryPowerOff(
                secondPowerOffId,
                secondPowerOn,
                3).Value;

            Assert.That(firstPowerOff.SourcePowerOnReceipt,
                Is.SameAs(firstPowerOn));
            Assert.That(secondPowerOn.PreflightReceipt, Is.SameAs(preflight));
            Assert.That(secondPowerOff.SourcePowerOnReceipt,
                Is.SameAs(secondPowerOn));
            Assert.That(powerState.State, Is.EqualTo(PcPowerState.Off));
            Assert.That(powerState.Revision, Is.EqualTo(4));
            Assert.That(powerState.ReceiptCount, Is.EqualTo(4));
            Assert.That(powerState.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void PostStartupBindsExactActivePowerOnWithoutMutatingGameplayState()
        {
            GarageStockFlowSession session = PreparePowerBudgetReadySession();
            PowerTestAttemptReceipt preflight = CompletePowerPreflight(session);
            PcPowerStateAuthority powerState = session.PowerState;
            PcPowerStateReceipt powerOn = powerState.TryPowerOn(
                session.PrototypePowerOnOperationId,
                preflight,
                0).Value;
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            long atx24Revision = session.AssemblyBuild.Atx24PowerCableRevision;
            long eps12vRevision = session.AssemblyBuild.Eps12vPowerCableRevision;
            long pcieRevision = session.AssemblyBuild.PcieGpuPowerCableRevision;
            StableId<PcPostStartupOperationIdScope> operationId =
                session.CreatePrototypePostStartupOperationId(powerOn);

            OperationResult<PcPostStartupReceipt> completed =
                powerState.TryCompleteStartupSelfTest(
                    operationId,
                    powerOn,
                    powerState.Revision);
            OperationResult<PcPostStartupReceipt> replay =
                powerState.TryCompleteStartupSelfTest(
                    operationId,
                    powerOn,
                    powerState.Revision);
            OperationResult<PcPostStartupReceipt> current =
                powerState.EvaluateCurrentStartupSelfTest();

            Assert.That(completed.IsSuccess, Is.True, completed.Error.Code);
            Assert.That(completed.Value.Result,
                Is.EqualTo(PcPostStartupResult.Passed));
            Assert.That(completed.Value.SourcePowerOnReceipt, Is.SameAs(powerOn));
            Assert.That(completed.Value.PreflightReceipt, Is.SameAs(preflight));
            Assert.That(completed.Value.ExpectedPowerStateRevision, Is.EqualTo(1));
            Assert.That(completed.Value.PowerStateRevision, Is.EqualTo(1));
            Assert.That(completed.Value.Revision, Is.EqualTo(1));
            Assert.That(replay.Value, Is.SameAs(completed.Value));
            Assert.That(current.Value, Is.SameAs(completed.Value));
            Assert.That(powerState.ActivePostStartupReceipt,
                Is.SameAs(completed.Value));
            Assert.That(powerState.PostStartupRevision, Is.EqualTo(1));
            Assert.That(powerState.PostStartupReceiptCount, Is.EqualTo(1));
            Assert.That(powerState.Revision, Is.EqualTo(1));
            Assert.That(powerState.ReceiptCount, Is.EqualTo(1));
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
            Assert.That(session.AssemblyBuild.EvaluateBenchmarkReadiness().Error,
                Is.EqualTo(AssemblyFailures.BuildIncomplete));
            Assert.That(powerState.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void PostStartupRejectsOffForeignRevisionConflictAndDuplicateCycle()
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
            StableId<PcPostStartupOperationIdScope> operationId =
                StableId<PcPostStartupOperationIdScope>.Parse(
                    "assembly.post-startup.issue127.primary");

            Assert.That(powerState.TryCompleteStartupSelfTest(
                    default,
                    null,
                    0).Error,
                Is.EqualTo(PcPostStartupFailures.InvalidOperationId));
            Assert.That(powerState.TryCompleteStartupSelfTest(
                    operationId,
                    null,
                    0).Error,
                Is.EqualTo(PcPostStartupFailures.NotCurrent));

            PcPowerStateReceipt powerOn = powerState.TryPowerOn(
                session.PrototypePowerOnOperationId,
                preflight,
                0).Value;
            Assert.That(powerState.TryCompleteStartupSelfTest(
                    operationId,
                    foreignPowerOn,
                    1).Error,
                Is.EqualTo(PcPostStartupFailures.InvalidPowerOnReceipt));
            Assert.That(powerState.TryCompleteStartupSelfTest(
                    operationId,
                    powerOn,
                    0).Error,
                Is.EqualTo(PcPostStartupFailures.PowerStateRevisionMismatch));

            PcPostStartupReceipt completed =
                powerState.TryCompleteStartupSelfTest(
                    operationId,
                    powerOn,
                    1).Value;
            Assert.That(powerState.TryCompleteStartupSelfTest(
                    operationId,
                    foreignPowerOn,
                    1).Error,
                Is.EqualTo(PcPostStartupFailures.OperationConflict));
            Assert.That(powerState.TryCompleteStartupSelfTest(
                    StableId<PcPostStartupOperationIdScope>.Parse(
                        "assembly.post-startup.issue127.duplicate"),
                    powerOn,
                    1).Error,
                Is.EqualTo(PcPostStartupFailures.AlreadyCompleted));

            PcPowerStateReceipt powerOff = powerState.TryPowerOff(
                session.PrototypePowerOffOperationId,
                powerOn,
                1).Value;
            Assert.That(powerOff.ResultingState, Is.EqualTo(PcPowerState.Off));
            Assert.That(powerState.ActivePostStartupReceipt, Is.Null);
            Assert.That(powerState.EvaluateCurrentStartupSelfTest().Error,
                Is.EqualTo(PcPostStartupFailures.NotCurrent));
            Assert.That(powerState.TryCompleteStartupSelfTest(
                    operationId,
                    powerOn,
                    1).Value,
                Is.SameAs(completed));
            Assert.That(powerState.PostStartupRevision, Is.EqualTo(1));
            Assert.That(powerState.PostStartupReceiptCount, Is.EqualTo(1));
            Assert.That(powerState.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void PostStartupHistorySupportsSkippedAndRepeatedPowerCycles()
        {
            GarageStockFlowSession session = PreparePowerBudgetReadySession();
            PowerTestAttemptReceipt preflight = CompletePowerPreflight(session);
            PcPowerStateAuthority powerState = session.PowerState;
            PcPowerStateReceipt firstPowerOn = powerState.TryPowerOn(
                session.PrototypePowerOnOperationId,
                preflight,
                0).Value;
            PcPowerStateReceipt firstPowerOff = powerState.TryPowerOff(
                session.PrototypePowerOffOperationId,
                firstPowerOn,
                1).Value;
            PcPowerStateReceipt secondPowerOn = powerState.TryPowerOn(
                session.CreatePrototypePowerStateOperationId(
                    PcPowerTransitionKind.PowerOn,
                    3),
                preflight,
                2).Value;
            PcPostStartupReceipt secondPost =
                powerState.TryCompleteStartupSelfTest(
                    session.CreatePrototypePostStartupOperationId(secondPowerOn),
                    secondPowerOn,
                    3).Value;
            PcPowerStateReceipt secondPowerOff = powerState.TryPowerOff(
                session.CreatePrototypePowerStateOperationId(
                    PcPowerTransitionKind.PowerOff,
                    4),
                secondPowerOn,
                3).Value;
            PcPowerStateReceipt thirdPowerOn = powerState.TryPowerOn(
                session.CreatePrototypePowerStateOperationId(
                    PcPowerTransitionKind.PowerOn,
                    5),
                preflight,
                4).Value;
            PcPostStartupReceipt thirdPost =
                powerState.TryCompleteStartupSelfTest(
                    session.CreatePrototypePostStartupOperationId(thirdPowerOn),
                    thirdPowerOn,
                    5).Value;
            PcPowerStateReceipt thirdPowerOff = powerState.TryPowerOff(
                session.CreatePrototypePowerStateOperationId(
                    PcPowerTransitionKind.PowerOff,
                    6),
                thirdPowerOn,
                5).Value;

            Assert.That(firstPowerOff.SourcePowerOnReceipt,
                Is.SameAs(firstPowerOn));
            Assert.That(secondPowerOff.SourcePowerOnReceipt,
                Is.SameAs(secondPowerOn));
            Assert.That(thirdPowerOff.SourcePowerOnReceipt,
                Is.SameAs(thirdPowerOn));
            Assert.That(secondPost.Revision, Is.EqualTo(1));
            Assert.That(thirdPost.Revision, Is.EqualTo(2));
            Assert.That(secondPost.SourcePowerOnReceipt, Is.SameAs(secondPowerOn));
            Assert.That(thirdPost.SourcePowerOnReceipt, Is.SameAs(thirdPowerOn));
            Assert.That(powerState.State, Is.EqualTo(PcPowerState.Off));
            Assert.That(powerState.Revision, Is.EqualTo(6));
            Assert.That(powerState.ReceiptCount, Is.EqualTo(6));
            Assert.That(powerState.PostStartupRevision, Is.EqualTo(2));
            Assert.That(powerState.PostStartupReceiptCount, Is.EqualTo(2));
            Assert.That(powerState.ActivePostStartupReceipt, Is.Null);
            Assert.That(powerState.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static PowerTestAttemptReceipt CompletePowerPreflight(
            GarageStockFlowSession session)
        {
            PowerTestAttemptAuthority attempts = session.PowerTestAttempts;
            OperationResult<PowerTestAttemptContext> observed =
                attempts.ObserveCurrentContext();
            Assert.That(observed.IsSuccess, Is.True, observed.Error.Code);
            OperationResult<PowerTestAttemptReceipt> preflight =
                attempts.TryAttemptPreflight(
                    session.PrototypePowerTestAttemptOperationId,
                    observed.Value,
                    attempts.Revision);
            Assert.That(preflight.IsSuccess, Is.True, preflight.Error.Code);
            return preflight.Value;
        }
    }
}
