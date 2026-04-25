using System;
using System.IO;
using NUnit.Framework;
using Sunmax0731.IconPaletteVariantGenerator.Models;
using Sunmax0731.IconPaletteVariantGenerator.Services;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Editor.Tests
{
    public sealed class PngExportServiceTests
    {
        private string tempRoot;

        [SetUp]
        public void SetUp()
        {
            tempRoot = Path.Combine(Path.GetTempPath(), "IconPaletteVariantGeneratorTests", Guid.NewGuid().ToString("N"));
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
        public void ExportWritesPng()
        {
            Texture2D texture = CreateTexture();
            ExportSettings settings = new ExportSettings
            {
                outputFolder = "Generated",
                filePrefix = "ore",
                fileSuffix = "blue",
                conflictMode = ExportConflictMode.Duplicate
            };

            PngExportResult result = new PngExportService().Export(texture, settings, tempRoot);

            Assert.That(result.Status, Is.EqualTo(PngExportStatus.Exported));
            Assert.That(File.Exists(result.OutputPath), Is.True);
            Assert.That(Path.GetFileName(result.OutputPath), Is.EqualTo("ore_blue.png"));

            UnityEngine.Object.DestroyImmediate(texture);
        }

        [Test]
        public void ExportDuplicatesExistingFileWhenRequested()
        {
            Texture2D texture = CreateTexture();
            string outputFolder = Path.Combine(tempRoot, "Generated");
            Directory.CreateDirectory(outputFolder);
            File.WriteAllText(Path.Combine(outputFolder, "ore_blue.png"), "existing");

            ExportSettings settings = new ExportSettings
            {
                outputFolder = "Generated",
                filePrefix = "ore",
                fileSuffix = "blue",
                conflictMode = ExportConflictMode.Duplicate
            };

            PngExportResult result = new PngExportService().Export(texture, settings, tempRoot);

            Assert.That(result.Status, Is.EqualTo(PngExportStatus.Exported));
            Assert.That(Path.GetFileName(result.OutputPath), Is.EqualTo("ore_blue_001.png"));

            UnityEngine.Object.DestroyImmediate(texture);
        }

        private static Texture2D CreateTexture()
        {
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixels32(new[] { new Color32(0, 0, 255, 255) });
            texture.Apply();
            return texture;
        }
    }
}
