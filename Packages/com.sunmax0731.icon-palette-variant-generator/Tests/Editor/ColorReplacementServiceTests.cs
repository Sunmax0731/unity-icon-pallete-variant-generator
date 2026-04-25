using System.Collections.Generic;
using NUnit.Framework;
using Sunmax0731.IconPaletteVariantGenerator.Models;
using Sunmax0731.IconPaletteVariantGenerator.Services;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Editor.Tests
{
    public sealed class ColorReplacementServiceTests
    {
        [Test]
        public void ApplyUsesGroupTargetColorAndPreservesAlpha()
        {
            Texture2D source = new Texture2D(2, 1, TextureFormat.RGBA32, false);
            source.SetPixels32(new[]
            {
                new Color32(255, 0, 0, 128),
                new Color32(0, 0, 0, 0)
            });
            source.Apply();

            PaletteVariantSession session = new PaletteVariantSession
            {
                analyzeSettings = new AnalyzeSettings { alphaThreshold = 8, quantizeStep = 1 },
                groupSettings = new GroupSettings { preserveAlpha = true },
                paletteColors = new List<PaletteColorEntry>
                {
                    new PaletteColorEntry
                    {
                        id = "#FF0000",
                        hex = "#FF0000",
                        color = new Color32(255, 0, 0, 255),
                        pixelCount = 1,
                        groupId = "group_01"
                    }
                },
                colorGroups = new List<ColorGroup>
                {
                    new ColorGroup
                    {
                        id = "group_01",
                        targetColor = new Color32(0, 0, 255, 255),
                        blendRatio = 1f
                    }
                }
            };

            Texture2D output = new ColorReplacementService().Apply(source, session);
            Color32[] pixels = output.GetPixels32();

            Assert.That(pixels[0].r, Is.EqualTo(0));
            Assert.That(pixels[0].g, Is.EqualTo(0));
            Assert.That(pixels[0].b, Is.EqualTo(255));
            Assert.That(pixels[0].a, Is.EqualTo(128));
            Assert.That(pixels[1].a, Is.EqualTo(0));

            Object.DestroyImmediate(source);
            Object.DestroyImmediate(output);
        }

        [Test]
        public void ApplyHonorsBlendRatio()
        {
            Texture2D source = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            source.SetPixels32(new[] { new Color32(100, 0, 0, 255) });
            source.Apply();

            PaletteVariantSession session = new PaletteVariantSession
            {
                analyzeSettings = new AnalyzeSettings { alphaThreshold = 0, quantizeStep = 1 },
                groupSettings = new GroupSettings { preserveAlpha = true },
                paletteColors = new List<PaletteColorEntry>
                {
                    new PaletteColorEntry
                    {
                        id = "#640000",
                        hex = "#640000",
                        color = new Color32(100, 0, 0, 255),
                        pixelCount = 1,
                        groupId = "group_01"
                    }
                },
                colorGroups = new List<ColorGroup>
                {
                    new ColorGroup
                    {
                        id = "group_01",
                        targetColor = new Color32(200, 0, 0, 255),
                        blendRatio = 0.5f
                    }
                }
            };

            Texture2D output = new ColorReplacementService().Apply(source, session);

            Assert.That(output.GetPixel(0, 0).r * 255f, Is.EqualTo(150f).Within(1f));

            Object.DestroyImmediate(source);
            Object.DestroyImmediate(output);
        }

        [Test]
        public void ApplyUsesTransparentTargetAlphaEvenWhenPreserveAlphaIsEnabled()
        {
            Texture2D source = CreateSinglePixelTexture(new Color32(255, 0, 0, 255));
            PaletteVariantSession session = CreateSingleColorSession(ColorReplacementMode.GroupUniform);
            session.groupSettings.preserveAlpha = true;
            session.colorGroups[0].targetColor = new Color32(0, 0, 0, 0);
            session.colorGroups[0].blendRatio = 1f;

            Texture2D output = new ColorReplacementService().Apply(source, session);
            Color32 pixel = output.GetPixels32()[0];

            Assert.That(pixel.a, Is.EqualTo(0));

            Object.DestroyImmediate(source);
            Object.DestroyImmediate(output);
        }

        [Test]
        public void ApplyLetsHybridColorRuleOverrideGroupFallback()
        {
            Texture2D source = CreateSinglePixelTexture(new Color32(255, 0, 0, 255));
            PaletteVariantSession session = CreateSingleColorSession(ColorReplacementMode.Hybrid);
            session.colorGroups[0].targetColor = new Color32(0, 0, 255, 255);
            session.colorRules = new List<ColorReplacementRule>
            {
                new ColorReplacementRule
                {
                    id = "rule_01",
                    groupId = "group_01",
                    colorEntryId = "#FF0000",
                    scope = ColorReplacementScope.ColorEntry,
                    targetColor = new Color32(0, 255, 0, 255),
                    blendRatio = 1f,
                    enabled = true
                }
            };

            Texture2D output = new ColorReplacementService().Apply(source, session);
            Color32 pixel = output.GetPixels32()[0];

            Assert.That(pixel.r, Is.EqualTo(0));
            Assert.That(pixel.g, Is.EqualTo(255));
            Assert.That(pixel.b, Is.EqualTo(0));

            Object.DestroyImmediate(source);
            Object.DestroyImmediate(output);
        }

        [Test]
        public void ApplyDoesNotUseGroupFallbackInPerColorMode()
        {
            Texture2D source = CreateSinglePixelTexture(new Color32(255, 0, 0, 255));
            PaletteVariantSession session = CreateSingleColorSession(ColorReplacementMode.PerColor);
            session.colorGroups[0].targetColor = new Color32(0, 0, 255, 255);

            Texture2D output = new ColorReplacementService().Apply(source, session);
            Color32 pixel = output.GetPixels32()[0];

            Assert.That(pixel.r, Is.EqualTo(255));
            Assert.That(pixel.g, Is.EqualTo(0));
            Assert.That(pixel.b, Is.EqualTo(0));

            Object.DestroyImmediate(source);
            Object.DestroyImmediate(output);
        }

        private static Texture2D CreateSinglePixelTexture(Color32 color)
        {
            Texture2D source = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            source.SetPixels32(new[] { color });
            source.Apply();
            return source;
        }

        private static PaletteVariantSession CreateSingleColorSession(ColorReplacementMode replacementMode)
        {
            return new PaletteVariantSession
            {
                analyzeSettings = new AnalyzeSettings { alphaThreshold = 0, quantizeStep = 1 },
                groupSettings = new GroupSettings { preserveAlpha = true },
                paletteColors = new List<PaletteColorEntry>
                {
                    new PaletteColorEntry
                    {
                        id = "#FF0000",
                        hex = "#FF0000",
                        color = new Color32(255, 0, 0, 255),
                        pixelCount = 1,
                        groupId = "group_01"
                    }
                },
                colorGroups = new List<ColorGroup>
                {
                    new ColorGroup
                    {
                        id = "group_01",
                        targetColor = new Color32(0, 0, 255, 255),
                        blendRatio = 1f,
                        replacementMode = replacementMode
                    }
                }
            };
        }
    }
}
