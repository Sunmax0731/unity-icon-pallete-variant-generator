using System;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Models
{
    /// <summary>
    /// Color entry extracted from an analyzed icon image.
    /// </summary>
    [Serializable]
    public sealed class PaletteColorEntry
    {
        public string id = string.Empty;
        public Color32 color;
        public string hex = "#000000";
        public int pixelCount;
        public float pixelRatio;
        public string groupId = string.Empty;
        public bool lockedGroup;
        public bool ignored;
    }
}
