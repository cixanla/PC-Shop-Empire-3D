using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation
{
    public sealed partial class GaragePrototypeMarker
    {
        public const string Eps12vPowerCableR31Marker =
            Eps12vPowerCableRuntimeGeometry.RuntimeMarker;

        [SerializeField] private Eps12vPowerCableRouteProjection eps12vPowerCableRoute;
        [SerializeField] private Eps12vPowerCableAssemblyItemBinding eps12vPowerCableBinding;
        [SerializeField] private PhysicalItemProjection eps12vPowerCable;
        [SerializeField] private Eps12vPowerCableRuntimeGeometry eps12vPowerCableGeometry;

        public Eps12vPowerCableRouteProjection Eps12vPowerCableRoute =>
            eps12vPowerCableRoute;

        public Eps12vPowerCableAssemblyItemBinding Eps12vPowerCableBinding =>
            eps12vPowerCableBinding;

        public PhysicalItemProjection Eps12vPowerCable => eps12vPowerCable;

        public Eps12vPowerCableRuntimeGeometry Eps12vPowerCableGeometry =>
            eps12vPowerCableGeometry;

        public bool HasEps12vPowerCableR31Runtime =>
            eps12vPowerCableGeometry != null &&
            eps12vPowerCableGeometry.IsCanonical &&
            eps12vPowerCableRoute != null &&
            eps12vPowerCableRoute.IsConfigured &&
            eps12vPowerCableBinding != null &&
            eps12vPowerCableBinding.Route == eps12vPowerCableRoute &&
            eps12vPowerCableBinding.PhysicalItem == eps12vPowerCable &&
            eps12vPowerCableBinding.Geometry == eps12vPowerCableGeometry &&
            eps12vPowerCable != null &&
            eps12vPowerCableRoute.FocusCollider.isTrigger &&
            eps12vPowerCableRoute.FocusCollider.gameObject.layer ==
                LayerMask.NameToLayer("Interactable") &&
            eps12vPowerCableRoute.Waypoints.Length == 3 &&
            CountCanonicalEps12vPowerCableProjections(
                GarageStockFlowSession.Eps12vPowerCableItemInstanceIdValue) == 1 &&
            FindObjectsByType<Eps12vPowerCableRuntimeGeometry>(
                FindObjectsSortMode.None).Length == 1;

        private void ConfigureEps12vPowerCable(
            Eps12vPowerCableRouteProjection physicalRoute,
            Eps12vPowerCableAssemblyItemBinding physicalBinding,
            PhysicalItemProjection physicalCable,
            Eps12vPowerCableRuntimeGeometry physicalGeometry)
        {
            eps12vPowerCableRoute = physicalRoute;
            eps12vPowerCableBinding = physicalBinding;
            eps12vPowerCable = physicalCable;
            eps12vPowerCableGeometry = physicalGeometry;
        }

        private static int CountCanonicalEps12vPowerCableProjections(
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
