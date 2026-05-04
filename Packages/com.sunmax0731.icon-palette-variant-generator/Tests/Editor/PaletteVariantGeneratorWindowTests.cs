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
        public void DirectSourceEraserClearsAlphaAndPersistsAfterPreviewRefresh()
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
            window.SetDrawToolForValidation(DrawToolKind.Eraser);
            window.SetBrushSettingsForValidation(1, 1f);
            window.ApplyPaintAtSourcePixelForValidation(0, 0);

            Assert.That(window.GetSourcePixelForValidation(0, 0).a, Is.EqualTo(0));

            window.RefreshAfterPreviewForValidation();
            Assert.That(window.GetSourcePixelForValidation(0, 0).a, Is.EqualTo(0));

            window.Close();
            Object.DestroyImmediate(texture);
        }
    }
}
