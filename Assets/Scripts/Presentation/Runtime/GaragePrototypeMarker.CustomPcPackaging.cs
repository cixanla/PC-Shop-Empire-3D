using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation
{
    public sealed partial class GaragePrototypeMarker
    {
        [SerializeField]
        private CustomPcPackagingStationProjection customPcPackagingStation;
        [SerializeField]
        private CustomPcPackageDispatchProjection customPcPackageDispatch;
        [SerializeField]
        private CustomPcPackagePhysicalBinding customPcPackageBinding;
        [SerializeField]
        private PhysicalItemProjection customPcPackage;

        public CustomPcPackagingStationProjection CustomPcPackagingStation =>
            customPcPackagingStation;

        public CustomPcPackageDispatchProjection CustomPcPackageDispatch =>
            customPcPackageDispatch;

        public CustomPcPackagePhysicalBinding CustomPcPackageBinding =>
            customPcPackageBinding;

        public PhysicalItemProjection CustomPcPackage => customPcPackage;

        public bool HasCustomPcPackagingR68Runtime =>
            customPcPackagingStation != null &&
            customPcPackagingStation.StockFlow == stockFlow &&
            customPcPackagingStation.PlayerInput == playerInput &&
            customPcPackagingStation.PlayerMotor == playerMotor &&
            customPcPackagingStation.PlayerCarry == playerCarry &&
            customPcPackageDispatch != null &&
            customPcPackageDispatch.PlayerInput == playerInput &&
            customPcPackageDispatch.PlayerMotor == playerMotor &&
            customPcPackageDispatch.PlayerCarry == playerCarry &&
            customPcPackageBinding != null &&
            customPcPackageBinding.StockFlow == stockFlow &&
            customPcPackageBinding.PackageItem == customPcPackage &&
            customPcPackageBinding.SourceProjections.Count ==
                CustomPcPackagePhysicalBinding.RequiredSourceProjectionCount &&
            customPcPackage != null &&
            customPcPackage.CarryProfile == PhysicalCarryProfile.LargeBox &&
            customPcPackage.ItemIdValue ==
                GarageStockFlowSession.PrototypeCustomPcPackageIdValue &&
            customPcPackagingStation.PackageBinding == customPcPackageBinding &&
            customPcPackageDispatch.PackageBinding == customPcPackageBinding;

        public void ConfigureCustomPcPackaging(
            CustomPcPackagingStationProjection packagingStation,
            CustomPcPackageDispatchProjection dispatchProjection,
            CustomPcPackagePhysicalBinding packageBinding,
            PhysicalItemProjection packageProjection)
        {
            customPcPackagingStation = packagingStation;
            customPcPackageDispatch = dispatchProjection;
            customPcPackageBinding = packageBinding;
            customPcPackage = packageProjection;
        }
    }
}
