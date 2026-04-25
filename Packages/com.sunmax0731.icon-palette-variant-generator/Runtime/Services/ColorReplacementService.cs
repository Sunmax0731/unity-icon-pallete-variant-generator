using System.Collections.Generic;
using System.Linq;
using Sunmax0731.IconPaletteVariantGenerator.Models;
using Sunmax0731.IconPaletteVariantGenerator.Utilities;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Services
{
    /// <summary>
    /// Applies palette group replacement rules to a source texture.
    /// </summary>
    public sealed class ColorReplacementService
    {
        private readonly ColorQuantizationService quantizationService;
        private readonly EdgeOutsideCleanupService edgeOutsideCleanupService;
        private readonly NoiseRemovalService noiseRemovalService;

        public ColorReplacementService()
            : this(new ColorQuantizationService(), new EdgeOutsideCleanupService(), new NoiseRemovalService())
        {
        }

        public ColorReplacementService(ColorQuantizationService quantizationService)
            : this(quantizationService, new EdgeOutsideCleanupService(), new NoiseRemovalService())
        {
        }

        public ColorReplacementService(ColorQuantizationService quantizationService, EdgeOutsideCleanupService edgeOutsideCleanupService, NoiseRemovalService noiseRemovalService)
        {
            this.quantizationService = quantizationService;
            this.edgeOutsideCleanupService = edgeOutsideCleanupService;
            this.noiseRemovalService = noiseRemovalService;
        }

        public EdgeOutsideCleanupResult LastEdgeOutsideCleanupResult { get; private set; }
        public NoiseRemovalResult LastNoiseRemovalResult { get; private set; }

        public Texture2D Apply(Texture2D source, PaletteVariantSession session)
        {
            if (source == null)
            {
                throw new System.ArgumentNullException(nameof(source));
            }

            if (session == null)
            {
                throw new System.ArgumentNullException(nameof(session));
            }

            Dictionary<uint, ReplacementTarget> replacementsByColorKey = BuildReplacementLookup(session);
            Texture2D output = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false)
            {
                name = $"{source.name}_Preview",
                filterMode = source.filterMode,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            Color32[] sourcePixels = source.GetPixels32();
            LastEdgeOutsideCleanupResult = edgeOutsideCleanupService.Apply(sourcePixels, source.width, source.height, session);
            LastNoiseRemovalResult = noiseRemovalService.Apply(sourcePixels, source.width, source.height, session);
            Color32[] outputPixels = new Color32[sourcePixels.Length];
            int alphaThreshold = Mathf.Clamp(session.analyzeSettings.alphaThreshold, 0, 255);
            int quantizeStep = Mathf.Clamp(session.analyzeSettings.quantizeStep, 1, 64);

            for (int index = 0; index < sourcePixels.Length; index++)
            {
                Color32 pixel = sourcePixels[index];
                if (pixel.a <= alphaThreshold)
                {
                    outputPixels[index] = pixel;
                    continue;
                }

                Color32 quantized = quantizationService.Quantize(pixel, quantizeStep);
                uint colorKey = ColorCodeUtility.ToRgbKey(quantized);
                if (!replacementsByColorKey.TryGetValue(colorKey, out ReplacementTarget replacementTarget))
                {
                    outputPixels[index] = pixel;
                    continue;
                }

                float ratio = Mathf.Clamp01(replacementTarget.BlendRatio);
                Color32 replacement = Lerp(pixel, replacementTarget.TargetColor, ratio);
                if (session.groupSettings.preserveAlpha && replacementTarget.TargetColor.a == 255)
                {
                    replacement.a = pixel.a;
                }
                else if (!session.groupSettings.preserveAlpha)
                {
                    replacement.a = replacementTarget.TargetColor.a;
                }

                outputPixels[index] = replacement;
            }

            output.SetPixels32(outputPixels);
            output.Apply(false, false);
            return output;
        }

        private static Dictionary<uint, ReplacementTarget> BuildReplacementLookup(PaletteVariantSession session)
        {
            Dictionary<string, ColorGroup> groupsById = session.colorGroups
                .Where(group => group != null && !string.IsNullOrEmpty(group.id))
                .ToDictionary(group => group.id, group => group);
            Dictionary<string, ColorReplacementRule> colorRulesByEntryId = new Dictionary<string, ColorReplacementRule>();
            foreach (ColorReplacementRule rule in session.colorRules ?? new List<ColorReplacementRule>())
            {
                if (rule == null
                    || !rule.enabled
                    || rule.scope != ColorReplacementScope.ColorEntry
                    || string.IsNullOrEmpty(rule.colorEntryId))
                {
                    continue;
                }

                colorRulesByEntryId[rule.colorEntryId] = rule;
            }

            Dictionary<uint, ReplacementTarget> replacementsByColorKey = new Dictionary<uint, ReplacementTarget>();

            foreach (PaletteColorEntry entry in session.paletteColors)
            {
                if (entry == null || entry.ignored || string.IsNullOrEmpty(entry.groupId))
                {
                    continue;
                }

                if (!groupsById.TryGetValue(entry.groupId, out ColorGroup group))
                {
                    continue;
                }

                bool canUseColorRule = group.replacementMode == ColorReplacementMode.PerColor
                    || group.replacementMode == ColorReplacementMode.Hybrid;
                if (canUseColorRule && colorRulesByEntryId.TryGetValue(entry.id, out ColorReplacementRule colorRule))
                {
                    replacementsByColorKey[ColorCodeUtility.ToRgbKey(entry.color)] = new ReplacementTarget(colorRule.targetColor, colorRule.blendRatio);
                    continue;
                }

                if (group.replacementMode == ColorReplacementMode.PerColor)
                {
                    continue;
                }

                replacementsByColorKey[ColorCodeUtility.ToRgbKey(entry.color)] = new ReplacementTarget(group.targetColor, group.blendRatio);
            }

            return replacementsByColorKey;
        }

        private static Color32 Lerp(Color32 from, Color32 to, float ratio)
        {
            return new Color32(
                (byte)Mathf.Clamp(Mathf.RoundToInt(from.r + ((to.r - from.r) * ratio)), 0, 255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(from.g + ((to.g - from.g) * ratio)), 0, 255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(from.b + ((to.b - from.b) * ratio)), 0, 255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(from.a + ((to.a - from.a) * ratio)), 0, 255));
        }

        private readonly struct ReplacementTarget
        {
            public ReplacementTarget(Color32 targetColor, float blendRatio)
            {
                TargetColor = targetColor;
                BlendRatio = blendRatio;
            }

            public Color32 TargetColor { get; }
            public float BlendRatio { get; }
        }
    }
}
