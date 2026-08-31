using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.Quality;

namespace PCShopEmpire3D.Tests.EditMode.Orders
{
    public sealed partial class CustomPcBuildKitAuthorityTests
    {
        [Test]
        public void QualityReleaseBindsExactJobValidationAndSafeShutdownWithoutUpstreamMutation()
        {
            QualityFixture fixture = PrepareQualityFixture();
            GarageStockFlowSession session = fixture.Session;
            long inventoryRevision = session.Inventory.Revision;
            long workOrderRevision = session.CustomPcWorkOrders.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            long powerRevision = session.PowerState.Revision;
            long validationRevision = session.Validation.Revision;
            int validationReceiptCount = session.Validation.ReceiptCount;
            StableId<CustomPcQualityReleaseOperationIdScope> operationId =
                QualityOperationId("exact");

            OperationResult<CustomPcQualityReleaseReceipt> completed =
                fixture.Authority.TryReleaseForPackaging(
                    operationId,
                    fixture.WorkOrder,
                    fixture.WorkTicket,
                    fixture.ValidationReceipt,
                    fixture.PowerOffReceipt,
                    fixture.Authority.Revision);
            OperationResult<CustomPcQualityReleaseReceipt> replay =
                fixture.Authority.TryReleaseForPackaging(
                    operationId,
                    fixture.WorkOrder,
                    fixture.WorkTicket,
                    fixture.ValidationReceipt,
                    fixture.PowerOffReceipt,
                    0);
            OperationResult<CustomPcQualityReleaseReceipt> conflict =
                fixture.Authority.TryReleaseForPackaging(
                    operationId,
                    fixture.WorkOrder,
                    fixture.WorkTicket,
                    fixture.ValidationReceipt,
                    fixture.PowerOffReceipt,
                    1);

            Assert.That(completed.IsSuccess, Is.True, completed.Error.Code);
            Assert.That(replay.Value, Is.SameAs(completed.Value));
            Assert.That(conflict.Error,
                Is.EqualTo(CustomPcQualityReleaseFailures.OperationConflict));
            Assert.That(fixture.Authority.EvaluateCurrentRelease().Value,
                Is.SameAs(completed.Value));
            Assert.That(completed.Value.Result,
                Is.EqualTo(CustomPcQualityReleaseResult.ReadyForPackaging));
            Assert.That(completed.Value.WorkOrder, Is.SameAs(fixture.WorkOrder));
            Assert.That(completed.Value.WorkTicket, Is.SameAs(fixture.WorkTicket));
            Assert.That(completed.Value.WorkOrderId,
                Is.EqualTo(fixture.WorkOrder.Id));
            Assert.That(completed.Value.WorkTicketId,
                Is.EqualTo(fixture.WorkTicket.Id));
            Assert.That(completed.Value.SourceQuoteId,
                Is.EqualTo(fixture.WorkOrder.SourceQuoteId));
            Assert.That(completed.Value.SourceRequestId,
                Is.EqualTo(fixture.WorkOrder.SourceRequestId));
            Assert.That(completed.Value.CustomerBindingId,
                Is.EqualTo(fixture.WorkOrder.CustomerBindingId));
            Assert.That(completed.Value.InventoryClaimId,
                Is.EqualTo(fixture.WorkOrder.InventoryClaimId));
            Assert.That(completed.Value.WorkbenchContainerId,
                Is.EqualTo(fixture.WorkOrder.WorkbenchContainerId));
            Assert.That(completed.Value.SourceValidationReceipt,
                Is.SameAs(fixture.ValidationReceipt));
            Assert.That(completed.Value.SourcePowerOffReceipt,
                Is.SameAs(fixture.PowerOffReceipt));
            Assert.That(completed.Value.SourcePowerOnReceipt,
                Is.SameAs(fixture.ValidationReceipt.SourcePowerOnReceipt));
            Assert.That(completed.Value.SourceElectricalReadiness,
                Is.SameAs(fixture.ValidationReceipt.SourceElectricalReadiness));
            Assert.That(completed.Value.BuildId,
                Is.EqualTo(session.AssemblyBuild.BuildId));
            Assert.That(completed.Value.ChassisId,
                Is.EqualTo(session.AssemblyBuild.ChassisId));
            Assert.That(completed.Value.PerformanceCatalogId,
                Is.EqualTo(fixture.ValidationReceipt.PerformanceCatalogId));
            Assert.That(completed.Value.ValidationProfileId,
                Is.EqualTo(fixture.ValidationReceipt.ProfileId));
            Assert.That(completed.Value.QualityTier,
                Is.EqualTo(fixture.ValidationReceipt.QualityTier));
            Assert.That(completed.Value.BenchmarkScore, Is.EqualTo(401));
            Assert.That(completed.Value.StressSteps, Is.EqualTo(300));
            Assert.That(completed.Value.ProcessorPeakTemperatureCelsius,
                Is.EqualTo(67));
            Assert.That(completed.Value.GraphicsCardPeakTemperatureCelsius,
                Is.EqualTo(64));
            Assert.That(completed.Value.SystemPowerDrawWatts, Is.EqualTo(380));
            Assert.That(completed.Value.PowerMarginWatts, Is.EqualTo(50));
            Assert.That(completed.Value.PowerOffRevision,
                Is.EqualTo(fixture.PowerOffReceipt.Revision));
            Assert.That(completed.Value.ExpectedRevision, Is.Zero);
            Assert.That(completed.Value.Revision, Is.EqualTo(1));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(1));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(1));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcWorkOrders.Revision,
                Is.EqualTo(workOrderRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevision));
            Assert.That(session.PowerState.Revision, Is.EqualTo(powerRevision));
            Assert.That(session.Validation.Revision,
                Is.EqualTo(validationRevision));
            Assert.That(session.Validation.ReceiptCount,
                Is.EqualTo(validationReceiptCount));
            Assert.That(fixture.Authority.ValidateReceiptHistory().IsSuccess,
                Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void QualityReleaseAllowsControlledRerunButBecomesNotCurrentOnNewPowerCycle()
        {
            QualityFixture fixture = PrepareQualityFixture();
            CustomPcQualityReleaseReceipt first = Release(
                fixture,
                "rerun-1");
            CustomPcQualityReleaseReceipt second = Release(
                fixture,
                "rerun-2");

            Assert.That(first.Revision, Is.EqualTo(1));
            Assert.That(second.Revision, Is.EqualTo(2));
            Assert.That(second.OperationId, Is.Not.EqualTo(first.OperationId));
            Assert.That(second.SourceValidationReceipt,
                Is.SameAs(first.SourceValidationReceipt));
            Assert.That(second.SourcePowerOffReceipt,
                Is.SameAs(first.SourcePowerOffReceipt));
            Assert.That(fixture.Authority.EvaluateCurrentRelease().Value,
                Is.SameAs(second));

            PcPowerStateReceipt nextPowerOn = fixture.Session.PowerState.TryPowerOn(
                fixture.Session.CreatePrototypePowerStateOperationId(
                    PcPowerTransitionKind.PowerOn,
                    fixture.Session.PowerState.Revision + 1L),
                fixture.ValidationReceipt.PreflightReceipt,
                fixture.Session.PowerState.Revision).Value;

            Assert.That(nextPowerOn.ResultingState,
                Is.EqualTo(PcPowerState.Energized));
            Assert.That(fixture.Authority.EvaluateCurrentRelease().Error,
                Is.EqualTo(CustomPcQualityReleaseFailures.NotCurrent));
            Assert.That(fixture.Authority.ValidateReceiptHistory().IsSuccess,
                Is.True);
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(2));
            Assert.That(fixture.Session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void QualityReleaseRejectsForeignStaleAndInvalidCommandsWithoutReceipt()
        {
            QualityFixture fixture = PrepareQualityFixture();
            QualityFixture foreign = PrepareQualityFixture();
            StableId<CustomPcQualityReleaseOperationIdScope> operationId =
                QualityOperationId("invalid");

            Assert.That(CustomPcQualityReleaseAuthority.Create(
                    null,
                    fixture.Session.Validation).Error,
                Is.EqualTo(CustomPcQualityReleaseFailures.ConfigurationMissing));
            Assert.That(fixture.Authority.TryReleaseForPackaging(
                    default,
                    fixture.WorkOrder,
                    fixture.WorkTicket,
                    fixture.ValidationReceipt,
                    fixture.PowerOffReceipt,
                    0).Error,
                Is.EqualTo(CustomPcQualityReleaseFailures.InvalidOperationId));
            Assert.That(fixture.Authority.TryReleaseForPackaging(
                    operationId,
                    foreign.WorkOrder,
                    fixture.WorkTicket,
                    fixture.ValidationReceipt,
                    fixture.PowerOffReceipt,
                    0).Error,
                Is.EqualTo(CustomPcQualityReleaseFailures.InvalidWorkOrder));
            Assert.That(fixture.Authority.TryReleaseForPackaging(
                    operationId,
                    fixture.WorkOrder,
                    foreign.WorkTicket,
                    fixture.ValidationReceipt,
                    fixture.PowerOffReceipt,
                    0).Error,
                Is.EqualTo(CustomPcQualityReleaseFailures.InvalidWorkTicket));
            Assert.That(fixture.Authority.TryReleaseForPackaging(
                    operationId,
                    fixture.WorkOrder,
                    fixture.WorkTicket,
                    foreign.ValidationReceipt,
                    fixture.PowerOffReceipt,
                    0).Error,
                Is.EqualTo(
                    CustomPcQualityReleaseFailures.ValidationReceiptInvalid));
            Assert.That(fixture.Authority.TryReleaseForPackaging(
                    operationId,
                    fixture.WorkOrder,
                    fixture.WorkTicket,
                    fixture.ValidationReceipt,
                    fixture.ValidationReceipt.SourcePowerOnReceipt,
                    0).Error,
                Is.EqualTo(CustomPcQualityReleaseFailures.SafePowerOffMissing));
            Assert.That(fixture.Authority.TryReleaseForPackaging(
                    operationId,
                    fixture.WorkOrder,
                    fixture.WorkTicket,
                    fixture.ValidationReceipt,
                    fixture.PowerOffReceipt,
                    1).Error,
                Is.EqualTo(CustomPcQualityReleaseFailures.RevisionMismatch));
            Assert.That(fixture.Authority.Revision, Is.Zero);
            Assert.That(fixture.Authority.ReceiptCount, Is.Zero);
            Assert.That(fixture.Authority.ValidateReceiptHistory().IsSuccess,
                Is.True);
        }

        [Test]
        public void QualityReleaseRejectsAssemblyDriftAfterValidationAndSafeShutdown()
        {
            QualityFixture fixture = PrepareQualityFixture();
            ElectricalReadinessSnapshot source =
                fixture.ValidationReceipt.SourceElectricalReadiness;
            OperationResult<PcieGpuPowerCableOperationReceipt> drift =
                fixture.Session.AssemblyBuild.UnroutePcieGpuPowerCable(
                    StableId<AssemblyOperationIdScope>.Parse(
                        "assembly.operation.issue137.unroute-pcie"),
                    source.PcieGpuPowerCableItemId,
                    source.PcieGpuRouteOperationId,
                    fixture.Session.AssemblyBuild.PcieGpuPowerCableRevision);

            Assert.That(drift.IsSuccess, Is.True, drift.Error.Code);
            OperationResult<CustomPcQualityReleaseReceipt> result =
                fixture.Authority.TryReleaseForPackaging(
                    QualityOperationId("assembly-drift"),
                    fixture.WorkOrder,
                    fixture.WorkTicket,
                    fixture.ValidationReceipt,
                    fixture.PowerOffReceipt,
                    fixture.Authority.Revision);

            Assert.That(result.Error,
                Is.EqualTo(CustomPcQualityReleaseFailures.AssemblyDrift));
            Assert.That(fixture.Authority.Revision, Is.Zero);
            Assert.That(fixture.Authority.ReceiptCount, Is.Zero);
            Assert.That(fixture.Authority.ValidateReceiptHistory().IsSuccess,
                Is.True);
            Assert.That(fixture.Session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void QualityReleaseHistoryTamperFailsClosedBeforeReplay()
        {
            QualityFixture fixture = PrepareQualityFixture();
            CustomPcQualityReleaseReceipt receipt = Release(fixture, "history");
            System.Reflection.FieldInfo field = receipt.GetType().GetField(
                "<SourceElectricalReadiness>k__BackingField",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            object original = field.GetValue(receipt);
            field.SetValue(receipt, null);

            Assert.That(fixture.Authority.ValidateReceiptHistory().Error,
                Is.EqualTo(
                    CustomPcQualityReleaseFailures.ReceiptHistoryInvalid));
            Assert.That(fixture.Session.ValidateInvariants().Error,
                Is.EqualTo(
                    CustomPcQualityReleaseFailures.ReceiptHistoryInvalid));
            Assert.That(fixture.Authority.TryReleaseForPackaging(
                    receipt.OperationId,
                    receipt.WorkOrder,
                    receipt.WorkTicket,
                    receipt.SourceValidationReceipt,
                    receipt.SourcePowerOffReceipt,
                    receipt.ExpectedRevision).Error,
                Is.EqualTo(
                    CustomPcQualityReleaseFailures.ReceiptHistoryInvalid));

            field.SetValue(receipt, original);
            Assert.That(fixture.Authority.ValidateReceiptHistory().IsSuccess,
                Is.True);
            Assert.That(fixture.Session.ValidateInvariants().IsSuccess,
                Is.True);
            Assert.That(fixture.Authority.EvaluateCurrentRelease().Value,
                Is.SameAs(receipt));
        }

        private static QualityFixture PrepareQualityFixture()
        {
            GarageStockFlowSession session = PreparePowerBudgetReadySession();
            Assert.That(session.TryGetPrototypeCustomPcBuildOrder(
                out CustomPcBuildOrderRecord workOrder), Is.True);
            Assert.That(session.TryGetPrototypeCustomPcWorkTicket(
                out CustomPcWorkTicketRecord workTicket), Is.True);
            PcFictionalDriverInstallationReceipt driver =
                CompleteCurrentFictionalDriverInstallation(
                    session,
                    out _,
                    out PcFirmwareBaselineReceipt firmware,
                    out PcPowerStateReceipt powerOn);
            PcValidationReceipt validationReceipt = CompleteValidation(
                session.Validation,
                session,
                driver,
                firmware,
                "issue137-quality");
            PcPowerStateReceipt powerOffReceipt = session.PowerState.TryPowerOff(
                session.CreatePrototypePowerStateOperationId(
                    PcPowerTransitionKind.PowerOff,
                    session.PowerState.Revision + 1L),
                powerOn,
                session.PowerState.Revision).Value;
            OperationResult<CustomPcQualityReleaseAuthority> created =
                session.EnsureQualityReleaseAuthority();
            Assert.That(created.IsSuccess, Is.True, created.Error.Code);
            return new QualityFixture(
                session,
                workOrder,
                workTicket,
                validationReceipt,
                powerOffReceipt,
                created.Value);
        }

        private static CustomPcQualityReleaseReceipt Release(
            QualityFixture fixture,
            string suffix)
        {
            OperationResult<CustomPcQualityReleaseReceipt> result =
                fixture.Authority.TryReleaseForPackaging(
                    QualityOperationId(suffix),
                    fixture.WorkOrder,
                    fixture.WorkTicket,
                    fixture.ValidationReceipt,
                    fixture.PowerOffReceipt,
                    fixture.Authority.Revision);
            Assert.That(result.IsSuccess, Is.True, result.Error.Code);
            return result.Value;
        }

        private static StableId<CustomPcQualityReleaseOperationIdScope>
            QualityOperationId(string suffix)
        {
            return StableId<CustomPcQualityReleaseOperationIdScope>.Parse(
                "quality.custom-pc-release.issue137." + suffix);
        }

        private sealed class QualityFixture
        {
            internal QualityFixture(
                GarageStockFlowSession session,
                CustomPcBuildOrderRecord workOrder,
                CustomPcWorkTicketRecord workTicket,
                PcValidationReceipt validationReceipt,
                PcPowerStateReceipt powerOffReceipt,
                CustomPcQualityReleaseAuthority authority)
            {
                Session = session;
                WorkOrder = workOrder;
                WorkTicket = workTicket;
                ValidationReceipt = validationReceipt;
                PowerOffReceipt = powerOffReceipt;
                Authority = authority;
            }

            internal GarageStockFlowSession Session { get; }
            internal CustomPcBuildOrderRecord WorkOrder { get; }
            internal CustomPcWorkTicketRecord WorkTicket { get; }
            internal PcValidationReceipt ValidationReceipt { get; }
            internal PcPowerStateReceipt PowerOffReceipt { get; }
            internal CustomPcQualityReleaseAuthority Authority { get; }
        }
    }
}
