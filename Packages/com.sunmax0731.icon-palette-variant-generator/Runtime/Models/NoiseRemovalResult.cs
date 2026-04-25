namespace Sunmax0731.IconPaletteVariantGenerator.Models
{
    /// <summary>
    /// Result summary for noise removal preprocessing.
    /// </summary>
    public readonly struct NoiseRemovalResult
    {
        public NoiseRemovalResult(int filledRegionCount, int filledPixelCount)
            : this(filledRegionCount, filledPixelCount, System.Array.Empty<int>())
        {
        }

        public NoiseRemovalResult(int filledRegionCount, int filledPixelCount, System.Collections.Generic.IReadOnlyList<int> filledPixelIndices)
        {
            FilledRegionCount = filledRegionCount;
            FilledPixelCount = filledPixelCount;
            FilledPixelIndices = filledPixelIndices ?? System.Array.Empty<int>();
        }

        public int FilledRegionCount { get; }
        public int FilledPixelCount { get; }
        public System.Collections.Generic.IReadOnlyList<int> FilledPixelIndices { get; }
    }
}
