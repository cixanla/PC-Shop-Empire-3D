using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public enum MotherboardFastenerStatus
    {
        Uninitialized = 0,
        ValidUnsecured = 1,
        ValidSecured = 2,
        ContextMissing = 3,
        Paused = 4,
        AuthorityBlocked = 5,
        OutOfRange = 6,
        NotFocused = 7,
        LineOfSightBlocked = 8,
        Obstructed = 9
    }

    public readonly struct MotherboardFastenerEvaluation
    {
        public MotherboardFastenerEvaluation(
            MotherboardFastenerStatus status,
            bool isSecured)
        {
            Status = status;
            IsSecured = isSecured;
        }

        public MotherboardFastenerStatus Status { get; }

        public bool IsSecured { get; }

        public bool CanOperate =>
            Status == MotherboardFastenerStatus.ValidUnsecured ||
            Status == MotherboardFastenerStatus.ValidSecured;

        public string FailureCode => Status switch
        {
            MotherboardFastenerStatus.ContextMissing =>
                "assembly-fastener.context-missing",
            MotherboardFastenerStatus.Paused => "assembly-fastener.paused",
            MotherboardFastenerStatus.AuthorityBlocked =>
                "assembly-fastener.authority-blocked",
            MotherboardFastenerStatus.OutOfRange =>
                "assembly-fastener.out-of-range",
            MotherboardFastenerStatus.NotFocused =>
                "assembly-fastener.focus-missing",
            MotherboardFastenerStatus.LineOfSightBlocked =>
                "assembly-fastener.line-of-sight-blocked",
            MotherboardFastenerStatus.Obstructed =>
                "assembly-fastener.obstructed",
            _ => string.Empty
        };
    }

    /// <summary>
    /// Allocation-free, fail-closed interaction gate for the single captive motherboard
    /// fastener. Domain state is supplied by the Assembly authority and is never inferred
    /// from the presentation transform.
    /// </summary>
    public static class MotherboardFastenerSolver
    {
        private const int HitCapacity = 32;
        private const float DistanceTieEpsilon = 0.0001f;
        private static readonly RaycastHit[] LineHits = new RaycastHit[HitCapacity];

        public static MotherboardFastenerEvaluation Evaluate(
            Transform interactionOrigin,
            Transform playerRoot,
            Collider focusCollider,
            LayerMask obstructionMask,
            float maximumRange,
            float minimumFocusDot,
            bool paused,
            bool isSeated,
            bool isSecured)
        {
            if (interactionOrigin == null ||
                focusCollider == null ||
                !focusCollider.gameObject.activeInHierarchy)
            {
                return new MotherboardFastenerEvaluation(
                    MotherboardFastenerStatus.ContextMissing,
                    isSecured);
            }

            if (paused)
            {
                return Invalid(MotherboardFastenerStatus.Paused, isSecured);
            }

            if (!isSeated)
            {
                return Invalid(
                    MotherboardFastenerStatus.AuthorityBlocked,
                    isSecured);
            }

            if (!focusCollider.enabled)
            {
                return Invalid(
                    MotherboardFastenerStatus.ContextMissing,
                    isSecured);
            }

            Vector3 focusPoint = focusCollider.bounds.center;
            Vector3 toFocus = focusPoint - interactionOrigin.position;
            float distance = toFocus.magnitude;
            if (distance <= Mathf.Epsilon || distance > Mathf.Max(0.1f, maximumRange))
            {
                return Invalid(MotherboardFastenerStatus.OutOfRange, isSecured);
            }

            Vector3 direction = toFocus / distance;
            if (Vector3.Dot(interactionOrigin.forward, direction) <
                Mathf.Clamp01(minimumFocusDot))
            {
                return Invalid(MotherboardFastenerStatus.NotFocused, isSecured);
            }

            int targetMask = 1 << focusCollider.gameObject.layer;
            int hitCount = Physics.RaycastNonAlloc(
                interactionOrigin.position,
                direction,
                LineHits,
                distance + 0.03f,
                obstructionMask | targetMask,
                QueryTriggerInteraction.Collide);
            if (hitCount <= 0)
            {
                return Invalid(
                    MotherboardFastenerStatus.LineOfSightBlocked,
                    isSecured);
            }

            if (hitCount >= HitCapacity)
            {
                return Invalid(MotherboardFastenerStatus.Obstructed, isSecured);
            }

            float targetDistance = float.PositiveInfinity;
            float obstructionDistance = float.PositiveInfinity;
            for (int index = 0; index < hitCount; index++)
            {
                RaycastHit hit = LineHits[index];
                if (hit.collider == null || IsChildOf(hit.collider.transform, playerRoot))
                {
                    continue;
                }

                if (hit.collider == focusCollider)
                {
                    targetDistance = Mathf.Min(targetDistance, hit.distance);
                }
                else if (hit.collider.isTrigger)
                {
                    continue;
                }
                else
                {
                    obstructionDistance = Mathf.Min(
                        obstructionDistance,
                        hit.distance);
                }
            }

            if (float.IsPositiveInfinity(targetDistance))
            {
                return Invalid(
                    MotherboardFastenerStatus.LineOfSightBlocked,
                    isSecured);
            }

            if (obstructionDistance <= targetDistance + DistanceTieEpsilon)
            {
                return Invalid(MotherboardFastenerStatus.Obstructed, isSecured);
            }

            return new MotherboardFastenerEvaluation(
                isSecured
                    ? MotherboardFastenerStatus.ValidSecured
                    : MotherboardFastenerStatus.ValidUnsecured,
                isSecured);
        }

        private static bool IsChildOf(Transform candidate, Transform root)
        {
            return candidate != null && root != null && candidate.IsChildOf(root);
        }

        private static MotherboardFastenerEvaluation Invalid(
            MotherboardFastenerStatus status,
            bool isSecured)
        {
            return new MotherboardFastenerEvaluation(status, isSecured);
        }
    }
}
