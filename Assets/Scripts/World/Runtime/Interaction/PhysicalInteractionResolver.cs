using System;
using PCShopEmpire3D.Core.Primitives;
using UnityEngine;

namespace PCShopEmpire3D.World.Interaction
{
    [DisallowMultipleComponent]
    public sealed class PhysicalInteractionResolver : MonoBehaviour
    {
        private const int HitCapacity = 24;

        [SerializeField] private Transform origin;
        [SerializeField] private Transform ignoredRoot;
        [SerializeField, Min(0.1f)] private float maximumRange = 2f;
        [SerializeField, Range(0f, 0.25f)] private float assistRadius = 0.08f;
        [SerializeField] private LayerMask queryMask = ~0;

        private readonly RaycastHit[] _hits = new RaycastHit[HitCapacity];

        public float MaximumRange => maximumRange;

        public void Configure(
            Transform interactionOrigin,
            Transform rootToIgnore,
            float range,
            float radius,
            LayerMask layers)
        {
            origin = interactionOrigin != null
                ? interactionOrigin
                : throw new ArgumentNullException(nameof(interactionOrigin));
            ignoredRoot = rootToIgnore;
            maximumRange = Mathf.Max(0.1f, range);
            assistRadius = Mathf.Clamp(radius, 0f, 0.25f);
            queryMask = layers;
        }

        public OperationResult<PhysicalItemProjection> Resolve()
        {
            if (origin == null)
            {
                return OperationResult<PhysicalItemProjection>.Fail(
                    Failure.FromCode("interaction.origin-missing"));
            }

            int count = assistRadius > 0f
                ? Physics.SphereCastNonAlloc(
                    origin.position,
                    assistRadius,
                    origin.forward,
                    _hits,
                    maximumRange,
                    queryMask,
                    QueryTriggerInteraction.Ignore)
                : Physics.RaycastNonAlloc(
                    origin.position,
                    origin.forward,
                    _hits,
                    maximumRange,
                    queryMask,
                    QueryTriggerInteraction.Ignore);

            if (count >= HitCapacity)
            {
                return OperationResult<PhysicalItemProjection>.Fail(
                    Failure.FromCode("interaction.query-capacity"));
            }

            PhysicalItemProjection best = null;
            float bestDistance = float.PositiveInfinity;
            string bestId = null;
            for (int index = 0; index < count; index++)
            {
                RaycastHit hit = _hits[index];
                if (hit.collider == null || IsIgnored(hit.collider.transform))
                {
                    continue;
                }

                PhysicalItemProjection candidate = hit.collider.GetComponentInParent<PhysicalItemProjection>();
                if (candidate == null || !candidate.isActiveAndEnabled || candidate.IsCarried)
                {
                    continue;
                }

                if (!HasLineOfSight(candidate, hit.point))
                {
                    continue;
                }

                string candidateId = candidate.ItemIdValue;
                if (hit.distance < bestDistance - 0.0001f ||
                    (Mathf.Abs(hit.distance - bestDistance) <= 0.0001f &&
                     string.CompareOrdinal(candidateId, bestId) < 0))
                {
                    best = candidate;
                    bestDistance = hit.distance;
                    bestId = candidateId;
                }
            }

            return best != null
                ? OperationResult<PhysicalItemProjection>.Success(best)
                : OperationResult<PhysicalItemProjection>.Fail(Failure.FromCode("interaction.no-target"));
        }

        private bool HasLineOfSight(PhysicalItemProjection candidate, Vector3 assistHitPoint)
        {
            Vector3 target = candidate.Body != null
                ? candidate.Body.worldCenterOfMass
                : assistHitPoint;
            Vector3 direction = target - origin.position;
            float distance = direction.magnitude;
            if (distance <= Mathf.Epsilon || distance > maximumRange + assistRadius)
            {
                return false;
            }

            if (!Physics.Raycast(
                    origin.position,
                    direction / distance,
                    out RaycastHit lineHit,
                    distance + 0.02f,
                    queryMask,
                    QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            return lineHit.collider.GetComponentInParent<PhysicalItemProjection>() == candidate;
        }

        private bool IsIgnored(Transform candidate)
        {
            return ignoredRoot != null && candidate.IsChildOf(ignoredRoot);
        }

        private void OnValidate()
        {
            maximumRange = Mathf.Max(0.1f, maximumRange);
            assistRadius = Mathf.Clamp(assistRadius, 0f, 0.25f);
        }
    }
}
