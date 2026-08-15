using System;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlacementSurface))]
    public sealed class InventoryPlacementZone : MonoBehaviour
    {
        [SerializeField] private string containerId = GarageStockFlowSession.ShelfContainerIdValue;
        [SerializeField] private InventoryContainerKind containerKind = InventoryContainerKind.Shelf;
        [SerializeField] private string displayName = "RAF A";
        [SerializeField] private PlacementSurface placementSurface;

        public StableId<ContainerIdScope> ContainerId =>
            StableId<ContainerIdScope>.Parse(containerId);

        public InventoryContainerKind ContainerKind => containerKind;

        public string DisplayName => displayName;

        public PlacementSurface PlacementSurface => placementSurface;

        public void Configure(
            string stableContainerId,
            InventoryContainerKind kind,
            string playerFacingName,
            PlacementSurface surface)
        {
            containerId = StableId<ContainerIdScope>.Parse(stableContainerId).Value;
            if (!Enum.IsDefined(typeof(InventoryContainerKind), kind))
            {
                throw new ArgumentOutOfRangeException(nameof(kind), kind, "A known container kind is required.");
            }

            containerKind = kind;
            displayName = string.IsNullOrWhiteSpace(playerFacingName)
                ? throw new ArgumentException("A display name is required.", nameof(playerFacingName))
                : playerFacingName.Trim();
            placementSurface = surface != null
                ? surface
                : throw new ArgumentNullException(nameof(surface));
        }

        private void Awake()
        {
            placementSurface ??= GetComponent<PlacementSurface>();
            _ = ContainerId;
        }

        private void OnValidate()
        {
            placementSurface ??= GetComponent<PlacementSurface>();
        }
    }
}
