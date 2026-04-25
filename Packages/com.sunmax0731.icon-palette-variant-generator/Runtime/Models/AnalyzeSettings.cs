using System;

namespace Sunmax0731.IconPaletteVariantGenerator.Models
{
    /// <summary>
    /// Settings used when extracting palette colors from a source image.
    /// </summary>
    [Serializable]
    public sealed class AnalyzeSettings
    {
        public int alphaThreshold = 8;
        public int minimumPixelCount = 1;
        public int quantizeStep = 4;
        public int maxPaletteColors = 256;
    }
}
