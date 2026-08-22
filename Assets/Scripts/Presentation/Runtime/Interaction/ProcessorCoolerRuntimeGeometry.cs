using System;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    /// <summary>
    /// Canonical r27 world geometry: cold plate, pre-applied TIM, fin stack,
    /// fan, bracket and four distinct retention points.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ProcessorCoolerRuntimeGeometry : MonoBehaviour
    {
        public const string RuntimeMarker = "r27.processor-cooler.geometry";

        [SerializeField] private Transform coldPlate;
        [SerializeField] private Transform preAppliedTim;
        [SerializeField] private Transform finStack;
        [SerializeField] private Transform fan;
        [SerializeField] private Transform bracket;
        [SerializeField] private Transform[] retentionPoints = new Transform[4];

        public Transform ColdPlate => coldPlate;

        public Transform PreAppliedTim => preAppliedTim;

        public Transform FinStack => finStack;

        public Transform Fan => fan;

        public Transform Bracket => bracket;

        public Transform[] RetentionPoints => retentionPoints;

        public bool IsCanonical =>
            IsOwnedChild(coldPlate) &&
            IsOwnedChild(preAppliedTim) &&
            IsOwnedChild(finStack) &&
            IsOwnedChild(fan) &&
            IsOwnedChild(bracket) &&
            AreDistinct(coldPlate, preAppliedTim, finStack, fan, bracket) &&
            retentionPoints != null &&
            retentionPoints.Length == 4 &&
            Array.TrueForAll(retentionPoints, IsOwnedChild) &&
            AreDistinct(retentionPoints) &&
            Array.TrueForAll(
                retentionPoints,
                point => point != coldPlate &&
                         point != preAppliedTim &&
                         point != finStack &&
                         point != fan &&
                         point != bracket);

        public void Configure(
            Transform plate,
            Transform tim,
            Transform fins,
            Transform coolingFan,
            Transform mountingBracket,
            Transform[] points)
        {
            if (points == null || points.Length != 4)
            {
                throw new ArgumentException(
                    "Exactly four retention points are required.",
                    nameof(points));
            }

            coldPlate = plate != null
                ? plate
                : throw new ArgumentNullException(nameof(plate));
            preAppliedTim = tim != null
                ? tim
                : throw new ArgumentNullException(nameof(tim));
            finStack = fins != null
                ? fins
                : throw new ArgumentNullException(nameof(fins));
            fan = coolingFan != null
                ? coolingFan
                : throw new ArgumentNullException(nameof(coolingFan));
            bracket = mountingBracket != null
                ? mountingBracket
                : throw new ArgumentNullException(nameof(mountingBracket));
            retentionPoints = new Transform[4];
            Array.Copy(points, retentionPoints, points.Length);

            if (!IsCanonical)
            {
                throw new ArgumentException(
                    "Cooler geometry must contain distinct children owned by its canonical root.",
                    nameof(points));
            }
        }

        private bool IsOwnedChild(Transform candidate)
        {
            return candidate != null &&
                   candidate != transform &&
                   candidate.IsChildOf(transform);
        }

        private static bool AreDistinct(params Transform[] transforms)
        {
            if (transforms == null)
            {
                return false;
            }

            for (int left = 0; left < transforms.Length; left++)
            {
                if (transforms[left] == null)
                {
                    return false;
                }

                for (int right = left + 1; right < transforms.Length; right++)
                {
                    if (transforms[left] == transforms[right])
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
