using System;
using UnityEngine;

namespace PCShopEmpire3D.World.Interaction
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class PlacementSurface : MonoBehaviour
    {
        [SerializeField] private string surfaceId = "prototype.placement-surface";
        [SerializeField] private Collider surfaceCollider;
        [SerializeField] private float gridSize = 0.25f;
        [SerializeField] private float yawStepDegrees = 90f;

        public string SurfaceId => surfaceId;

        public Collider SurfaceCollider => surfaceCollider;

        public float GridSize => gridSize;

        public float YawStepDegrees => yawStepDegrees;

        public void Configure(
            string stableSurfaceId,
            Collider placementCollider,
            float positionGridSize,
            float rotationStepDegrees)
        {
            surfaceId = string.IsNullOrWhiteSpace(stableSurfaceId)
                ? throw new ArgumentException("A placement surface ID is required.", nameof(stableSurfaceId))
                : stableSurfaceId.Trim();
            surfaceCollider = placementCollider != null
                ? placementCollider
                : throw new ArgumentNullException(nameof(placementCollider));
            gridSize = Mathf.Max(0.01f, positionGridSize);
            yawStepDegrees = Mathf.Clamp(rotationStepDegrees, 1f, 360f);
        }

        public Vector3 SnapPoint(Vector3 worldPoint)
        {
            Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
            localPoint.x = Mathf.Round(localPoint.x / gridSize) * gridSize;
            localPoint.z = Mathf.Round(localPoint.z / gridSize) * gridSize;
            return transform.TransformPoint(localPoint);
        }

        public Quaternion SnapRotation(Quaternion requestedWorldRotation)
        {
            float surfaceYaw = transform.eulerAngles.y;
            float relativeYaw = Mathf.DeltaAngle(surfaceYaw, requestedWorldRotation.eulerAngles.y);
            float snappedRelativeYaw = Mathf.Round(relativeYaw / yawStepDegrees) * yawStepDegrees;
            return Quaternion.Euler(0f, surfaceYaw + snappedRelativeYaw, 0f);
        }

        private void Awake()
        {
            EnsureContract();
        }

        private void OnValidate()
        {
            surfaceCollider ??= GetComponent<Collider>();
            gridSize = Mathf.Max(0.01f, gridSize);
            yawStepDegrees = Mathf.Clamp(yawStepDegrees, 1f, 360f);
        }

        private void EnsureContract()
        {
            surfaceCollider ??= GetComponent<Collider>();
            if (surfaceCollider == null)
            {
                throw new InvalidOperationException("A placement surface requires a Collider.");
            }

            if (string.IsNullOrWhiteSpace(surfaceId))
            {
                throw new InvalidOperationException("A placement surface ID is required.");
            }
        }
    }
}
