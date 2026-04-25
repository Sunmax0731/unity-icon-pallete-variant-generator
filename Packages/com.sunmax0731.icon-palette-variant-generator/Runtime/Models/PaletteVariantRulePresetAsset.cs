using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Models
{
    /// <summary>
    /// Unity asset wrapper for source-control friendly shared replacement presets.
    /// </summary>
    [CreateAssetMenu(
        fileName = "PaletteVariantRulePreset",
        menuName = "Icon Palette Variant Generator/Rule Preset")]
    public sealed class PaletteVariantRulePresetAsset : ScriptableObject
    {
        public PaletteVariantRulePreset preset = new PaletteVariantRulePreset();
    }
}
