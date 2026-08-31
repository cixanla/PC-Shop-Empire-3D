using PCShopEmpire3D.Presentation.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation
{
    public sealed partial class GaragePrototypeHud
    {
        [SerializeField]
        private CustomPcPackagingStationProjection customPcPackagingStation;
        [SerializeField]
        private CustomPcPackageDispatchProjection customPcPackageDispatch;

        public CustomPcPackagingStationProjection CustomPcPackagingStation =>
            customPcPackagingStation;

        public CustomPcPackageDispatchProjection CustomPcPackageDispatch =>
            customPcPackageDispatch;

        public void ConfigureCustomPcPackage(
            CustomPcPackagingStationProjection packagingStation,
            CustomPcPackageDispatchProjection dispatchProjection)
        {
            customPcPackagingStation = packagingStation;
            customPcPackageDispatch = dispatchProjection;
        }

        private string ResolveCustomPcPackagePrompt()
        {
            string prompt = customPcPackageDispatch != null
                ? customPcPackageDispatch.PromptText
                : string.Empty;
            return string.IsNullOrEmpty(prompt) &&
                   customPcPackagingStation != null
                ? customPcPackagingStation.PromptText
                : prompt;
        }
    }
}
