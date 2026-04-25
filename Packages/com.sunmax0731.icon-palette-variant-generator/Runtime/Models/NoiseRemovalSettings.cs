using System;

namespace Sunmax0731.IconPaletteVariantGenerator.Models
{
    /// <summary>
    /// Settings for filling small isolated regions with nearby surrounding colors.
    /// </summary>
    [Serializable]
    public sealed class NoiseRemovalSettings
    {
        public bool enabled;
        public int maxRegionPixels = 4;
        public float neighborDistanceThreshold = 32f;
        public bool sameGroupOnly = true;
    }
}
