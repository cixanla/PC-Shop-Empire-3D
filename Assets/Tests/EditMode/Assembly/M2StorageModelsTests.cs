using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Tests.EditMode.Assembly
{
    public sealed class M2StorageModelsTests
    {
        private static readonly StableId<AssemblySlotIdScope> SlotId =
            StableId<AssemblySlotIdScope>.Parse("assembly.slot.m2-primary");
        private static readonly StableId<AssemblyStorageStandoffIdScope> StandoffId =
            StableId<AssemblyStorageStandoffIdScope>.Parse("assembly.standoff.m2-2280");
        private static readonly StableId<AssemblyRetentionIdScope> CaptiveScrewId =
            StableId<AssemblyRetentionIdScope>.Parse("assembly.retention.m2-captive-screw");
        private static readonly StableId<ContainerIdScope> ContainerId =
            StableId<ContainerIdScope>.Parse("container.storage-slot-m2-primary");

        [Test]
        public void PersistedStorageStateAndOrientationValuesAreExplicit()
        {
            Assert.That((int)StorageSlotState.Unsupported, Is.Zero);
            Assert.That((int)StorageSlotState.EmptyOpen, Is.EqualTo(1));
            Assert.That((int)StorageSlotState.StorageDeviceSeatedUnsecured, Is.EqualTo(2));
            Assert.That((int)StorageSlotState.StorageDeviceSecured, Is.EqualTo(3));
            Assert.That((int)M2KeyOrientation.KeyAligned, Is.EqualTo(1));
            Assert.That((int)M2KeyOrientation.Reversed, Is.EqualTo(2));
        }

        [Test]
        public void M2SlotDefinitionKeepsStableSlotStandoffScrewAndContainerIdentity()
        {
            OperationResult<M2SlotDefinition> result = M2SlotDefinition.Create(
                SlotId,
                StandoffId,
                CaptiveScrewId,
                ContainerId,
                M2StorageType.NvmePcie4X4_2280);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.IsValid, Is.True);
            Assert.That(result.Value.SlotId, Is.EqualTo(SlotId));
            Assert.That(result.Value.StandoffId, Is.EqualTo(StandoffId));
            Assert.That(result.Value.CaptiveScrewId, Is.EqualTo(CaptiveScrewId));
            Assert.That(result.Value.ContainerId, Is.EqualTo(ContainerId));
            Assert.That(result.Value.SupportedStorageType,
                Is.EqualTo(M2StorageType.NvmePcie4X4_2280));
            Assert.That(default(M2SlotDefinition).IsValid, Is.False);
        }

        [Test]
        public void M2SlotDefinitionRejectsInvalidTopologyInDeterministicOrder()
        {
            Assert.That(M2SlotDefinition.Create(
                    default,
                    default,
                    default,
                    default,
                    default).Error,
                Is.EqualTo(AssemblyFailures.InvalidSlotId));
            Assert.That(M2SlotDefinition.Create(
                    SlotId,
                    default,
                    default,
                    default,
                    default).Error,
                Is.EqualTo(AssemblyFailures.InvalidStorageStandoff));
            Assert.That(M2SlotDefinition.Create(
                    SlotId,
                    StandoffId,
                    default,
                    default,
                    default).Error,
                Is.EqualTo(AssemblyFailures.InvalidRetention));
            Assert.That(M2SlotDefinition.Create(
                    SlotId,
                    StandoffId,
                    CaptiveScrewId,
                    default,
                    default).Error,
                Is.EqualTo(AssemblyFailures.InvalidStorageSlotContainer));
            Assert.That(M2SlotDefinition.Create(
                    SlotId,
                    StandoffId,
                    CaptiveScrewId,
                    ContainerId,
                    (M2StorageType)99).Error,
                Is.EqualTo(AssemblyFailures.InvalidM2StorageType));
        }

        [Test]
        public void StorageCompatibilityRequiresExactKindTypeAndKeyOrientation()
        {
            ProductCatalog products = CreateProducts();
            PcComponentSpecification motherboard = PcComponentSpecification.CreateMotherboard(
                products,
                ProductId("component.motherboard-matx"),
                MotherboardFormFactor.MicroAtx,
                CpuSocketFamily.Lga1700,
                DimmType.Ddr5Udimm,
                M2StorageType.NvmePcie4X4_2280).Value;
            PcComponentSpecification storage = PcComponentSpecification.CreateStorageDevice(
                products,
                ProductId("component.storage-nvme-2280"),
                M2StorageType.NvmePcie4X4_2280).Value;

            AssemblyCompatibilityResult compatible =
                AssemblyCompatibilityEvaluator.EvaluateStorageDeviceSeat(
                    storage,
                    motherboard,
                    M2StorageType.NvmePcie4X4_2280,
                    M2KeyOrientation.KeyAligned);
            Assert.That(compatible.IsCompatible, Is.True);
            Assert.That(compatible.Reason, Is.EqualTo(Failure.None));

            Assert.That(AssemblyCompatibilityEvaluator.EvaluateStorageDeviceSeat(
                    storage,
                    motherboard,
                    M2StorageType.NvmePcie4X4_2280,
                    M2KeyOrientation.Reversed).Reason,
                Is.EqualTo(AssemblyFailures.M2OrientationMismatch));
            Assert.That(AssemblyCompatibilityEvaluator.EvaluateStorageDeviceSeat(
                    storage,
                    motherboard,
                    M2StorageType.NvmePcie4X4_2280,
                    default).Reason,
                Is.EqualTo(AssemblyFailures.InvalidM2Orientation));
            Assert.That(AssemblyCompatibilityEvaluator.EvaluateStorageDeviceSeat(
                    null,
                    motherboard,
                    M2StorageType.NvmePcie4X4_2280,
                    M2KeyOrientation.KeyAligned).Reason,
                Is.EqualTo(AssemblyFailures.UnknownComponentSpecification));

            PcComponentSpecification legacyMotherboard =
                PcComponentSpecification.CreateMotherboard(
                    products,
                    ProductId("component.motherboard-matx"),
                    MotherboardFormFactor.MicroAtx,
                    CpuSocketFamily.Lga1700).Value;
            Assert.That(AssemblyCompatibilityEvaluator.EvaluateStorageDeviceSeat(
                    storage,
                    legacyMotherboard,
                    M2StorageType.NvmePcie4X4_2280,
                    M2KeyOrientation.KeyAligned).Reason,
                Is.EqualTo(AssemblyFailures.M2StorageTypeMismatch));
        }

        private static ProductCatalog CreateProducts()
        {
            return ProductCatalog.Create(new[]
            {
                Definition("component.motherboard-matx"),
                Definition("component.storage-nvme-2280")
            }).Value;
        }

        private static ProductDefinition Definition(string id)
        {
            return ProductDefinition.Create(
                ProductId(id),
                StableId<ProductCategoryIdScope>.Parse("pc-components"),
                id,
                ProductTrackingPolicy.SerializedInstance,
                730).Value;
        }

        private static StableId<ProductDefinitionIdScope> ProductId(string value)
        {
            return StableId<ProductDefinitionIdScope>.Parse(value);
        }
    }
}
