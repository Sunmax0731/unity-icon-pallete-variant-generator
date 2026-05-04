using System.IO;
using NUnit.Framework;
using Sunmax0731.IconPaletteVariantGenerator.Editor.Services;
using Sunmax0731.IconPaletteVariantGenerator.Models;
using Sunmax0731.IconPaletteVariantGenerator.Services;
using UnityEditor;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Editor.Tests
{
    public sealed class TextureAssetLoaderTests
    {
        [Test]
        public void JpegSourceLoadsAsEditableRgbaTexture()
        {
            const string assetPath = "Assets/__PaletteVariantGeneratorTextureAssetLoaderTest.jpg";
            string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
            Texture2D source = null;
            Texture2D readable = null;
            Texture2D roundTripPng = null;

            try
            {
                source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                source.SetPixels32(new[]
                {
                    new Color32(255, 0, 0, 255),
                    new Color32(255, 255, 0, 255),
                    new Color32(0, 255, 0, 255),
                    new Color32(0, 0, 255, 255)
                });
                source.Apply(false, false);
                File.WriteAllBytes(absolutePath, source.EncodeToJPG(90));
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);

                Texture2D imported = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                Assert.That(imported, Is.Not.Null);

                TextureAssetLoader loader = new TextureAssetLoader();
                Assert.That(loader.TryLoadReadableTexture(imported, out readable, out _, out string error), Is.True, error);
                Assert.That(readable.format, Is.EqualTo(TextureFormat.RGBA32));

                Color32[] pixels = readable.GetPixels32();
                new RasterPaintService().ApplyTool(
                    pixels,
                    readable.width,
                    readable.height,
                    0,
                    0,
                    new DrawingToolSettings
                    {
                        activeTool = DrawToolKind.Eraser,
                        brushSize = 1,
                        strength = 1f
                    });
                readable.SetPixels32(pixels);
                readable.Apply(false, false);

                Assert.That(readable.GetPixels32()[0].a, Is.EqualTo(0));

                byte[] pngBytes = readable.EncodeToPNG();
                roundTripPng = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                Assert.That(ImageConversion.LoadImage(roundTripPng, pngBytes, false), Is.True);
                Assert.That(roundTripPng.GetPixels32()[0].a, Is.EqualTo(0));
                Assert.That(TextureAssetLoader.IsJpegAssetPath(assetPath), Is.True);
            }
            finally
            {
                if (roundTripPng != null)
                {
                    Object.DestroyImmediate(roundTripPng);
                }

                if (readable != null)
                {
                    Object.DestroyImmediate(readable);
                }

                if (source != null)
                {
                    Object.DestroyImmediate(source);
                }

                AssetDatabase.DeleteAsset(assetPath);
            }
        }
    }
}
