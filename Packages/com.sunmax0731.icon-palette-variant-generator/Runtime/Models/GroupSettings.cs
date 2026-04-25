using System;

namespace Sunmax0731.IconPaletteVariantGenerator.Models
{
    /// <summary>
    /// Settings used when grouping extracted palette colors.
    /// </summary>
    [Serializable]
    public sealed class GroupSettings
    {
        public int targetGroupCount = 6;
        public ColorDistanceMode distanceMode = ColorDistanceMode.Rgb;
        public float maxColorDistance = 441f;
        public bool preserveDarkOutline = true;
        public bool preserveAlpha = true;
    }
}
