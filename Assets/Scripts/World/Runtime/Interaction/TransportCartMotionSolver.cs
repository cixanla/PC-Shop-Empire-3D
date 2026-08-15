using UnityEngine;

namespace PCShopEmpire3D.World.Interaction
{
    public enum TransportCartMotionStatus
    {
        Valid = 0,
        ContextMissing = 1,
        DriverInactive = 2,
        InvalidPose = 3,
        NoSupport = 4,
        Blocked = 5,
        QueryCapacity = 6
    }

    public readonly struct TransportCartMotionEvaluation
    {
        internal TransportCartMotionEvaluation(
            TransportCartMotionStatus status,
            Pose pose,
            string failureCode)
        {
            Status = status;
            Pose = pose;
            FailureCode = failureCode ?? string.Empty;
        }

        public TransportCartMotionStatus Status { get; }

        public Pose Pose { get; }

        public string FailureCode { get; }

        public bool IsValid => Status == TransportCartMotionStatus.Valid;
    }

    public static class TransportCartMotionSolver
    {
        private const int HitCapacity = 32;
        private const float BoundsInset = 0.96f;
        private const float MinimumUpDot = 0.75f;
        private const float ProbeStartHeight = 0.45f;
        private const float ProbeDistance = 0.75f;
        private static readonly Collider[] Overlaps = new Collider[HitCapacity];
        private static readonly RaycastHit[] CastHits = new RaycastHit[HitCapacity];

        public static TransportCartMotionEvaluation Evaluate(
            TransportCartProjection cart,
            Pose desiredPose,
            LayerMask supportMask,
            LayerMask obstructionMask,
            Transform ignoredRoot)
        {
            if (cart == null)
            {
                return Invalid(
                    TransportCartMotionStatus.ContextMissing,
                    desiredPose,
                    "cart.context-missing");
            }

            if (!cart.IsDriven)
            {
                return Invalid(
                    TransportCartMotionStatus.DriverInactive,
                    desiredPose,
                    "cart.driver-inactive");
            }

            if (!IsFinite(desiredPose.position) || !IsFinite(desiredPose.rotation))
            {
                return Invalid(
                    TransportCartMotionStatus.InvalidPose,
                    desiredPose,
                    "cart.pose-invalid");
            }

            if (!HasFullWheelSupport(cart, desiredPose, supportMask))
            {
                return Invalid(
                    TransportCartMotionStatus.NoSupport,
                    desiredPose,
                    "cart.no-support");
            }

            Vector3 halfExtents = cart.MotionHalfExtents * BoundsInset;
            Vector3 desiredCenter = cart.GetMotionCenter(desiredPose);
            int overlapCount = Physics.OverlapBoxNonAlloc(
                desiredCenter,
                halfExtents,
                Overlaps,
                desiredPose.rotation,
                obstructionMask,
                QueryTriggerInteraction.Ignore);
            if (overlapCount >= HitCapacity)
            {
                return Invalid(
                    TransportCartMotionStatus.QueryCapacity,
                    desiredPose,
                    "cart.query-capacity");
            }

            for (int index = 0; index < overlapCount; index++)
            {
                if (!IsIgnored(Overlaps[index], cart, ignoredRoot))
                {
                    return Invalid(
                        TransportCartMotionStatus.Blocked,
                        desiredPose,
                        "cart.drive-blocked");
                }
            }

            Vector3 currentCenter = cart.GetMotionCenter(
                new Pose(cart.transform.position, cart.transform.rotation));
            Vector3 displacement = desiredCenter - currentCenter;
            float distance = displacement.magnitude;
            if (distance > 0.0001f)
            {
                int castCount = Physics.BoxCastNonAlloc(
                    currentCenter,
                    halfExtents,
                    displacement / distance,
                    CastHits,
                    desiredPose.rotation,
                    distance,
                    obstructionMask,
                    QueryTriggerInteraction.Ignore);
                if (castCount >= HitCapacity)
                {
                    return Invalid(
                        TransportCartMotionStatus.QueryCapacity,
                        desiredPose,
                        "cart.query-capacity");
                }

                for (int index = 0; index < castCount; index++)
                {
                    if (!IsIgnored(CastHits[index].collider, cart, ignoredRoot))
                    {
                        return Invalid(
                            TransportCartMotionStatus.Blocked,
                            desiredPose,
                            "cart.drive-blocked");
                    }
                }
            }

            return new TransportCartMotionEvaluation(
                TransportCartMotionStatus.Valid,
                desiredPose,
                string.Empty);
        }

        private static bool HasFullWheelSupport(
            TransportCartProjection cart,
            Pose pose,
            LayerMask supportMask)
        {
            Vector3 extents = cart.MotionHalfExtents;
            float x = extents.x * 0.78f;
            float z = extents.z * 0.78f;
            Vector3[] localProbes =
            {
                new Vector3(-x, ProbeStartHeight, -z),
                new Vector3(x, ProbeStartHeight, -z),
                new Vector3(-x, ProbeStartHeight, z),
                new Vector3(x, ProbeStartHeight, z)
            };

            foreach (Vector3 localProbe in localProbes)
            {
                Vector3 origin = pose.position + (pose.rotation * localProbe);
                if (!Physics.Raycast(
                        origin,
                        Vector3.down,
                        out RaycastHit support,
                        ProbeDistance,
                        supportMask,
                        QueryTriggerInteraction.Ignore) ||
                    Vector3.Dot(support.normal, Vector3.up) < MinimumUpDot)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsIgnored(
            Collider collider,
            TransportCartProjection cart,
            Transform ignoredRoot)
        {
            if (collider == null)
            {
                return true;
            }

            Transform candidate = collider.transform;
            if (candidate.IsChildOf(cart.transform))
            {
                return true;
            }

            if (ignoredRoot != null && candidate.IsChildOf(ignoredRoot))
            {
                return true;
            }

            PhysicalItemProjection cargo = cart.Cargo;
            return cargo != null && candidate.IsChildOf(cargo.transform);
        }

        private static TransportCartMotionEvaluation Invalid(
            TransportCartMotionStatus status,
            Pose pose,
            string failureCode)
        {
            return new TransportCartMotionEvaluation(status, pose, failureCode);
        }

        private static bool IsFinite(Vector3 value)
        {
            return float.IsFinite(value.x) && float.IsFinite(value.y) && float.IsFinite(value.z);
        }

        private static bool IsFinite(Quaternion value)
        {
            return float.IsFinite(value.x) &&
                   float.IsFinite(value.y) &&
                   float.IsFinite(value.z) &&
                   float.IsFinite(value.w);
        }
    }
}
