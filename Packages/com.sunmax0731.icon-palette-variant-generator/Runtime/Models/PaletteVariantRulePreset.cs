using System;
using System.Collections.Generic;

namespace Sunmax0731.IconPaletteVariantGenerator.Models
{
    /// <summary>
    /// Reusable replacement rule preset for applying palette edits across sessions.
    /// </summary>
    [Serializable]
    public sealed class PaletteVariantRulePreset
    {
        public string schemaVersion = "1.0.0";
        public string displayName = "Rule Preset";
        public List<ColorGroup> colorGroups = new List<ColorGroup>();
        public List<ColorReplacementRule> colorRules = new List<ColorReplacementRule>();
    }
}
