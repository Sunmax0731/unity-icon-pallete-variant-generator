using System.Collections.Generic;
using System.IO;
using Sunmax0731.IconPaletteVariantGenerator.Models;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Services
{
    /// <summary>
    /// Saves and loads reusable replacement rule presets as JSON.
    /// </summary>
    public sealed class RulePresetJsonService
    {
        public const string CurrentSchemaVersion = "1.0.0";

        public PaletteVariantRulePreset CreatePreset(PaletteVariantSession session, string displayName)
        {
            if (session == null)
            {
                throw new System.ArgumentNullException(nameof(session));
            }

            return new PaletteVariantRulePreset
            {
                schemaVersion = CurrentSchemaVersion,
                displayName = string.IsNullOrWhiteSpace(displayName) ? "Rule Preset" : displayName,
                colorGroups = CloneGroups(session.colorGroups),
                colorRules = CloneRules(session.colorRules)
            };
        }

        public void Save(string path, PaletteVariantRulePreset preset)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new System.ArgumentException("Preset path is required.", nameof(path));
            }

            if (preset == null)
            {
                throw new System.ArgumentNullException(nameof(preset));
            }

            preset.schemaVersion = string.IsNullOrWhiteSpace(preset.schemaVersion)
                ? CurrentSchemaVersion
                : preset.schemaVersion;

            string folder = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllText(path, JsonUtility.ToJson(preset, true));
        }

        public RulePresetLoadResult Load(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return new RulePresetLoadResult(null, new[] { "Preset JSON was not found." });
            }

            string json = File.ReadAllText(path);
            PaletteVariantRulePreset preset = JsonUtility.FromJson<PaletteVariantRulePreset>(json);
            List<string> warnings = Validate(preset);
            return new RulePresetLoadResult(preset, warnings);
        }

        public IReadOnlyList<string> ApplyToSession(PaletteVariantRulePreset preset, PaletteVariantSession session)
        {
            if (preset == null)
            {
                throw new System.ArgumentNullException(nameof(preset));
            }

            if (session == null)
            {
                throw new System.ArgumentNullException(nameof(session));
            }

            session.colorGroups ??= new List<ColorGroup>();
            session.colorRules ??= new List<ColorReplacementRule>();

            HashSet<string> existingGroupIds = new HashSet<string>();
            foreach (ColorGroup group in session.colorGroups)
            {
                if (group != null && !string.IsNullOrWhiteSpace(group.id))
                {
                    existingGroupIds.Add(group.id);
                }
            }

            List<string> warnings = new List<string>();
            foreach (ColorGroup presetGroup in preset.colorGroups ?? new List<ColorGroup>())
            {
                if (presetGroup == null || string.IsNullOrWhiteSpace(presetGroup.id))
                {
                    continue;
                }

                ColorGroup target = session.colorGroups.Find(group => group != null && group.id == presetGroup.id);
                if (target == null)
                {
                    warnings.Add($"Preset group was skipped because current session has no group: {presetGroup.id}");
                    continue;
                }

                target.replacementMode = presetGroup.replacementMode;
                target.targetColor = presetGroup.targetColor;
                target.blendRatio = presetGroup.blendRatio;
            }

            List<ColorReplacementRule> importedRules = new List<ColorReplacementRule>();
            foreach (ColorReplacementRule rule in preset.colorRules ?? new List<ColorReplacementRule>())
            {
                if (rule == null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(rule.groupId) && !existingGroupIds.Contains(rule.groupId))
                {
                    warnings.Add($"Preset rule was skipped because current session has no group: {rule.groupId}");
                    continue;
                }

                importedRules.Add(CloneRule(rule));
            }

            session.colorRules = importedRules;
            return warnings;
        }

        private static List<string> Validate(PaletteVariantRulePreset preset)
        {
            List<string> warnings = new List<string>();
            if (preset == null)
            {
                warnings.Add("Preset JSON could not be parsed.");
                return warnings;
            }

            if (preset.schemaVersion != CurrentSchemaVersion)
            {
                warnings.Add($"Preset schemaVersion is {preset.schemaVersion}; expected {CurrentSchemaVersion}.");
            }

            preset.colorGroups ??= new List<ColorGroup>();
            preset.colorRules ??= new List<ColorReplacementRule>();
            return warnings;
        }

        private static List<ColorGroup> CloneGroups(IEnumerable<ColorGroup> groups)
        {
            List<ColorGroup> clones = new List<ColorGroup>();
            if (groups == null)
            {
                return clones;
            }

            foreach (ColorGroup group in groups)
            {
                if (group == null)
                {
                    continue;
                }

                clones.Add(new ColorGroup
                {
                    id = group.id,
                    displayName = group.displayName,
                    representativeColor = group.representativeColor,
                    colorEntryIds = group.colorEntryIds == null ? new List<string>() : new List<string>(group.colorEntryIds),
                    replacementMode = group.replacementMode,
                    targetColor = group.targetColor,
                    blendRatio = group.blendRatio,
                    lockedGroup = group.lockedGroup,
                    pixelCount = group.pixelCount,
                    pixelRatio = group.pixelRatio
                });
            }

            return clones;
        }

        private static List<ColorReplacementRule> CloneRules(IEnumerable<ColorReplacementRule> rules)
        {
            List<ColorReplacementRule> clones = new List<ColorReplacementRule>();
            if (rules == null)
            {
                return clones;
            }

            foreach (ColorReplacementRule rule in rules)
            {
                if (rule != null)
                {
                    clones.Add(CloneRule(rule));
                }
            }

            return clones;
        }

        private static ColorReplacementRule CloneRule(ColorReplacementRule rule)
        {
            return new ColorReplacementRule
            {
                id = rule.id,
                groupId = rule.groupId,
                colorEntryId = rule.colorEntryId,
                scope = rule.scope,
                targetColor = rule.targetColor,
                blendRatio = rule.blendRatio,
                enabled = rule.enabled
            };
        }
    }
}
