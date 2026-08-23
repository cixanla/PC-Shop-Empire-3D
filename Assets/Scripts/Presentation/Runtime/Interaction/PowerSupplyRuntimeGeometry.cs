using System;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    /// <summary>
    /// Canonical r29 semi-realistic ATX PSU and chassis-owned mount geometry.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PowerSupplyRuntimeGeometry : MonoBehaviour
    {
        public const string RuntimeMarker = "r29.power-supply.geometry";

        [SerializeField] private Transform housing;
        [SerializeField] private Transform fanAndGrille;
        [SerializeField] private Transform filteredFloorIntake;
        [SerializeField] private Transform acInlet;
        [SerializeField] private Transform rockerSwitch;
        [SerializeField] private Transform modularSocketPanel;
        [SerializeField] private Transform rearMountPlate;
        [SerializeField] private Transform[] fastenerPivots = new Transform[4];

        public Transform Housing => housing;

        public Transform FanAndGrille => fanAndGrille;

        public Transform FilteredFloorIntake => filteredFloorIntake;

        public Transform AcInlet => acInlet;

        public Transform RockerSwitch => rockerSwitch;

        public Transform ModularSocketPanel => modularSocketPanel;

        public Transform RearMountPlate => rearMountPlate;

        public Transform[] FastenerPivots => fastenerPivots;

        public bool IsCanonical =>
            IsOwnedChild(housing) &&
            IsOwnedChild(fanAndGrille) &&
            IsOwnedChild(acInlet) &&
            IsOwnedChild(rockerSwitch) &&
            IsOwnedChild(modularSocketPanel) &&
            filteredFloorIntake != null &&
            rearMountPlate != null &&
            !filteredFloorIntake.IsChildOf(transform) &&
            !rearMountPlate.IsChildOf(transform) &&
            fastenerPivots != null &&
            fastenerPivots.Length == 4 &&
            Array.TrueForAll(
                fastenerPivots,
                pivot => pivot != null && pivot.IsChildOf(rearMountPlate)) &&
            AreDistinct(
                housing,
                fanAndGrille,
                filteredFloorIntake,
                acInlet,
                rockerSwitch,
                modularSocketPanel,
                rearMountPlate) &&
            AreDistinct(fastenerPivots);

        public void Configure(
            Transform steelHousing,
            Transform intakeFanAndGrille,
            Transform chassisFilteredFloorIntake,
            Transform rearAcInlet,
            Transform rearRockerSwitch,
            Transform disconnectedModularSocketPanel,
            Transform chassisRearMountPlate,
            Transform[] fourFastenerPivots)
        {
            if (fourFastenerPivots == null || fourFastenerPivots.Length != 4)
            {
                throw new ArgumentException(
                    "Exactly four PSU fastener pivots are required.",
                    nameof(fourFastenerPivots));
            }

            housing = steelHousing ??
                throw new ArgumentNullException(nameof(steelHousing));
            fanAndGrille = intakeFanAndGrille ??
                throw new ArgumentNullException(nameof(intakeFanAndGrille));
            filteredFloorIntake = chassisFilteredFloorIntake ??
                throw new ArgumentNullException(nameof(chassisFilteredFloorIntake));
            acInlet = rearAcInlet ??
                throw new ArgumentNullException(nameof(rearAcInlet));
            rockerSwitch = rearRockerSwitch ??
                throw new ArgumentNullException(nameof(rearRockerSwitch));
            modularSocketPanel = disconnectedModularSocketPanel ??
                throw new ArgumentNullException(nameof(disconnectedModularSocketPanel));
            rearMountPlate = chassisRearMountPlate ??
                throw new ArgumentNullException(nameof(chassisRearMountPlate));
            fastenerPivots = (Transform[])fourFastenerPivots.Clone();

            if (!IsCanonical)
            {
                throw new ArgumentException(
                    "PSU geometry must preserve one item root, one chassis mount and four distinct screws.");
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
