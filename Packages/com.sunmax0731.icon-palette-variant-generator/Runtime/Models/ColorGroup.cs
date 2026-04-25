using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Models
{
    /// <summary>
    /// Group of nearby palette colors used as a replacement target.
    /// </summary>
    [Serializable]
    public sealed class ColorGroup
    {
        public string id = string.Empty;
        public string displayName = string.Empty;
        public Color32 representativeColor;
        public List<string> colorEntryIds = new List<string>();
        public ColorReplacementMode replacementMode = ColorReplacementMode.GroupUniform;
        public Color32 targetColor;
        public float blendRatio = 1f;
        public bool lockedGroup;
        public int pixelCount;
        public float pixelRatio;
    }
}
