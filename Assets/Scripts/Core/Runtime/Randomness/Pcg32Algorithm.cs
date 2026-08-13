namespace PCShopEmpire3D.Core.Randomness
{
    /// <summary>
    /// Versioned constants for the PCG XSH-RR 64/32 set-sequence generator.
    /// </summary>
    public static class Pcg32Algorithm
    {
        public const string Id = "pcg32-xsh-rr-64-32-v1";

        public const ulong Multiplier = 6364136223846793005UL;

        /// <summary>
        /// PCG set-sequence uses 63 selector bits to address 2^63 distinct streams.
        /// Rejecting the high bit prevents two public selector values from silently aliasing.
        /// </summary>
        public const ulong MaximumStreamSelector = 0x7FFF_FFFF_FFFF_FFFFUL;
    }
}
