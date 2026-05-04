using NUnit.Framework;
using Sunmax0731.IconPaletteVariantGenerator.Models;
using Sunmax0731.IconPaletteVariantGenerator.Services;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Editor.Tests
{
    public sealed class RasterPaintServiceTests
    {
        [Test]
        public void BrushWritesTargetColor()
        {
            Color32[] pixels = new Color32[9];
            RasterPaintService service = new RasterPaintService();
            service.ApplyTool(
                pixels,
                3,
                3,
                1,
                1,
                new DrawingToolSettings
                {
                    activeTool = DrawToolKind.Brush,
                    brushSize = 3,
                    strength = 1f,
                    paintOpacity = 1f,
                    paintColor = new Color32(255, 0, 0, 255)
                });

            Assert.That(pixels[4].r, Is.EqualTo(255));
            Assert.That(pixels[4].a, Is.EqualTo(255));
        }

        [Test]
        public void EraserReducesAlpha()
        {
            Color32[] pixels =
            {
                new Color32(10, 20, 30, 255)
            };

            RasterPaintService service = new RasterPaintService();
            service.ApplyTool(
                pixels,
                1,
                1,
                0,
                0,
                new DrawingToolSettings
                {
                    activeTool = DrawToolKind.Eraser,
                    brushSize = 1,
                    strength = 0.5f
                });

            Assert.That(pixels[0].a, Is.LessThan(255));
        }

        [Test]
        public void BlurMutatesCenterPixel()
        {
            Color32[] pixels =
            {
                new Color32(255, 0, 0, 255), new Color32(255, 0, 0, 255), new Color32(255, 0, 0, 255),
                new Color32(255, 0, 0, 255), new Color32(0, 0, 255, 255), new Color32(255, 0, 0, 255),
                new Color32(255, 0, 0, 255), new Color32(255, 0, 0, 255), new Color32(255, 0, 0, 255)
            };

            RasterPaintService service = new RasterPaintService();
            service.ApplyTool(
                pixels,
                3,
                3,
                1,
                1,
                new DrawingToolSettings
                {
                    activeTool = DrawToolKind.Blur,
                    brushSize = 3,
                    blurRadius = 1,
                    strength = 1f
                });

            Assert.That(pixels[4].r, Is.GreaterThan(0));
            Assert.That(pixels[4].b, Is.LessThan(255));
        }

        [Test]
        public void NoiseRemovalCanFillHighContrastSinglePixelNoise()
        {
            Color32[] pixels =
            {
                new Color32(0, 255, 0, 255), new Color32(0, 255, 0, 255), new Color32(0, 255, 0, 255),
                new Color32(0, 255, 0, 255), new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255),
                new Color32(0, 255, 0, 255), new Color32(0, 255, 0, 255), new Color32(0, 255, 0, 255)
            };

            RasterPaintService service = new RasterPaintService();
            service.ApplyTool(
                pixels,
                3,
                3,
                1,
                1,
                new DrawingToolSettings
                {
                    activeTool = DrawToolKind.NoiseRemoval,
                    brushSize = 3,
                    noiseRegionPixels = 1,
                    noiseThreshold = 442f
                });

            Assert.That(pixels[4].r, Is.EqualTo(0));
            Assert.That(pixels[4].g, Is.EqualTo(255));
            Assert.That(pixels[4].b, Is.EqualTo(0));
            Assert.That(pixels[4].a, Is.EqualTo(255));
        }
    }
}
