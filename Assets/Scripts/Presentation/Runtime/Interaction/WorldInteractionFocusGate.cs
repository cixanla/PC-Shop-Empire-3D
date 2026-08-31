using PCShopEmpire3D.Core.Primitives;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public static class WorldInteractionFocusFailures
    {
        public static readonly Failure ConfigurationMissing = Failure.FromCode(
            "world-interaction-focus.configuration-missing");
        public static readonly Failure OutOfRange = Failure.FromCode(
            "world-interaction-focus.out-of-range");
        public static readonly Failure FocusMissing = Failure.FromCode(
            "world-interaction-focus.focus-missing");
        public static readonly Failure LineOfSightBlocked = Failure.FromCode(
            "world-interaction-focus.line-of-sight-blocked");
    }

    /// <summary>
    /// Shared non-allocating focus, range and line-of-sight gate for fixed world
    /// stations. It deliberately ignores the player's own hierarchy and the
    /// Ignore Raycast layer, but no intervening world geometry.
    /// </summary>
    public static class WorldInteractionFocusGate
    {
        private const int HitCapacity = 24;
        private static readonly RaycastHit[] Hits = new RaycastHit[HitCapacity];

        public static OperationResult Evaluate(
            Camera camera,
            Collider target,
            float interactionRange,
            float focusDegrees,
            Transform ignoredPlayerRoot)
        {
            if (camera == null || target == null || interactionRange <= 0f ||
                focusDegrees <= 0f)
            {
                return OperationResult.Fail(
                    WorldInteractionFocusFailures.ConfigurationMissing);
            }

            Vector3 origin = camera.transform.position;
            Vector3 targetPoint = target.bounds.ClosestPoint(origin);
            Vector3 delta = targetPoint - origin;
            if (delta.sqrMagnitude <= 0.000001f)
            {
                targetPoint = target.bounds.center;
                delta = targetPoint - origin;
            }

            float distance = delta.magnitude;
            if (!float.IsFinite(distance) || distance > interactionRange)
            {
                return OperationResult.Fail(
                    WorldInteractionFocusFailures.OutOfRange);
            }

            Vector3 direction = distance > 0.0001f
                ? delta / distance
                : camera.transform.forward;
            if (Vector3.Angle(camera.transform.forward, direction) > focusDegrees)
            {
                return OperationResult.Fail(
                    WorldInteractionFocusFailures.FocusMissing);
            }

            int ignoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
            int mask = ignoreRaycast >= 0
                ? ~(1 << ignoreRaycast)
                : Physics.DefaultRaycastLayers;
            int hitCount = Physics.RaycastNonAlloc(
                origin,
                direction,
                Hits,
                distance + 0.05f,
                mask,
                QueryTriggerInteraction.Collide);
            float nearestDistance = float.PositiveInfinity;
            Collider nearest = null;
            for (int index = 0; index < hitCount; index++)
            {
                RaycastHit hit = Hits[index];
                if (hit.collider == null ||
                    IsInHierarchy(hit.collider.transform, ignoredPlayerRoot))
                {
                    continue;
                }

                if (hit.distance < nearestDistance)
                {
                    nearestDistance = hit.distance;
                    nearest = hit.collider;
                }
            }

            return nearest == target ||
                   (nearest != null && nearest.transform.IsChildOf(target.transform)) ||
                   (nearest != null && target.transform.IsChildOf(nearest.transform))
                ? OperationResult.Success()
                : OperationResult.Fail(
                    WorldInteractionFocusFailures.LineOfSightBlocked);
        }

        private static bool IsInHierarchy(Transform candidate, Transform root)
        {
            return candidate != null && root != null &&
                   (candidate == root || candidate.IsChildOf(root));
        }
    }
}
