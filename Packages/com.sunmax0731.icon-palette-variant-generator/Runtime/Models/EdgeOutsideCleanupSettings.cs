using System;

namespace Sunmax0731.IconPaletteVariantGenerator.Models
{
    /// <summary>
    /// Settings for clearing small foreground islands just outside the main image edge.
    /// </summary>
    [Serializable]
    public sealed class EdgeOutsideCleanupSettings
    {
        public bool enabled;
        public EdgeOutsideCleanupMode mode = EdgeOutsideCleanupMode.DetachedRegions;
        public int maxDistancePixels = 2;
        public int maxRegionPixels = 12;
        public int trimDistancePixels = 1;
    }

    public enum EdgeOutsideCleanupMode
    {
        DetachedRegions,
        BoundaryTrim
    }
}
