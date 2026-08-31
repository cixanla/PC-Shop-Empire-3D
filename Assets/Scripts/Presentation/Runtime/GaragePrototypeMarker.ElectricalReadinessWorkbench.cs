using PCShopEmpire3D.Presentation.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation
{
    public sealed partial class GaragePrototypeMarker
    {
        [SerializeField]
        private ElectricalReadinessWorkbenchProjection
            electricalReadinessWorkbench;
        [SerializeField]
        private ElectricalPowerTestStationProjection electricalPowerTestStation;

        public ElectricalReadinessWorkbenchProjection ElectricalReadinessWorkbench =>
            electricalReadinessWorkbench;

        public ElectricalPowerTestStationProjection ElectricalPowerTestStation =>
            electricalPowerTestStation;

        public bool HasPowerBudgetWorkbenchR59Runtime =>
            electricalReadinessWorkbench != null &&
            electricalReadinessWorkbench.IsConfigured &&
            electricalReadinessWorkbench.Runtime == stockFlow &&
            electricalReadinessWorkbench.StatusText != null &&
            electricalReadinessWorkbench.StatusIndicator != null &&
            electricalReadinessWorkbench.GetComponentsInChildren<Collider>(true)
                .Length == 0 &&
            FindObjectsByType<ElectricalReadinessWorkbenchProjection>(
                FindObjectsSortMode.None).Length == 1;

        public bool HasElectricalReadinessWorkbenchR58Runtime =>
            HasPowerBudgetWorkbenchR59Runtime;

        public bool HasPowerTestPreflightR60Runtime =>
            HasPowerBudgetWorkbenchR59Runtime &&
            electricalPowerTestStation != null &&
            electricalPowerTestStation.IsConfigured &&
            electricalPowerTestStation.StockFlow == stockFlow &&
            electricalPowerTestStation.ReadinessProjection ==
                electricalReadinessWorkbench &&
            electricalPowerTestStation.FocusAnchor ==
                electricalReadinessWorkbench.StatusText.transform &&
            electricalPowerTestStation.GetComponentsInChildren<Collider>(true)
                .Length == 0 &&
            FindObjectsByType<ElectricalPowerTestStationProjection>(
                FindObjectsSortMode.None).Length == 1;

        private void ConfigureElectricalReadinessWorkbench(
            ElectricalReadinessWorkbenchProjection projection,
            ElectricalPowerTestStationProjection powerTestStation)
        {
            electricalReadinessWorkbench = projection;
            electricalPowerTestStation = powerTestStation;
        }
    }
}
