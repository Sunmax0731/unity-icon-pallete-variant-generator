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

        public ColorReplacementService()
            : this(new ColorQuantizationService())
        {
        }

        public ColorReplacementService(ColorQuantizationService quantizationService)
        {
            this.quantizationService = quantizationService;
        }

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

            Dictionary<uint, ColorGroup> groupsByColorKey = BuildGroupLookup(session);
            Texture2D output = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false)
            {
                name = $"{source.name}_Preview",
                filterMode = source.filterMode,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            Color32[] sourcePixels = source.GetPixels32();
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
                if (!groupsByColorKey.TryGetValue(colorKey, out ColorGroup group))
                {
                    outputPixels[index] = pixel;
                    continue;
                }

                float ratio = Mathf.Clamp01(group.blendRatio);
                Color32 replacement = Lerp(pixel, group.targetColor, ratio);
                replacement.a = session.groupSettings.preserveAlpha ? pixel.a : group.targetColor.a;
                outputPixels[index] = replacement;
            }

            output.SetPixels32(outputPixels);
            output.Apply(false, false);
            return output;
        }

        private static Dictionary<uint, ColorGroup> BuildGroupLookup(PaletteVariantSession session)
        {
            Dictionary<string, ColorGroup> groupsById = session.colorGroups
                .Where(group => group != null && !string.IsNullOrEmpty(group.id))
                .ToDictionary(group => group.id, group => group);
            Dictionary<uint, ColorGroup> groupsByColorKey = new Dictionary<uint, ColorGroup>();

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

                groupsByColorKey[ColorCodeUtility.ToRgbKey(entry.color)] = group;
            }

            return groupsByColorKey;
        }

        private static Color32 Lerp(Color32 from, Color32 to, float ratio)
        {
            return new Color32(
                (byte)Mathf.Clamp(Mathf.RoundToInt(from.r + ((to.r - from.r) * ratio)), 0, 255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(from.g + ((to.g - from.g) * ratio)), 0, 255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(from.b + ((to.b - from.b) * ratio)), 0, 255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(from.a + ((to.a - from.a) * ratio)), 0, 255));
        }
    }
}
