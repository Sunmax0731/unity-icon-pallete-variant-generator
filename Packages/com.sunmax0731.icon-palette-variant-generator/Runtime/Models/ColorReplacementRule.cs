using System;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Models
{
    /// <summary>
    /// Replacement rule for a color group or individual palette color.
    /// </summary>
    [Serializable]
    public sealed class ColorReplacementRule
    {
        public string id = string.Empty;
        public string groupId = string.Empty;
        public string colorEntryId = string.Empty;
        public ColorReplacementScope scope = ColorReplacementScope.Group;
        public Color32 targetColor;
        public float blendRatio = 1f;
        public bool enabled = true;
    }
}
