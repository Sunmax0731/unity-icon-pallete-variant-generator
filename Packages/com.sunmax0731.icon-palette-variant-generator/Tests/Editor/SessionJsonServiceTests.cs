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
                }
            };

            SessionJsonService service = new SessionJsonService();
            service.Save(path, session);
            SessionLoadResult result = service.Load(path);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Session.analyzeSettings.alphaThreshold, Is.EqualTo(16));
            Assert.That(result.Session.paletteColors, Has.Count.EqualTo(1));
            Assert.That(result.Session.colorGroups, Has.Count.EqualTo(1));
            Assert.That(result.Session.colorGroups[0].blendRatio, Is.EqualTo(0.5f));
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
