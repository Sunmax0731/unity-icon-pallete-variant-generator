using System.IO;
using NUnit.Framework;
using Sunmax0731.IconPaletteVariantGenerator.Editor.Services;
using Sunmax0731.IconPaletteVariantGenerator.Models;
using Sunmax0731.IconPaletteVariantGenerator.Services;
using UnityEditor;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Editor.Tests
{
    public sealed class ExportedTextureImportSettingsServiceTests
    {
        private const string TestFolder = "Assets/__PaletteVariantExportImportSettingsTests";

        [SetUp]
        public void SetUp()
        {
            AssetDatabase.DeleteAsset(TestFolder);
            AssetDatabase.CreateFolder("Assets", "__PaletteVariantExportImportSettingsTests");
        }

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(TestFolder);
        }

        [Test]
        public void ApplyAlphaIsTransparencyEnablesImporterFlagForExportedPng()
        {
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixels32(new[] { new Color32(255, 0, 0, 0) });
            texture.Apply(false, false);

            ExportSettings settings = new ExportSettings
            {
                outputFolder = TestFolder,
                filePrefix = "alpha",
                fileSuffix = "transparent",
                conflictMode = ExportConflictMode.Overwrite,
                refreshAssetDatabase = false
            };

            PngExportResult result = new PngExportService().Export(texture, settings, Directory.GetCurrentDirectory());
            bool applied = new ExportedTextureImportSettingsService().ApplyAlphaIsTransparency(result.OutputPath, Directory.GetCurrentDirectory(), out string message);

            Assert.That(result.Status, Is.EqualTo(PngExportStatus.Exported));
            Assert.That(applied, Is.True, message);
            TextureImporter importer = AssetImporter.GetAtPath($"{TestFolder}/alpha_transparent.png") as TextureImporter;
            Assert.That(importer, Is.Not.Null);
            Assert.That(importer.alphaIsTransparency, Is.True);
            Assert.That(importer.alphaSource, Is.EqualTo(TextureImporterAlphaSource.FromInput));

            Object.DestroyImmediate(texture);
        }
    }
}
