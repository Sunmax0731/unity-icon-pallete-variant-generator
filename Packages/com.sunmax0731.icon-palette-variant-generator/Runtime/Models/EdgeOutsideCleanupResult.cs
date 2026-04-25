namespace Sunmax0731.IconPaletteVariantGenerator.Models
{
    /// <summary>
    /// Result summary for edge outside cleanup preprocessing.
    /// </summary>
    public readonly struct EdgeOutsideCleanupResult
    {
        public EdgeOutsideCleanupResult(int clearedRegionCount, int clearedPixelCount)
            : this(clearedRegionCount, clearedPixelCount, System.Array.Empty<int>())
        {
        }

        public EdgeOutsideCleanupResult(int clearedRegionCount, int clearedPixelCount, System.Collections.Generic.IReadOnlyList<int> clearedPixelIndices)
        {
            ClearedRegionCount = clearedRegionCount;
            ClearedPixelCount = clearedPixelCount;
            ClearedPixelIndices = clearedPixelIndices ?? System.Array.Empty<int>();
        }

        public int ClearedRegionCount { get; }
        public int ClearedPixelCount { get; }
        public System.Collections.Generic.IReadOnlyList<int> ClearedPixelIndices { get; }
    }
}
