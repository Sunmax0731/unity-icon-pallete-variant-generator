using NUnit.Framework;
using Sunmax0731.IconPaletteVariantGenerator.Editor.Windows;
using Sunmax0731.IconPaletteVariantGenerator.Models;
using UnityEditor;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Editor.Tests
{
    public sealed class PaletteVariantGeneratorWindowTests
    {
        [Test]
        public void OpenCreatesWindowWithExpectedTitle()
        {
            PaletteVariantGeneratorWindow.Open();

            PaletteVariantGeneratorWindow window = EditorWindow.GetWindow<PaletteVariantGeneratorWindow>();

            Assert.That(window, Is.Not.Null);
            Assert.That(window.titleContent.text, Is.EqualTo("Palette Variant Generator"));

            window.Close();
        }

        [Test]
        public void DirectSourceBrushThenEraserClearsAlphaAndPersistsAfterPreviewRefresh()
        {
            PaletteVariantGeneratorWindow.Open();
            PaletteVariantGeneratorWindow window = EditorWindow.GetWindow<PaletteVariantGeneratorWindow>();
            Assert.That(window, Is.Not.Null);

            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
            {
                name = "DirectSourceEraserValidation"
            };
            texture.SetPixels32(new[]
            {
                new Color32(255, 0, 0, 255),
                new Color32(0, 0, 255, 255),
                new Color32(255, 0, 0, 255),
                new Color32(0, 0, 255, 255)
            });
            texture.Apply();

            PaletteVariantSession session = new PaletteVariantSession
            {
                analyzeSettings = new AnalyzeSettings { alphaThreshold = 0, quantizeStep = 1 },
                drawingToolSettings = new DrawingToolSettings
                {
                    paintTarget = PaintEditTarget.SourceImage,
                    activeTool = DrawToolKind.Eraser,
                    brushSize = 1,
                    strength = 1f
                }
            };

            window.SetValidationSession(texture, session);
            window.SetPreviewInteractionModeForValidation(PreviewInteractionMode.Paint);
            window.SetPaintTargetForValidation(PaintEditTarget.SourceImage);
            window.SetDrawToolForValidation(DrawToolKind.Brush);
            window.SetBrushSettingsForValidation(1, 1f);
            window.ApplyPaintAtSourcePixelForValidation(0, 0);

            Assert.That(window.GetSourcePixelForValidation(0, 0).r, Is.EqualTo(255));
            Assert.That(window.GetSourcePixelForValidation(0, 0).g, Is.EqualTo(64));
            Assert.That(window.GetSourcePixelForValidation(0, 0).a, Is.EqualTo(255));

            window.SetPaintTargetForValidation(PaintEditTarget.SourceImage);
            window.SetDrawToolForValidation(DrawToolKind.Eraser);
            window.SetBrushSettingsForValidation(1, 1f);

            Color32 beforePreview = window.GetPrimaryPreviewPixelForValidation(0, 0);
            window.ApplyPaintAtSourcePixelForValidation(0, 0);

            Assert.That(window.GetSourcePixelForValidation(0, 0).a, Is.EqualTo(0));
            Assert.That(window.GetPrimaryPreviewPixelForValidation(0, 0), Is.Not.EqualTo(beforePreview));

            window.RefreshAfterPreviewForValidation();
            Assert.That(window.GetSourcePixelForValidation(0, 0).a, Is.EqualTo(0));
            Assert.That(window.GetPrimaryPreviewPixelForValidation(0, 0), Is.Not.EqualTo(beforePreview));

            window.Close();
            Object.DestroyImmediate(texture);
        }

        [Test]
        public void DirectSourceEraserClearsAlphaOnRgbSourceBuffer()
        {
            PaletteVariantGeneratorWindow.Open();
            PaletteVariantGeneratorWindow window = EditorWindow.GetWindow<PaletteVariantGeneratorWindow>();
            Assert.That(window, Is.Not.Null);

            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGB24, false)
            {
                name = "DirectSourceRgbEraserValidation"
            };
            texture.SetPixels32(new[]
            {
                new Color32(255, 0, 0, 255),
                new Color32(255, 255, 0, 255),
                new Color32(0, 255, 0, 255),
                new Color32(0, 0, 255, 255)
            });
            texture.Apply();

            window.SetValidationSession(
                texture,
                new PaletteVariantSession
                {
                    drawingToolSettings = new DrawingToolSettings
                    {
                        paintTarget = PaintEditTarget.SourceImage,
                        activeTool = DrawToolKind.Eraser,
                        brushSize = 1,
                        strength = 1f
                    }
                });
            window.SetPreviewInteractionModeForValidation(PreviewInteractionMode.Paint);
            window.ApplyPaintAtSourcePixelForValidation(0, 0);

            Assert.That(window.GetSourcePixelForValidation(0, 0).a, Is.EqualTo(0));

            window.Close();
            Object.DestroyImmediate(texture);
        }

        [Test]
        public void DirectSourceFillRecolorsContiguousMatchingPixels()
        {
            PaletteVariantGeneratorWindow.Open();
            PaletteVariantGeneratorWindow window = EditorWindow.GetWindow<PaletteVariantGeneratorWindow>();
            Assert.That(window, Is.Not.Null);

            Texture2D texture = new Texture2D(3, 1, TextureFormat.RGBA32, false)
            {
                name = "DirectSourceFillValidation"
            };
            Color32 red = new Color32(255, 0, 0, 255);
            texture.SetPixels32(new[]
            {
                red,
                red,
                new Color32(0, 0, 255, 255)
            });
            texture.Apply();

            window.SetValidationSession(
                texture,
                new PaletteVariantSession
                {
                    drawingToolSettings = new DrawingToolSettings
                    {
                        paintTarget = PaintEditTarget.SourceImage,
                        activeTool = DrawToolKind.Fill,
                        strength = 1f,
                        paintOpacity = 1f,
                        paintColor = new Color32(0, 255, 0, 128)
                    }
                });
            window.SetPreviewInteractionModeForValidation(PreviewInteractionMode.Paint);
            window.ApplyPaintAtSourcePixelForValidation(0, 0);

            Assert.That(window.GetSourcePixelForValidation(0, 0), Is.EqualTo(new Color32(0, 255, 0, 128)));
            Assert.That(window.GetSourcePixelForValidation(1, 0), Is.EqualTo(new Color32(0, 255, 0, 128)));
            Assert.That(window.GetSourcePixelForValidation(2, 0), Is.EqualTo(new Color32(0, 0, 255, 255)));

            window.Close();
            Object.DestroyImmediate(texture);
        }

        [Test]
        public void ToolPopupValuesStaySyncedAfterSessionReplacement()
        {
            PaletteVariantGeneratorWindow.Open();
            PaletteVariantGeneratorWindow window = EditorWindow.GetWindow<PaletteVariantGeneratorWindow>();
            Assert.That(window, Is.Not.Null);

            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                name = "ToolPopupSyncValidation"
            };
            texture.SetPixels32(new[] { new Color32(255, 255, 0, 255) });
            texture.Apply();

            window.SetValidationSession(
                texture,
                new PaletteVariantSession
                {
                    drawingToolSettings = new DrawingToolSettings
                    {
                        paintTarget = PaintEditTarget.SourceImage,
                        activeTool = DrawToolKind.Brush
                    }
                });
            Assert.That(window.IsToolPopupStateSyncedForValidation(), Is.True);

            window.SetValidationSession(
                texture,
                new PaletteVariantSession
                {
                    drawingToolSettings = new DrawingToolSettings
                    {
                        paintTarget = PaintEditTarget.ActiveLayer,
                        activeTool = DrawToolKind.Eraser
                    }
                });
            Assert.That(window.IsToolPopupStateSyncedForValidation(), Is.True);

            window.Close();
            Object.DestroyImmediate(texture);
        }
    }
}
