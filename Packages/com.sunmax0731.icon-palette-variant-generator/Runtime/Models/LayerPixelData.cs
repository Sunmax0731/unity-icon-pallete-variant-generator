using System;

namespace Sunmax0731.IconPaletteVariantGenerator.Models
{
    /// <summary>
    /// Serializable pixel payload for a raster layer.
    /// </summary>
    [Serializable]
    public sealed class LayerPixelData
    {
        public int width;
        public int height;
        public string rgbaBytesBase64 = string.Empty;
    }
}
