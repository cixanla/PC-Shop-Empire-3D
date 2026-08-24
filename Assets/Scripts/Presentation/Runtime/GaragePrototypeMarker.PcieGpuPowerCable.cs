using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation
{
    public sealed partial class GaragePrototypeMarker
    {
        public const string PcieGpuPowerCableR32Marker =
            PcieGpuPowerCableRuntimeGeometry.RuntimeMarker;

        [SerializeField] private PcieGpuPowerCableRouteProjection pcieGpuPowerCableRoute;
        [SerializeField] private PcieGpuPowerCableAssemblyItemBinding pcieGpuPowerCableBinding;
        [SerializeField] private PhysicalItemProjection pcieGpuPowerCable;
        [SerializeField] private PcieGpuPowerCableRuntimeGeometry pcieGpuPowerCableGeometry;

        public PcieGpuPowerCableRouteProjection PcieGpuPowerCableRoute =>
            pcieGpuPowerCableRoute;

        public PcieGpuPowerCableAssemblyItemBinding PcieGpuPowerCableBinding =>
            pcieGpuPowerCableBinding;

        public PhysicalItemProjection PcieGpuPowerCable => pcieGpuPowerCable;

        public PcieGpuPowerCableRuntimeGeometry PcieGpuPowerCableGeometry =>
            pcieGpuPowerCableGeometry;

        public bool HasPcieGpuPowerCableR32Runtime =>
            pcieGpuPowerCableGeometry != null &&
            pcieGpuPowerCableGeometry.IsCanonical &&
            pcieGpuPowerCableRoute != null &&
            pcieGpuPowerCableRoute.IsConfigured &&
            pcieGpuPowerCableBinding != null &&
            pcieGpuPowerCableBinding.Route == pcieGpuPowerCableRoute &&
            pcieGpuPowerCableBinding.PhysicalItem == pcieGpuPowerCable &&
            pcieGpuPowerCableBinding.Geometry == pcieGpuPowerCableGeometry &&
            pcieGpuPowerCable != null &&
            pcieGpuPowerCableRoute.FocusCollider.isTrigger &&
            pcieGpuPowerCableRoute.FocusCollider.gameObject.layer ==
                LayerMask.NameToLayer("Interactable") &&
            pcieGpuPowerCableRoute.Waypoints.Length == 3 &&
            CountCanonicalPcieGpuPowerCableProjections(
                GarageStockFlowSession.PcieGpuPowerCableItemInstanceIdValue) == 1 &&
            FindObjectsByType<PcieGpuPowerCableRuntimeGeometry>(
                FindObjectsSortMode.None).Length == 1;

        private void ConfigurePcieGpuPowerCable(
            PcieGpuPowerCableRouteProjection physicalRoute,
            PcieGpuPowerCableAssemblyItemBinding physicalBinding,
            PhysicalItemProjection physicalCable,
            PcieGpuPowerCableRuntimeGeometry physicalGeometry)
        {
            pcieGpuPowerCableRoute = physicalRoute;
            pcieGpuPowerCableBinding = physicalBinding;
            pcieGpuPowerCable = physicalCable;
            pcieGpuPowerCableGeometry = physicalGeometry;
        }

        private static int CountCanonicalPcieGpuPowerCableProjections(
            string canonicalItemId)
        {
            int count = 0;
            foreach (PhysicalItemProjection item in
                     FindObjectsByType<PhysicalItemProjection>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                if (item != null && item.ItemIdValue == canonicalItemId)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
