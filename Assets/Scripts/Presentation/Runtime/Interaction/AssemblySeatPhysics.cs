using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    internal enum AssemblySeatPhysicsStatus
    {
        Valid = 0,
        ContextMissing = 1,
        Paused = 2,
        AuthorityBlocked = 3,
        OutOfRange = 4,
        NotFocused = 5,
        LineOfSightBlocked = 6,
        Obstructed = 7
    }

    /// <summary>
    /// Shared allocation-free focus, line-of-sight and insertion-volume gate for keyed
    /// motherboard components. Component-specific orientation and authority states remain
    /// in their own solver.
    /// </summary>
    internal static class AssemblySeatPhysics
    {
        private const int HitCapacity = 32;
        private const float DistanceTieEpsilon = 0.0001f;
        private static readonly RaycastHit[] LineHits = new RaycastHit[HitCapacity];
        private static readonly RaycastHit[] InsertionHits = new RaycastHit[HitCapacity];
        private static readonly Collider[] Overlaps = new Collider[HitCapacity];

        internal static AssemblySeatPhysicsStatus EvaluateFocus(
            Transform interactionOrigin,
            Transform playerRoot,
            Transform heldItemRoot,
            Collider focusCollider,
            Transform assemblyRoot,
            LayerMask obstructionMask,
            float maximumRange,
            float minimumFocusDot,
            bool paused,
            bool authorityAvailable)
        {
            if (interactionOrigin == null ||
                focusCollider == null ||
                assemblyRoot == null ||
                !focusCollider.enabled ||
                !focusCollider.gameObject.activeInHierarchy)
            {
                return AssemblySeatPhysicsStatus.ContextMissing;
            }

            if (paused)
            {
                return AssemblySeatPhysicsStatus.Paused;
            }

            if (!authorityAvailable)
            {
                return AssemblySeatPhysicsStatus.AuthorityBlocked;
            }

            Vector3 toFocus = focusCollider.bounds.center - interactionOrigin.position;
            float distance = toFocus.magnitude;
            if (distance <= Mathf.Epsilon || distance > Mathf.Max(0.1f, maximumRange))
            {
                return AssemblySeatPhysicsStatus.OutOfRange;
            }

            Vector3 direction = toFocus / distance;
            if (Vector3.Dot(interactionOrigin.forward, direction) <
                Mathf.Clamp01(minimumFocusDot))
            {
                return AssemblySeatPhysicsStatus.NotFocused;
            }

            int targetMask = 1 << focusCollider.gameObject.layer;
            int hitCount = Physics.RaycastNonAlloc(
                interactionOrigin.position,
                direction,
                LineHits,
                distance + 0.03f,
                obstructionMask | targetMask,
                QueryTriggerInteraction.Ignore);
            if (hitCount <= 0)
            {
                return AssemblySeatPhysicsStatus.LineOfSightBlocked;
            }

            if (hitCount >= HitCapacity)
            {
                return AssemblySeatPhysicsStatus.Obstructed;
            }

            float targetDistance = float.PositiveInfinity;
            float obstructionDistance = float.PositiveInfinity;
            for (int index = 0; index < hitCount; index++)
            {
                RaycastHit hit = LineHits[index];
                if (hit.collider == null ||
                    IsChildOf(hit.collider.transform, playerRoot) ||
                    IsChildOf(hit.collider.transform, heldItemRoot) ||
                    (hit.collider != focusCollider &&
                     IsChildOf(hit.collider.transform, assemblyRoot)))
                {
                    continue;
                }

                if (hit.collider == focusCollider)
                {
                    targetDistance = Mathf.Min(targetDistance, hit.distance);
                }
                else
                {
                    obstructionDistance = Mathf.Min(obstructionDistance, hit.distance);
                }
            }

            if (float.IsPositiveInfinity(targetDistance))
            {
                return AssemblySeatPhysicsStatus.LineOfSightBlocked;
            }

            return obstructionDistance <= targetDistance + DistanceTieEpsilon
                ? AssemblySeatPhysicsStatus.Obstructed
                : AssemblySeatPhysicsStatus.Valid;
        }

        internal static bool IsPoseObstructed(
            PhysicalItemProjection item,
            Pose candidatePose,
            Vector3 insertionNormal,
            float insertionDistance,
            Collider focusCollider,
            Transform playerRoot,
            Transform assemblyRoot,
            LayerMask obstructionMask)
        {
            if (item == null || focusCollider == null || assemblyRoot == null)
            {
                return true;
            }

            int overlapCount = Physics.OverlapBoxNonAlloc(
                candidatePose.position,
                item.DropHalfExtents,
                Overlaps,
                candidatePose.rotation,
                obstructionMask,
                QueryTriggerInteraction.Ignore);
            if (overlapCount >= HitCapacity)
            {
                return true;
            }

            for (int index = 0; index < overlapCount; index++)
            {
                Collider overlap = Overlaps[index];
                if (!ShouldIgnore(
                    overlap,
                    focusCollider,
                    playerRoot,
                    item.transform,
                    assemblyRoot))
                {
                    return true;
                }
            }

            Vector3 normalizedInsertion = insertionNormal.sqrMagnitude > Mathf.Epsilon
                ? insertionNormal.normalized
                : Vector3.forward;
            float distance = Mathf.Max(0.001f, insertionDistance);
            int insertionCount = Physics.BoxCastNonAlloc(
                candidatePose.position + (normalizedInsertion * distance),
                item.DropHalfExtents,
                -normalizedInsertion,
                InsertionHits,
                candidatePose.rotation,
                distance,
                obstructionMask,
                QueryTriggerInteraction.Ignore);
            if (insertionCount >= HitCapacity)
            {
                return true;
            }

            for (int index = 0; index < insertionCount; index++)
            {
                if (!ShouldIgnore(
                    InsertionHits[index].collider,
                    focusCollider,
                    playerRoot,
                    item.transform,
                    assemblyRoot))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ShouldIgnore(
            Collider collider,
            Collider focusCollider,
            Transform playerRoot,
            Transform itemRoot,
            Transform assemblyRoot)
        {
            return collider == null ||
                   collider == focusCollider ||
                   IsChildOf(collider.transform, playerRoot) ||
                   IsChildOf(collider.transform, itemRoot) ||
                   IsChildOf(collider.transform, assemblyRoot);
        }

        private static bool IsChildOf(Transform candidate, Transform root)
        {
            return candidate != null && root != null && candidate.IsChildOf(root);
        }
    }
}
