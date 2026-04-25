using System;
using System.Collections.Generic;

namespace Sunmax0731.IconPaletteVariantGenerator.Models
{
    /// <summary>
    /// Named export variation with independent replacement rules.
    /// </summary>
    [Serializable]
    public sealed class IconVariation
    {
        public string id = string.Empty;
        public string displayName = "Variation";
        public string fileSuffix = "variant";
        public bool exportEnabled = true;
        public List<ColorGroup> colorGroups = new List<ColorGroup>();
        public List<ColorReplacementRule> colorRules = new List<ColorReplacementRule>();
    }
}
