using System.Collections.Generic;
using Sunmax0731.IconPaletteVariantGenerator.Models;
using Sunmax0731.IconPaletteVariantGenerator.Services;
using UnityEditor;

namespace Sunmax0731.IconPaletteVariantGenerator.Editor.Services
{
    /// <summary>
    /// Creates and updates Unity ScriptableObject assets for reusable rule presets.
    /// </summary>
    public sealed class RulePresetAssetService
    {
        private readonly RulePresetJsonService rulePresetJsonService;

        public RulePresetAssetService()
            : this(new RulePresetJsonService())
        {
        }

        public RulePresetAssetService(RulePresetJsonService rulePresetJsonService)
        {
            this.rulePresetJsonService = rulePresetJsonService;
        }

        public PaletteVariantRulePresetAsset CreateAsset(string assetPath, PaletteVariantSession session, string displayName)
        {
            PaletteVariantRulePresetAsset asset = UnityEngine.ScriptableObject.CreateInstance<PaletteVariantRulePresetAsset>();
            asset.preset = rulePresetJsonService.CreatePreset(session, displayName);
            AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath(assetPath));
            AssetDatabase.SaveAssets();
            return asset;
        }

        public void UpdateAsset(PaletteVariantRulePresetAsset asset, PaletteVariantSession session, string displayName)
        {
            if (asset == null)
            {
                throw new System.ArgumentNullException(nameof(asset));
            }

            asset.preset = rulePresetJsonService.CreatePreset(session, displayName);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
        }

        public IReadOnlyList<string> ApplyToSession(PaletteVariantRulePresetAsset asset, PaletteVariantSession session)
        {
            if (asset == null)
            {
                return new[] { "Preset asset is not selected." };
            }

            return rulePresetJsonService.ApplyToSession(asset.preset, session);
        }
    }
}
