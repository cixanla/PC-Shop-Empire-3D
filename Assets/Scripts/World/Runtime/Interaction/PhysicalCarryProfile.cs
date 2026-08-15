using System;

namespace PCShopEmpire3D.World.Interaction
{
    public enum PhysicalCarryProfile
    {
        SmallBox = 0,
        LargeBox = 1,
        PcComponent = 2
    }

    public readonly struct PhysicalCarryProfileDefinition
    {
        internal PhysicalCarryProfileDefinition(
            float movementSpeedMultiplier,
            float fieldOfViewPenalty,
            bool allowsSprint,
            bool supportsPlacement)
        {
            MovementSpeedMultiplier = movementSpeedMultiplier;
            FieldOfViewPenalty = fieldOfViewPenalty;
            AllowsSprint = allowsSprint;
            SupportsPlacement = supportsPlacement;
        }

        public float MovementSpeedMultiplier { get; }

        public float FieldOfViewPenalty { get; }

        public bool AllowsSprint { get; }

        public bool SupportsPlacement { get; }
    }

    public static class PhysicalCarryProfileRules
    {
        public const float MinimumMovementSpeedMultiplier = 0.5f;
        public const float MaximumMovementSpeedMultiplier = 1f;
        public const float MaximumFieldOfViewPenalty = 8f;
        public const float LargeBoxMovementSpeedMultiplier = 0.65f;
        public const float LargeBoxFieldOfViewPenalty = 6f;
        public const float PcComponentMovementSpeedMultiplier = 0.9f;
        public const float PcComponentFieldOfViewPenalty = 1.5f;

        private static readonly PhysicalCarryProfileDefinition SmallBox = new(
            MaximumMovementSpeedMultiplier,
            0f,
            allowsSprint: true,
            supportsPlacement: true);

        private static readonly PhysicalCarryProfileDefinition LargeBox = new(
            LargeBoxMovementSpeedMultiplier,
            LargeBoxFieldOfViewPenalty,
            allowsSprint: false,
            supportsPlacement: false);

        private static readonly PhysicalCarryProfileDefinition PcComponent = new(
            PcComponentMovementSpeedMultiplier,
            PcComponentFieldOfViewPenalty,
            allowsSprint: false,
            supportsPlacement: false);

        public static PhysicalCarryProfileDefinition Resolve(PhysicalCarryProfile profile)
        {
            return profile switch
            {
                PhysicalCarryProfile.SmallBox => SmallBox,
                PhysicalCarryProfile.LargeBox => LargeBox,
                PhysicalCarryProfile.PcComponent => PcComponent,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(profile),
                    profile,
                    "The physical carry profile is unsupported.")
            };
        }
    }
}
