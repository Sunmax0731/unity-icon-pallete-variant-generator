using NUnit.Framework;
using Sunmax0731.IconPaletteVariantGenerator.Models;
using Sunmax0731.IconPaletteVariantGenerator.Services;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Editor.Tests
{
    public sealed class LayerCompositingServiceTests
    {
        [Test]
        public void ComposeBlendsVisibleLayerOverBaseTexture()
        {
            Texture2D baseTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            baseTexture.SetPixels32(new[] { new Color32(0, 0, 255, 255) });
            baseTexture.Apply();

            LayerCompositingService service = new LayerCompositingService();
            RasterLayer layer = service.CreatePaintLayer("layer_01", "Paint", 1, 1);
            layer.pixelData = new LayerTextureSerializationService().Serialize(
                new[] { new Color32(255, 0, 0, 255) },
                1,
                1);
            layer.opacity = 0.5f;

            Texture2D output = service.Compose(baseTexture, new[] { layer });
            Color32 pixel = output.GetPixels32()[0];

            Assert.That(pixel.r, Is.GreaterThan(0));
            Assert.That(pixel.b, Is.GreaterThan(0));
            Assert.That(pixel.a, Is.EqualTo(255));

            Object.DestroyImmediate(baseTexture);
            Object.DestroyImmediate(output);
        }

        [Test]
        public void UpdateCompositeTextureOverwritesExistingTexturePixels()
        {
            Texture2D baseTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            baseTexture.SetPixels32(new[] { new Color32(0, 0, 255, 255) });
            baseTexture.Apply();

            Texture2D targetTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            targetTexture.SetPixels32(new[] { new Color32(255, 255, 255, 255) });
            targetTexture.Apply();

            LayerCompositingService service = new LayerCompositingService();
            RasterLayer layer = service.CreatePaintLayer("layer_01", "Paint", 1, 1);
            layer.pixelData = new LayerTextureSerializationService().Serialize(
                new[] { new Color32(255, 0, 0, 255) },
                1,
                1);
            layer.opacity = 1f;

            service.UpdateCompositeTexture(targetTexture, baseTexture, new[] { layer });
            Color32 pixel = targetTexture.GetPixels32()[0];

            Assert.That(pixel.r, Is.EqualTo(255));
            Assert.That(pixel.g, Is.EqualTo(0));
            Assert.That(pixel.b, Is.EqualTo(0));
            Assert.That(pixel.a, Is.EqualTo(255));

            Object.DestroyImmediate(baseTexture);
            Object.DestroyImmediate(targetTexture);
        }

        [Test]
        public void UpdateCompositeTextureUsesOverridePixelsForMatchingLayer()
        {
            Texture2D baseTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            baseTexture.SetPixels32(new[] { new Color32(0, 0, 255, 255) });
            baseTexture.Apply();

            Texture2D targetTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            targetTexture.Apply();

            LayerCompositingService service = new LayerCompositingService();
            RasterLayer layer = service.CreatePaintLayer("layer_01", "Paint", 1, 1);
            layer.opacity = 1f;
            layer.pixelData = new LayerTextureSerializationService().Serialize(
                new[] { new Color32(0, 255, 0, 255) },
                1,
                1);

            service.UpdateCompositeTexture(
                targetTexture,
                baseTexture,
                new[] { layer },
                "layer_01",
                new[] { new Color32(255, 0, 0, 255) },
                1,
                1);

            Color32 pixel = targetTexture.GetPixels32()[0];
            Assert.That(pixel.r, Is.EqualTo(255));
            Assert.That(pixel.g, Is.EqualTo(0));
            Assert.That(pixel.b, Is.EqualTo(0));

            Object.DestroyImmediate(baseTexture);
            Object.DestroyImmediate(targetTexture);
        }
    }
}
