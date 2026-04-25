using System.Linq;
using NUnit.Framework;
using Sunmax0731.IconPaletteVariantGenerator.Models;
using Sunmax0731.IconPaletteVariantGenerator.Services;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Editor.Tests
{
    public sealed class ColorExtractionServiceTests
    {
        [Test]
        public void ExtractIgnoresTransparentPixelsAndCountsVisibleColors()
        {
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.SetPixels32(new[]
            {
                new Color32(255, 0, 0, 255),
                new Color32(255, 0, 0, 255),
                new Color32(0, 0, 255, 255),
                new Color32(0, 0, 0, 0)
            });
            texture.Apply();

            AnalyzeSettings settings = new AnalyzeSettings
            {
                alphaThreshold = 8,
                minimumPixelCount = 1,
                quantizeStep = 1,
                maxPaletteColors = 16
            };

            var entries = new ColorExtractionService().Extract(texture, settings).ToList();

            Assert.That(entries, Has.Count.EqualTo(2));
            Assert.That(entries[0].hex, Is.EqualTo("#FF0000"));
            Assert.That(entries[0].pixelCount, Is.EqualTo(2));
            Assert.That(entries[0].pixelRatio, Is.EqualTo(2f / 3f).Within(0.001f));
            Assert.That(entries[1].hex, Is.EqualTo("#0000FF"));

            Object.DestroyImmediate(texture);
        }

        [Test]
        public void ExtractAppliesQuantizeStep()
        {
            Texture2D texture = new Texture2D(2, 1, TextureFormat.RGBA32, false);
            texture.SetPixels32(new[]
            {
                new Color32(253, 1, 1, 255),
                new Color32(255, 0, 0, 255)
            });
            texture.Apply();

            AnalyzeSettings settings = new AnalyzeSettings
            {
                alphaThreshold = 0,
                minimumPixelCount = 1,
                quantizeStep = 4,
                maxPaletteColors = 16
            };

            var entries = new ColorExtractionService().Extract(texture, settings).ToList();

            Assert.That(entries, Has.Count.EqualTo(1));
            Assert.That(entries[0].hex, Is.EqualTo("#FC0000"));
            Assert.That(entries[0].pixelCount, Is.EqualTo(2));

            Object.DestroyImmediate(texture);
        }
    }
}
