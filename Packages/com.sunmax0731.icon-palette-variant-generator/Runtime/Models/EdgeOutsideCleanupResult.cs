namespace Sunmax0731.IconPaletteVariantGenerator.Models
{
    /// <summary>
    /// Result summary for edge outside cleanup preprocessing.
    /// </summary>
    public readonly struct EdgeOutsideCleanupResult
    {
        public EdgeOutsideCleanupResult(int clearedRegionCount, int clearedPixelCount)
        {
            ClearedRegionCount = clearedRegionCount;
            ClearedPixelCount = clearedPixelCount;
        }

        public int ClearedRegionCount { get; }
        public int ClearedPixelCount { get; }
    }
}
