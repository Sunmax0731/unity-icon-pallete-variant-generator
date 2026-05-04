using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Sunmax0731.IconPaletteVariantGenerator.Models;
using Sunmax0731.IconPaletteVariantGenerator.Services;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Editor.Tests
{
    public sealed class SessionJsonServiceTests
    {
        private string tempRoot;

        [SetUp]
        public void SetUp()
        {
            tempRoot = Path.Combine(Path.GetTempPath(), "IconPaletteSessionTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempRoot);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, true);
            }
        }

        [Test]
        public void SaveAndLoadRestoresSession()
        {
            string path = Path.Combine(tempRoot, "session.json");
            PaletteVariantSession session = new PaletteVariantSession
            {
                sourceImageAssetPath = "Assets/Icons/source.png",
                analyzeSettings = new AnalyzeSettings { alphaThreshold = 16, quantizeStep = 8 },
                edgeOutsideCleanupSettings = new EdgeOutsideCleanupSettings
                {
                    enabled = true,
                    maxDistancePixels = 2,
                    maxRegionPixels = 9
                },
                noiseRemovalSettings = new NoiseRemovalSettings
                {
                    enabled = true,
                    maxRegionPixels = 3,
                    neighborDistanceThreshold = 24f,
                    sameGroupOnly = true
                },
                drawingToolSettings = new DrawingToolSettings
                {
                    paintTarget = PaintEditTarget.SourceImage,
                    activeTool = DrawToolKind.Blur,
                    brushSize = 11,
                    strength = 0.6f,
                    paintOpacity = 0.7f
                },
                sourcePixelData = new LayerPixelData
                {
                    width = 1,
                    height = 1,
                    rgbaBytesBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3, 255 })
                },
                paletteColors = new List<PaletteColorEntry>
                {
                    new PaletteColorEntry
                    {
                        id = "#FF0000",
                        hex = "#FF0000",
                        color = new Color32(255, 0, 0, 255),
                        pixelCount = 4,
                        groupId = "group_01"
                    }
                },
                colorGroups = new List<ColorGroup>
                {
                    new ColorGroup
                    {
                        id = "group_01",
                        targetColor = new Color32(0, 0, 255, 255),
                        blendRatio = 0.5f
                    }
                },
                layers = new List<RasterLayer>
                {
                    new RasterLayer
                    {
                        id = "layer_01",
                        displayName = "Paint Layer 1",
                        kind = LayerKind.Paint,
                        opacity = 0.5f,
                        pixelData = new LayerPixelData
                        {
                            width = 1,
                            height = 1,
                            rgbaBytesBase64 = Convert.ToBase64String(new byte[] { 255, 0, 0, 128 })
                        }
                    }
                },
                activeLayerId = "layer_01",
                variations = new List<IconVariation>
                {
                    new IconVariation
                    {
                        id = "variation_01",
                        displayName = "Blue",
                        fileSuffix = "blue",
                        exportEnabled = true,
                        colorGroups = new List<ColorGroup>
                        {
                            new ColorGroup
                            {
                                id = "group_01",
                                targetColor = new Color32(0, 0, 255, 255),
                                blendRatio = 0.5f
                            }
                        }
                    }
                }
            };

            SessionJsonService service = new SessionJsonService();
            service.Save(path, session);
            SessionLoadResult result = service.Load(path);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Session.analyzeSettings.alphaThreshold, Is.EqualTo(16));
            Assert.That(result.Session.edgeOutsideCleanupSettings.enabled, Is.True);
            Assert.That(result.Session.edgeOutsideCleanupSettings.maxDistancePixels, Is.EqualTo(2));
            Assert.That(result.Session.edgeOutsideCleanupSettings.maxRegionPixels, Is.EqualTo(9));
            Assert.That(result.Session.noiseRemovalSettings.enabled, Is.True);
            Assert.That(result.Session.noiseRemovalSettings.maxRegionPixels, Is.EqualTo(3));
            Assert.That(result.Session.noiseRemovalSettings.neighborDistanceThreshold, Is.EqualTo(24f));
            Assert.That(result.Session.noiseRemovalSettings.sameGroupOnly, Is.True);
            Assert.That(result.Session.drawingToolSettings.paintTarget, Is.EqualTo(PaintEditTarget.SourceImage));
            Assert.That(result.Session.drawingToolSettings.activeTool, Is.EqualTo(DrawToolKind.Blur));
            Assert.That(result.Session.drawingToolSettings.brushSize, Is.EqualTo(11));
            Assert.That(result.Session.sourcePixelData.width, Is.EqualTo(1));
            Assert.That(result.Session.paletteColors, Has.Count.EqualTo(1));
            Assert.That(result.Session.colorGroups, Has.Count.EqualTo(1));
            Assert.That(result.Session.colorGroups[0].blendRatio, Is.EqualTo(0.5f));
            Assert.That(result.Session.layers, Has.Count.EqualTo(1));
            Assert.That(result.Session.activeLayerId, Is.EqualTo("layer_01"));
            Assert.That(result.Session.variations, Has.Count.EqualTo(1));
            Assert.That(result.Session.variations[0].fileSuffix, Is.EqualTo("blue"));
        }

        [Test]
        public void LoadWarnsWhenSourceImageMissing()
        {
            string path = Path.Combine(tempRoot, "session.json");
            PaletteVariantSession session = new PaletteVariantSession
            {
                sourceImageAssetPath = "Assets/Missing/source.png"
            };

            SessionJsonService service = new SessionJsonService();
            service.Save(path, session);
            SessionLoadResult result = service.Load(path);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Warnings, Has.Some.Contains("Source image was not found"));
        }
    }
}
