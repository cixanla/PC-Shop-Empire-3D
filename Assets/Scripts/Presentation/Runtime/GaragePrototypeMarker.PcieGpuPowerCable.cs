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
        [SerializeField]
        private PcieGpuPowerCableBuildKitProjection pcieGpuPowerCableBuildKit;

        public PcieGpuPowerCableRouteProjection PcieGpuPowerCableRoute =>
            pcieGpuPowerCableRoute;

        public PcieGpuPowerCableAssemblyItemBinding PcieGpuPowerCableBinding =>
            pcieGpuPowerCableBinding;

        public PhysicalItemProjection PcieGpuPowerCable => pcieGpuPowerCable;

        public PcieGpuPowerCableRuntimeGeometry PcieGpuPowerCableGeometry =>
            pcieGpuPowerCableGeometry;

        public PcieGpuPowerCableBuildKitProjection PcieGpuPowerCableBuildKit =>
            pcieGpuPowerCableBuildKit;

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

        public bool HasPcieGpuPowerCableBuildKitR44Runtime =>
            pcieGpuPowerCableBuildKit != null &&
            pcieGpuPowerCableBuildKit.IsCanonical &&
            pcieGpuPowerCableBuildKit.Runtime == stockFlow &&
            pcieGpuPowerCableBinding != null &&
            pcieGpuPowerCableBinding.PhysicalItem == pcieGpuPowerCable &&
            pcieGpuPowerCableBinding.Route == pcieGpuPowerCableRoute &&
            pcieGpuPowerCableBinding.MatchesBuildKitConfiguration(
                pcieGpuPowerCableBuildKit) &&
            pcieGpuPowerCable != null &&
            pcieGpuPowerCableRoute != null &&
            playerCarry != null &&
            playerCarry.MatchesPcieGpuPowerCableBuildKitConfiguration(
                pcieGpuPowerCableBuildKit,
                pcieGpuPowerCableBinding) &&
            FindObjectsByType<PcieGpuPowerCableBuildKitProjection>(
                FindObjectsSortMode.None).Length == 1;

        public bool HasPcieGpuPowerCableAssemblyHandoffR54Runtime =>
            HasEps12vPowerCableAssemblyHandoffR53Runtime &&
            HasPcieGpuPowerCableR32Runtime &&
            HasPcieGpuPowerCableBuildKitR44Runtime &&
            pcieGpuPowerCableBinding != null &&
            pcieGpuPowerCableBinding.BuildKit == pcieGpuPowerCableBuildKit &&
            pcieGpuPowerCableBinding.Route == pcieGpuPowerCableRoute &&
            pcieGpuPowerCableBinding.PhysicalItem == pcieGpuPowerCable &&
            pcieGpuPowerCableBinding.Geometry == pcieGpuPowerCableGeometry &&
            graphicsCardSlot != null &&
            ResolveAtx24ChassisCablePassThroughRoot() is Transform
                chassisCablePassThroughRoot &&
            chassisCablePassThroughRoot.GetComponent<Collider>() is Collider
                chassisCablePassThroughCollider &&
            pcieGpuPowerCableRoute.MatchesInstalledAssemblyColliders(
                chassisCablePassThroughCollider,
                graphicsCardSlot.SupportCollider) &&
            playerCarry != null &&
            playerCarry.MatchesPcieGpuPowerCableConfiguration(
                pcieGpuPowerCableRoute,
                pcieGpuPowerCableBinding) &&
            stockFlow != null &&
            stockFlow.Session != null &&
            stockFlow.Session.PrototypePcieGpuPowerCableAssemblyHandoffOperationId
                .Value !=
                stockFlow.Session.PrototypePcieGpuPowerCableBuildKitOperationId.Value &&
            stockFlow.Session.PrototypePcieGpuPowerCableAssemblyHandoffOperationId
                .Value !=
                stockFlow.Session.PrototypeEps12vPowerCableAssemblyHandoffOperationId
                    .Value &&
            stockFlow.Session.PcieGpuPowerCableRouteContainerId !=
                stockFlow.Session.PcieGpuPowerCableBuildKitContainerId &&
            FindObjectsByType<PcieGpuPowerCableAssemblyItemBinding>(
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
