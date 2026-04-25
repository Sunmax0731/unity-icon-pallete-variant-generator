using System.Collections.Generic;
using System.Linq;
using Sunmax0731.IconPaletteVariantGenerator.Models;
using Sunmax0731.IconPaletteVariantGenerator.Utilities;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Services
{
    /// <summary>
    /// Extracts visible palette colors and pixel counts from a readable Texture2D.
    /// </summary>
    public sealed class ColorExtractionService
    {
        private readonly ColorQuantizationService quantizationService;

        public ColorExtractionService()
            : this(new ColorQuantizationService())
        {
        }

        public ColorExtractionService(ColorQuantizationService quantizationService)
        {
            this.quantizationService = quantizationService;
        }

        public IReadOnlyList<PaletteColorEntry> Extract(Texture2D texture, AnalyzeSettings settings)
        {
            if (texture == null)
            {
                throw new System.ArgumentNullException(nameof(texture));
            }

            if (settings == null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            Color32[] pixels = texture.GetPixels32();
            Dictionary<uint, PaletteColorEntry> entriesByColor = new Dictionary<uint, PaletteColorEntry>();
            int analyzedPixels = 0;
            int alphaThreshold = Mathf.Clamp(settings.alphaThreshold, 0, 255);
            int quantizeStep = Mathf.Clamp(settings.quantizeStep, 1, 64);
            int minimumPixelCount = Mathf.Max(1, settings.minimumPixelCount);
            int maxPaletteColors = Mathf.Max(1, settings.maxPaletteColors);

            foreach (Color32 pixel in pixels)
            {
                if (pixel.a <= alphaThreshold)
                {
                    continue;
                }

                analyzedPixels++;
                Color32 quantized = quantizationService.Quantize(pixel, quantizeStep);
                uint key = ColorCodeUtility.ToRgbKey(quantized);

                if (!entriesByColor.TryGetValue(key, out PaletteColorEntry entry))
                {
                    entry = new PaletteColorEntry
                    {
                        id = ColorCodeUtility.ToHex(quantized),
                        color = quantized,
                        hex = ColorCodeUtility.ToHex(quantized)
                    };
                    entriesByColor.Add(key, entry);
                }

                entry.pixelCount++;
            }

            if (analyzedPixels == 0)
            {
                return new List<PaletteColorEntry>();
            }

            return entriesByColor.Values
                .Where(entry => entry.pixelCount >= minimumPixelCount)
                .OrderByDescending(entry => entry.pixelCount)
                .ThenBy(entry => entry.hex)
                .Take(maxPaletteColors)
                .Select(entry =>
                {
                    entry.pixelRatio = (float)entry.pixelCount / analyzedPixels;
                    return entry;
                })
                .ToList();
        }
    }
}
