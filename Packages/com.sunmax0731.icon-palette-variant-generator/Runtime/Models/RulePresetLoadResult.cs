using System.Collections.Generic;
using System.Linq;

namespace Sunmax0731.IconPaletteVariantGenerator.Models
{
    /// <summary>
    /// Result of loading a replacement rule preset JSON file.
    /// </summary>
    public sealed class RulePresetLoadResult
    {
        public RulePresetLoadResult(PaletteVariantRulePreset preset, IReadOnlyList<string> warnings)
        {
            Preset = preset;
            Warnings = warnings ?? new List<string>();
        }

        public PaletteVariantRulePreset Preset { get; }

        public IReadOnlyList<string> Warnings { get; }

        public bool Success => Preset != null && !Warnings.Any();
    }
}
