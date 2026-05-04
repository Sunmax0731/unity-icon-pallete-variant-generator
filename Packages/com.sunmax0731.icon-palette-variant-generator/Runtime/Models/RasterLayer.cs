using System;

namespace Sunmax0731.IconPaletteVariantGenerator.Models
{
    /// <summary>
    /// Serializable raster layer used for paint and imported image overlays.
    /// </summary>
    [Serializable]
    public sealed class RasterLayer
    {
        public string id = string.Empty;
        public string displayName = "Layer";
        public LayerKind kind = LayerKind.Paint;
        public bool visible = true;
        public bool locked;
        public float opacity = 1f;
        public LayerBlendMode blendMode = LayerBlendMode.Normal;
        public int offsetX;
        public int offsetY;
        public string sourceAssetPath = string.Empty;
        public LayerPixelData pixelData = new LayerPixelData();
    }
}
