using PCShopEmpire3D.Presentation.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation
{
    public sealed partial class GaragePrototypeMarker
    {
        [SerializeField]
        private ElectricalReadinessWorkbenchProjection
            electricalReadinessWorkbench;

        public ElectricalReadinessWorkbenchProjection ElectricalReadinessWorkbench =>
            electricalReadinessWorkbench;

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

        private void ConfigureElectricalReadinessWorkbench(
            ElectricalReadinessWorkbenchProjection projection)
        {
            electricalReadinessWorkbench = projection;
        }
    }
}
