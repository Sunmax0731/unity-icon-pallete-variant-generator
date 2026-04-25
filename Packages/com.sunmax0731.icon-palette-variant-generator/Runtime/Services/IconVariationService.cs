using System.Collections.Generic;
using System.Linq;
using Sunmax0731.IconPaletteVariantGenerator.Models;

namespace Sunmax0731.IconPaletteVariantGenerator.Services
{
    /// <summary>
    /// Maintains variation snapshots for palette replacement sessions.
    /// </summary>
    public sealed class IconVariationService
    {
        public IconVariation EnsureActiveVariation(PaletteVariantSession session)
        {
            if (session == null)
            {
                throw new System.ArgumentNullException(nameof(session));
            }

            session.variations ??= new List<IconVariation>();
            session.colorGroups ??= new List<ColorGroup>();
            session.colorRules ??= new List<ColorReplacementRule>();

            IconVariation active = GetActiveVariation(session);
            if (active != null)
            {
                return active;
            }

            IconVariation variation = CreateVariation(
                session,
                $"variation_{session.variations.Count + 1:00}",
                session.variations.Count == 0 ? "Variation 1" : $"Variation {session.variations.Count + 1}",
                session.exportSettings?.fileSuffix ?? "variant");
            session.variations.Add(variation);
            session.activeVariationId = variation.id;
            return variation;
        }

        public IconVariation GetActiveVariation(PaletteVariantSession session)
        {
            if (session == null || session.variations == null || string.IsNullOrEmpty(session.activeVariationId))
            {
                return null;
            }

            return session.variations.FirstOrDefault(variation => variation != null && variation.id == session.activeVariationId);
        }

        public IconVariation AddVariation(PaletteVariantSession session)
        {
            if (session == null)
            {
                throw new System.ArgumentNullException(nameof(session));
            }

            SyncActiveVariation(session);
            session.variations ??= new List<IconVariation>();
            int number = session.variations.Count + 1;
            IconVariation variation = CreateVariation(session, $"variation_{number:00}", $"Variation {number}", $"variant_{number:00}");
            session.variations.Add(variation);
            ApplyVariation(session, variation.id);
            return variation;
        }

        public IconVariation DuplicateActiveVariation(PaletteVariantSession session)
        {
            if (session == null)
            {
                throw new System.ArgumentNullException(nameof(session));
            }

            SyncActiveVariation(session);
            IconVariation source = EnsureActiveVariation(session);
            int number = session.variations.Count + 1;
            IconVariation duplicate = new IconVariation
            {
                id = $"variation_{number:00}",
                displayName = $"{source.displayName} Copy",
                fileSuffix = $"{source.fileSuffix}_copy",
                exportEnabled = source.exportEnabled,
                colorGroups = CloneGroups(source.colorGroups),
                colorRules = CloneRules(source.colorRules)
            };

            session.variations.Add(duplicate);
            ApplyVariation(session, duplicate.id);
            return duplicate;
        }

        public bool RemoveActiveVariation(PaletteVariantSession session)
        {
            if (session == null || session.variations == null || session.variations.Count <= 1)
            {
                return false;
            }

            IconVariation active = GetActiveVariation(session);
            if (active == null)
            {
                return false;
            }

            int index = session.variations.IndexOf(active);
            session.variations.RemoveAt(index);
            int nextIndex = System.Math.Min(index, session.variations.Count - 1);
            ApplyVariation(session, session.variations[nextIndex].id);
            return true;
        }

        public void ApplyVariation(PaletteVariantSession session, string variationId)
        {
            if (session == null)
            {
                throw new System.ArgumentNullException(nameof(session));
            }

            session.variations ??= new List<IconVariation>();
            IconVariation variation = session.variations.FirstOrDefault(candidate => candidate != null && candidate.id == variationId);
            if (variation == null)
            {
                return;
            }

            session.activeVariationId = variation.id;
            session.colorGroups = CloneGroups(variation.colorGroups);
            session.colorRules = CloneRules(variation.colorRules);
            session.exportSettings ??= new ExportSettings();
            session.exportSettings.fileSuffix = variation.fileSuffix;
        }

        public void SyncActiveVariation(PaletteVariantSession session)
        {
            if (session == null)
            {
                throw new System.ArgumentNullException(nameof(session));
            }

            IconVariation variation = EnsureActiveVariation(session);
            variation.colorGroups = CloneGroups(session.colorGroups);
            variation.colorRules = CloneRules(session.colorRules);
            variation.fileSuffix = string.IsNullOrWhiteSpace(variation.fileSuffix)
                ? session.exportSettings.fileSuffix
                : variation.fileSuffix;
        }

        private static IconVariation CreateVariation(PaletteVariantSession session, string id, string displayName, string fileSuffix)
        {
            return new IconVariation
            {
                id = id,
                displayName = displayName,
                fileSuffix = string.IsNullOrWhiteSpace(fileSuffix) ? id : fileSuffix,
                exportEnabled = true,
                colorGroups = CloneGroups(session.colorGroups),
                colorRules = CloneRules(session.colorRules)
            };
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
                if (rule == null)
                {
                    continue;
                }

                clones.Add(new ColorReplacementRule
                {
                    id = rule.id,
                    groupId = rule.groupId,
                    colorEntryId = rule.colorEntryId,
                    scope = rule.scope,
                    targetColor = rule.targetColor,
                    blendRatio = rule.blendRatio,
                    enabled = rule.enabled
                });
            }

            return clones;
        }
    }
}
