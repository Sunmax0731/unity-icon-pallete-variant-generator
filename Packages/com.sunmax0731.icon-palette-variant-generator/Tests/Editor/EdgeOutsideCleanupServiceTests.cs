using NUnit.Framework;
using Sunmax0731.IconPaletteVariantGenerator.Models;
using Sunmax0731.IconPaletteVariantGenerator.Services;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Editor.Tests
{
    public sealed class EdgeOutsideCleanupServiceTests
    {
        [Test]
        public void ApplyClearsSmallOutsideRegionNearMainEdge()
        {
            Color32 body = new Color32(255, 0, 0, 255);
            Color32 outside = new Color32(0, 255, 0, 255);
            Color32 clear = new Color32(0, 0, 0, 0);
            Color32[] pixels =
            {
                clear, clear, clear, clear, clear,
                clear, body, body, clear, outside,
                clear, body, body, clear, clear,
                clear, body, body, clear, clear,
                clear, clear, clear, clear, clear
            };

            PaletteVariantSession session = new PaletteVariantSession
            {
                analyzeSettings = new AnalyzeSettings { alphaThreshold = 0 },
                edgeOutsideCleanupSettings = new EdgeOutsideCleanupSettings
                {
                    enabled = true,
                    maxDistancePixels = 2,
                    maxRegionPixels = 4
                }
            };

            EdgeOutsideCleanupResult result = new EdgeOutsideCleanupService().Apply(pixels, 5, 5, session);

            Assert.That(result.ClearedRegionCount, Is.EqualTo(1));
            Assert.That(result.ClearedPixelCount, Is.EqualTo(1));
            Assert.That(pixels[9].a, Is.EqualTo(0));
            Assert.That(pixels[12].a, Is.EqualTo(255));
        }

        [Test]
        public void ApplyKeepsLargeOutsideRegion()
        {
            Color32 body = new Color32(255, 0, 0, 255);
            Color32 outside = new Color32(0, 255, 0, 255);
            Color32 clear = new Color32(0, 0, 0, 0);
            Color32[] pixels =
            {
                clear, clear, clear, clear, clear,
                clear, body, body, outside, outside,
                clear, body, body, outside, outside,
                clear, body, body, clear, clear,
                clear, clear, clear, clear, clear
            };

            PaletteVariantSession session = new PaletteVariantSession
            {
                analyzeSettings = new AnalyzeSettings { alphaThreshold = 0 },
                edgeOutsideCleanupSettings = new EdgeOutsideCleanupSettings
                {
                    enabled = true,
                    maxDistancePixels = 1,
                    maxRegionPixels = 2
                }
            };

            EdgeOutsideCleanupResult result = new EdgeOutsideCleanupService().Apply(pixels, 5, 5, session);

            Assert.That(result.ClearedRegionCount, Is.EqualTo(0));
            Assert.That(pixels[3].a, Is.EqualTo(255));
        }

        [Test]
        public void ApplyInfersOutsideGroupForOpaqueImage()
        {
            Color32 background = new Color32(255, 220, 64, 255);
            Color32 body = new Color32(32, 96, 192, 255);
            Color32 outside = new Color32(255, 255, 255, 255);
            Color32[] pixels =
            {
                background, background, background, background, background,
                background, body,       body,       background, outside,
                background, body,       body,       background, background,
                background, body,       body,       background, background,
                background, background, background, background, background
            };

            PaletteVariantSession session = new PaletteVariantSession
            {
                analyzeSettings = new AnalyzeSettings { alphaThreshold = 0, quantizeStep = 1 },
                edgeOutsideCleanupSettings = new EdgeOutsideCleanupSettings
                {
                    enabled = true,
                    maxDistancePixels = 2,
                    maxRegionPixels = 4
                }
            };
            session.colorGroups.Add(new ColorGroup
            {
                id = "background",
                displayName = "Background",
                representativeColor = background,
                pixelRatio = 0.72f
            });
            session.colorGroups.Add(new ColorGroup
            {
                id = "body",
                displayName = "Body",
                representativeColor = body,
                pixelRatio = 0.24f
            });
            session.colorGroups.Add(new ColorGroup
            {
                id = "outside",
                displayName = "Outside",
                representativeColor = outside,
                pixelRatio = 0.04f
            });
            session.paletteColors.Add(new PaletteColorEntry { id = "backgroundColor", color = background, groupId = "background" });
            session.paletteColors.Add(new PaletteColorEntry { id = "bodyColor", color = body, groupId = "body" });
            session.paletteColors.Add(new PaletteColorEntry { id = "outsideColor", color = outside, groupId = "outside" });

            EdgeOutsideCleanupResult result = new EdgeOutsideCleanupService().Apply(pixels, 5, 5, session);

            Assert.That(result.ClearedRegionCount, Is.EqualTo(1));
            Assert.That(result.ClearedPixelCount, Is.EqualTo(1));
            Assert.That(pixels[9].a, Is.EqualTo(0));
            Assert.That(pixels[6].a, Is.EqualTo(255));
            Assert.That(pixels[0].a, Is.EqualTo(255));
        }
    }
}
