namespace Sunmax0731.IconPaletteVariantGenerator.Models
{
    /// <summary>
    /// Result summary for noise removal preprocessing.
    /// </summary>
    public readonly struct NoiseRemovalResult
    {
        public NoiseRemovalResult(int filledRegionCount, int filledPixelCount)
        {
            FilledRegionCount = filledRegionCount;
            FilledPixelCount = filledPixelCount;
        }

        public int FilledRegionCount { get; }
        public int FilledPixelCount { get; }
    }
}
