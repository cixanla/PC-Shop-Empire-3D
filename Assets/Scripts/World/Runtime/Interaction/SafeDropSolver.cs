using PCShopEmpire3D.Core.Primitives;
using UnityEngine;

namespace PCShopEmpire3D.World.Interaction
{
    public static class SafeDropSolver
    {
        private const float SurfaceClearance = 0.025f;
        private const float MinimumUpDot = 0.75f;
        private static readonly float[] CandidateDistances = { 1.15f, 0.9f, 0.7f };

        public static OperationResult<Pose> FindPose(
            Transform origin,
            PhysicalItemProjection item,
            LayerMask supportMask,
            LayerMask obstructionMask)
        {
            if (origin == null || item == null)
            {
                return OperationResult<Pose>.Fail(Failure.FromCode("drop.context-missing"));
            }

            bool foundSupport = false;
            foreach (float distance in CandidateDistances)
            {
                Vector3 rayStart = origin.position + (origin.forward * distance) + (Vector3.up * 0.15f);
                if (!Physics.Raycast(
                        rayStart,
                        Vector3.down,
                        out RaycastHit support,
                        2.5f,
                        supportMask,
                        QueryTriggerInteraction.Ignore))
                {
                    continue;
                }

                foundSupport = true;
                if (Vector3.Dot(support.normal, Vector3.up) < MinimumUpDot)
                {
                    continue;
                }

                Quaternion rotation = Quaternion.Euler(0f, origin.eulerAngles.y, 0f);
                Vector3 halfExtents = item.DropHalfExtents;
                Vector3 position = support.point + (Vector3.up * (halfExtents.y + SurfaceClearance));
                Vector3 overlapExtents = halfExtents * 0.94f;
                if (Physics.CheckBox(
                        position,
                        overlapExtents,
                        rotation,
                        obstructionMask,
                        QueryTriggerInteraction.Ignore))
                {
                    continue;
                }

                return OperationResult<Pose>.Success(new Pose(position, rotation));
            }

            return OperationResult<Pose>.Fail(Failure.FromCode(
                foundSupport ? "drop.blocked" : "drop.no-support"));
        }
    }
}
