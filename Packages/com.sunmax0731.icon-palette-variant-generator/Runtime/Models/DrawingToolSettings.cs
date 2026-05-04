using System;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Models
{
    /// <summary>
    /// Editable drawing tool settings stored with the current session.
    /// </summary>
    [Serializable]
    public sealed class DrawingToolSettings
    {
        public DrawToolKind activeTool = DrawToolKind.Brush;
        public int brushSize = 5;
        public float strength = 1f;
        public Color32 paintColor = new Color32(255, 64, 64, 255);
        public float paintOpacity = 1f;
        public int noiseRegionPixels = 4;
        public float noiseThreshold = 48f;
        public int smoothIterations = 1;
        public int blurRadius = 1;
    }
}
