using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    /// <summary>Small explicit r27 smoke marker queried by automated runtime routes.</summary>
    [DisallowMultipleComponent]
    public sealed class ProcessorCoolerRuntimeSmokeMarker : MonoBehaviour
    {
        public const string Marker = "r27.processor-cooler.runtime-smoke";
        [SerializeField] private ProcessorCoolerRuntimeGeometry geometry;
        [SerializeField] private ProcessorCoolerSlotProjection slot;
        [SerializeField] private ProcessorCoolerAssemblyItemBinding binding;
        public bool IsReady =>
            geometry != null &&
            geometry.IsCanonical &&
            geometry.transform == transform &&
            slot != null &&
            slot.IsConfigured &&
            binding != null &&
            binding.transform == transform &&
            binding.Slot == slot &&
            binding.PhysicalItem != null &&
            binding.PhysicalItem.transform == transform &&
            binding.InventoryItemIdValue ==
                GarageStockFlowSession.ProcessorCoolerItemInstanceIdValue &&
            binding.PhysicalItem.ItemIdValue ==
                GarageStockFlowSession.ProcessorCoolerItemInstanceIdValue;

        public void Configure(
            ProcessorCoolerRuntimeGeometry runtimeGeometry,
            ProcessorCoolerSlotProjection slotProjection,
            ProcessorCoolerAssemblyItemBinding itemBinding)
        {
            geometry = runtimeGeometry != null
                ? runtimeGeometry
                : throw new System.ArgumentNullException(nameof(runtimeGeometry));
            slot = slotProjection != null
                ? slotProjection
                : throw new System.ArgumentNullException(nameof(slotProjection));
            binding = itemBinding != null
                ? itemBinding
                : throw new System.ArgumentNullException(nameof(itemBinding));
        }
    }
}
