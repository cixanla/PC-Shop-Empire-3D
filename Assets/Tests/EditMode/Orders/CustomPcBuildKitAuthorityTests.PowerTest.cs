using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Presentation.Interaction;

namespace PCShopEmpire3D.Tests.EditMode.Orders
{
    public sealed partial class CustomPcBuildKitAuthorityTests
    {
        [Test]
        public void PlayerPowerTestPreflightPublishesExactReceiptWithoutGameplayMutation()
        {
            GarageStockFlowSession session = PreparePowerBudgetReadySession();
            PowerTestAttemptAuthority authority = session.PowerTestAttempts;
            OperationResult<PowerTestAttemptContext> observed =
                authority.ObserveCurrentContext();
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            long atx24Revision = session.AssemblyBuild.Atx24PowerCableRevision;
            long eps12vRevision = session.AssemblyBuild.Eps12vPowerCableRevision;
            long pcieRevision = session.AssemblyBuild.PcieGpuPowerCableRevision;

            OperationResult<PowerTestAttemptReceipt> result =
                authority.TryAttemptPreflight(
                    session.PrototypePowerTestAttemptOperationId,
                    observed.Value,
                    authority.Revision);

            Assert.That(observed.IsSuccess, Is.True, observed.Error.Code);
            Assert.That(result.IsSuccess, Is.True, result.Error.Code);
            Assert.That(result.Value.OperationId,
                Is.EqualTo(session.PrototypePowerTestAttemptOperationId));
            Assert.That(result.Value.Kind,
                Is.EqualTo(PowerTestAttemptKind.PreflightReady));
            Assert.That(result.Value.ExpectedRevision, Is.Zero);
            Assert.That(result.Value.Revision, Is.EqualTo(1));
            Assert.That(result.Value.Context.BuildId,
                Is.EqualTo(session.AssemblyBuild.BuildId));
            Assert.That(result.Value.Context.ChassisId,
                Is.EqualTo(session.AssemblyBuild.ChassisId));
            Assert.That(result.Value.Context.PolicyId.Value,
                Is.EqualTo(GarageStockFlowSession.PrototypePowerBudgetPolicyIdValue));
            Assert.That(result.Value.Context.SystemPowerDrawWatts, Is.EqualTo(380));
            Assert.That(result.Value.Context.MinimumRecommendedPsuWatts,
                Is.EqualTo(500));
            Assert.That(result.Value.Context.InstalledPsuWatts, Is.EqualTo(550));
            Assert.That(result.Value.Context.CapacityMarginWatts, Is.EqualTo(50));
            Assert.That(authority.Revision, Is.EqualTo(1));
            Assert.That(authority.ReceiptCount, Is.EqualTo(1));
            Assert.That(authority.HasCompletedPreflight, Is.True);
            Assert.That(authority.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(authority.EvaluateCurrentReceipt().Value,
                Is.SameAs(result.Value));
            AssertPowerBudgetDidNotMutate(
                session,
                inventoryRevision,
                buildKitRevision,
                assemblyRevision,
                atx24Revision,
                eps12vRevision,
                pcieRevision);
            Assert.That(session.AssemblyBuild.EvaluateBenchmarkReadiness().IsSuccess,
                Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ExactPowerTestReplayReturnsSameReceiptAndSecondAttemptIsBlocked()
        {
            GarageStockFlowSession session = PreparePowerBudgetReadySession();
            PowerTestAttemptAuthority authority = session.PowerTestAttempts;
            PowerTestAttemptContext context =
                authority.ObserveCurrentContext().Value;
            OperationResult<PowerTestAttemptReceipt> first =
                authority.TryAttemptPreflight(
                    session.PrototypePowerTestAttemptOperationId,
                    context,
                    0);
            long inventoryRevision = session.Inventory.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;

            OperationResult<PowerTestAttemptReceipt> replay =
                authority.TryAttemptPreflight(
                    session.PrototypePowerTestAttemptOperationId,
                    context,
                    0);
            OperationResult<PowerTestAttemptReceipt> conflict =
                authority.TryAttemptPreflight(
                    session.PrototypePowerTestAttemptOperationId,
                    context,
                    1);
            OperationResult<PowerTestAttemptReceipt> second =
                authority.TryAttemptPreflight(
                    StableId<PowerTestAttemptOperationIdScope>.Parse(
                        "assembly.power-test-attempt.prototype-002"),
                    context,
                    1);

            Assert.That(first.IsSuccess, Is.True, first.Error.Code);
            Assert.That(replay.IsSuccess, Is.True, replay.Error.Code);
            Assert.That(replay.Value, Is.SameAs(first.Value));
            Assert.That(conflict.Error,
                Is.EqualTo(PowerTestAttemptFailures.OperationConflict));
            Assert.That(second.Error,
                Is.EqualTo(PowerTestAttemptFailures.AlreadyCompleted));
            Assert.That(authority.Revision, Is.EqualTo(1));
            Assert.That(authority.ReceiptCount, Is.EqualTo(1));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(authority.ValidateReceiptHistory().IsSuccess, Is.True);
        }

        [Test]
        public void HistoricalPowerTestReplayRemainsImmutableAfterContextBecomesStale()
        {
            GarageStockFlowSession session = PreparePowerBudgetReadySession();
            PowerTestAttemptAuthority authority = session.PowerTestAttempts;
            PowerTestAttemptContext acceptedContext =
                authority.ObserveCurrentContext().Value;
            PowerTestAttemptReceipt receipt = authority.TryAttemptPreflight(
                session.PrototypePowerTestAttemptOperationId,
                acceptedContext,
                0).Value;

            Assert.That(session.UnroutePcieGpuPowerCable(
                    StableId<AssemblyOperationIdScope>.Parse(
                        "assembly.operation.issue123.history-unroute-pcie-gpu"),
                    StableId<AssemblyOperationIdScope>.Parse(
                        "assembly.operation.issue121.route-pcie-gpu"),
                    session.AssemblyBuild.PcieGpuPowerCableRevision).IsSuccess,
                Is.True);
            Assert.That(session.RoutePcieGpuPowerCable(
                    StableId<AssemblyOperationIdScope>.Parse(
                        "assembly.operation.issue123.history-reroute-pcie-gpu"),
                    PowerCableKeyOrientation.Keyed,
                    session.AssemblyBuild.PcieGpuPowerCableRevision).IsSuccess,
                Is.True);

            OperationResult<PowerTestAttemptReceipt> historicalReplay =
                authority.TryAttemptPreflight(
                    session.PrototypePowerTestAttemptOperationId,
                    acceptedContext,
                    0);
            OperationResult<PowerTestAttemptReceipt> current =
                authority.EvaluateCurrentReceipt();
            PowerTestAttemptContext changedContext =
                authority.ObserveCurrentContext().Value;
            OperationResult<PowerTestAttemptReceipt> conflictingReplay =
                authority.TryAttemptPreflight(
                    session.PrototypePowerTestAttemptOperationId,
                    changedContext,
                    0);

            Assert.That(historicalReplay.IsSuccess, Is.True,
                historicalReplay.Error.Code);
            Assert.That(historicalReplay.Value, Is.SameAs(receipt));
            Assert.That(current.Error,
                Is.EqualTo(PowerTestAttemptFailures.ContextStale));
            Assert.That(conflictingReplay.Error,
                Is.EqualTo(PowerTestAttemptFailures.OperationConflict));
            Assert.That(authority.Revision, Is.EqualTo(1));
            Assert.That(authority.ReceiptCount, Is.EqualTo(1));
            Assert.That(authority.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ReroutedCableMakesObservedPowerTestContextStaleWithoutReceipt()
        {
            GarageStockFlowSession session = PreparePowerBudgetReadySession();
            PowerTestAttemptAuthority authority = session.PowerTestAttempts;
            PowerTestAttemptContext stale = authority.ObserveCurrentContext().Value;
            StableId<AssemblyOperationIdScope> originalRoute =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue121.route-pcie-gpu");

            Assert.That(session.UnroutePcieGpuPowerCable(
                    StableId<AssemblyOperationIdScope>.Parse(
                        "assembly.operation.issue123.unroute-pcie-gpu"),
                    originalRoute,
                    session.AssemblyBuild.PcieGpuPowerCableRevision).IsSuccess,
                Is.True);
            Assert.That(session.RoutePcieGpuPowerCable(
                    StableId<AssemblyOperationIdScope>.Parse(
                        "assembly.operation.issue123.reroute-pcie-gpu"),
                    PowerCableKeyOrientation.Keyed,
                    session.AssemblyBuild.PcieGpuPowerCableRevision).IsSuccess,
                Is.True);
            Assert.That(session.PowerBudget.AssessPowerBudget().IsSuccess, Is.True);
            long inventoryRevision = session.Inventory.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            long pcieRevision = session.AssemblyBuild.PcieGpuPowerCableRevision;

            OperationResult<PowerTestAttemptReceipt> result =
                authority.TryAttemptPreflight(
                    session.PrototypePowerTestAttemptOperationId,
                    stale,
                    0);

            Assert.That(result.Error,
                Is.EqualTo(PowerTestAttemptFailures.ContextStale));
            Assert.That(authority.Revision, Is.Zero);
            Assert.That(authority.ReceiptCount, Is.Zero);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableRevision,
                Is.EqualTo(pcieRevision));
            Assert.That(authority.ValidateReceiptHistory().IsSuccess, Is.True);
        }

        [Test]
        public void InvalidRevisionForeignAuthorityAndInsufficientPsuFailClosed()
        {
            GarageStockFlowSession session = PreparePowerBudgetReadySession();
            PowerTestAttemptAuthority authority = session.PowerTestAttempts;
            PowerTestAttemptContext context =
                authority.ObserveCurrentContext().Value;

            Assert.That(authority.TryAttemptPreflight(
                    default,
                    context,
                    0).Error,
                Is.EqualTo(PowerTestAttemptFailures.InvalidOperationId));
            Assert.That(authority.TryAttemptPreflight(
                    session.PrototypePowerTestAttemptOperationId,
                    context,
                    1).Error,
                Is.EqualTo(PowerTestAttemptFailures.RevisionMismatch));

            GarageStockFlowSession foreign =
                GarageStockFlowSession.CreateArrived(includeAssemblyPrototype: true);
            Assert.That(PowerTestAttemptAuthority.Create(
                    session.PowerBudget,
                    foreign.AssemblyBuild).Error,
                Is.EqualTo(PowerTestAttemptFailures.AuthorityMismatch));

            PcPowerBudgetAuthority insufficientBudget = CreateTestPowerBudget(
                session,
                450,
                includeGraphicsCardProfile: true);
            PowerTestAttemptAuthority insufficient = PowerTestAttemptAuthority.Create(
                insufficientBudget,
                session.AssemblyBuild).Value;
            PowerTestAttemptContext insufficientContext =
                insufficient.ObserveCurrentContext().Value;
            long inventoryRevision = session.Inventory.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;

            OperationResult<PowerTestAttemptReceipt> result =
                insufficient.TryAttemptPreflight(
                    session.PrototypePowerTestAttemptOperationId,
                    insufficientContext,
                    0);

            Assert.That(insufficientContext.IsSufficient, Is.False);
            Assert.That(insufficientContext.CapacityMarginWatts, Is.EqualTo(-50));
            Assert.That(result.Error,
                Is.EqualTo(PowerTestAttemptFailures.PowerSupplyInsufficient));
            Assert.That(insufficient.Revision, Is.Zero);
            Assert.That(insufficient.ReceiptCount, Is.Zero);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
        }
    }
}
