using System.Collections.Generic;
using NUnit.Framework;
using Sunmax0731.IconPaletteVariantGenerator.Models;
using Sunmax0731.IconPaletteVariantGenerator.Services;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Editor.Tests
{
    public sealed class NoiseRemovalServiceTests
    {
        [Test]
        public void ApplyFillsSmallRegionWithNearbyGroupColor()
        {
            Color32 fill = new Color32(240, 0, 0, 255);
            Color32 noise = new Color32(220, 10, 10, 255);
            Color32[] pixels =
            {
                fill, fill, fill,
                fill, noise, fill,
                fill, fill, fill
            };
            PaletteVariantSession session = CreateNoiseSession(fill, noise);
            session.noiseRemovalSettings = new NoiseRemovalSettings
            {
                enabled = true,
                maxRegionPixels = 1,
                neighborDistanceThreshold = 64f,
                sameGroupOnly = true
            };

            NoiseRemovalResult result = new NoiseRemovalService().Apply(pixels, 3, 3, session);

            Assert.That(result.FilledRegionCount, Is.EqualTo(1));
            Assert.That(result.FilledPixelCount, Is.EqualTo(1));
            Assert.That(result.FilledPixelIndices, Is.EquivalentTo(new[] { 4 }));
            Assert.That(pixels[4].r, Is.EqualTo(fill.r));
            Assert.That(pixels[4].g, Is.EqualTo(fill.g));
            Assert.That(pixels[4].b, Is.EqualTo(fill.b));
        }

        [Test]
        public void ApplyDoesNotFillRegionLargerThanThreshold()
        {
            Color32 fill = new Color32(240, 0, 0, 255);
            Color32 noise = new Color32(220, 10, 10, 255);
            Color32[] pixels =
            {
                fill, fill, fill,
                noise, noise, fill,
                fill, fill, fill
            };
            PaletteVariantSession session = CreateNoiseSession(fill, noise);
            session.noiseRemovalSettings = new NoiseRemovalSettings
            {
                enabled = true,
                maxRegionPixels = 1,
                neighborDistanceThreshold = 64f,
                sameGroupOnly = true
            };

            NoiseRemovalResult result = new NoiseRemovalService().Apply(pixels, 3, 3, session);

            Assert.That(result.FilledRegionCount, Is.EqualTo(0));
            Assert.That(pixels[3].r, Is.EqualTo(noise.r));
            Assert.That(pixels[4].r, Is.EqualTo(noise.r));
        }

        [Test]
        public void ColorReplacementAppliesNoiseRemovalBeforeReplacement()
        {
            Color32 fill = new Color32(240, 0, 0, 255);
            Color32 noise = new Color32(220, 10, 10, 255);
            Texture2D source = new Texture2D(3, 3, TextureFormat.RGBA32, false);
            source.SetPixels32(new[]
            {
                fill, fill, fill,
                fill, noise, fill,
                fill, fill, fill
            });
            source.Apply();

            PaletteVariantSession session = CreateNoiseSession(fill, noise);
            session.noiseRemovalSettings = new NoiseRemovalSettings
            {
                enabled = true,
                maxRegionPixels = 1,
                neighborDistanceThreshold = 64f,
                sameGroupOnly = true
            };
            session.colorGroups[0].targetColor = new Color32(0, 0, 255, 255);
            ColorReplacementService service = new ColorReplacementService();

            Texture2D output = service.Apply(source, session);
            Color32 center = output.GetPixels32()[4];

            Assert.That(service.LastNoiseRemovalResult.FilledPixelCount, Is.EqualTo(1));
            Assert.That(center.r, Is.EqualTo(0));
            Assert.That(center.g, Is.EqualTo(0));
            Assert.That(center.b, Is.EqualTo(255));

            Object.DestroyImmediate(source);
            Object.DestroyImmediate(output);
        }

        private static PaletteVariantSession CreateNoiseSession(Color32 fill, Color32 noise)
        {
            return new PaletteVariantSession
            {
                analyzeSettings = new AnalyzeSettings { alphaThreshold = 0, quantizeStep = 1 },
                groupSettings = new GroupSettings
                {
                    distanceMode = ColorDistanceMode.Rgb,
                    preserveAlpha = true
                },
                paletteColors = new List<PaletteColorEntry>
                {
                    new PaletteColorEntry
                    {
                        id = "#F00000",
                        hex = "#F00000",
                        color = fill,
                        pixelCount = 8,
                        groupId = "group_01"
                    },
                    new PaletteColorEntry
                    {
                        id = "#DC0A0A",
                        hex = "#DC0A0A",
                        color = noise,
                        pixelCount = 1,
                        groupId = "group_01"
                    }
                },
                colorGroups = new List<ColorGroup>
                {
                    new ColorGroup
                    {
                        id = "group_01",
                        displayName = "Group 1",
                        targetColor = fill,
                        blendRatio = 1f,
                        colorEntryIds = new List<string> { "#F00000", "#DC0A0A" }
                    }
                }
            };
        }
    }
}
