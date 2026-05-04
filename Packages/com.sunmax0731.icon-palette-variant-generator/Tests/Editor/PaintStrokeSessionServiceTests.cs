using NUnit.Framework;
using Sunmax0731.IconPaletteVariantGenerator.Editor.Services;
using Sunmax0731.IconPaletteVariantGenerator.Models;
using Sunmax0731.IconPaletteVariantGenerator.Services;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Editor.Tests
{
    public sealed class PaintStrokeSessionServiceTests
    {
        [Test]
        public void SourceStrokeBrushThenEraserClearsPaintedPixelAlpha()
        {
            LayerTextureSerializationService serializationService = new LayerTextureSerializationService();
            PaintStrokeSessionService service = new PaintStrokeSessionService(serializationService, new RasterPaintService());
            PaletteVariantSession session = new PaletteVariantSession
            {
                sourcePixelData = serializationService.Serialize(
                    new[]
                    {
                        new Color32(255, 255, 0, 255)
                    },
                    1,
                    1),
                drawingToolSettings = new DrawingToolSettings
                {
                    paintTarget = PaintEditTarget.SourceImage,
                    activeTool = DrawToolKind.Brush,
                    brushSize = 1,
                    strength = 1f,
                    paintOpacity = 1f,
                    paintColor = new Color32(255, 0, 0, 255)
                }
            };

            PaintStrokeSession brushStroke = service.Begin(session, null, null);
            service.Apply(brushStroke, session.drawingToolSettings, 0, 0);
            PaintStrokeCommitResult brushCommit = service.Commit(brushStroke, session, null);

            Assert.That(brushCommit.Changed, Is.True);
            Assert.That(brushCommit.Pixels[0].r, Is.EqualTo(255));
            Assert.That(brushCommit.Pixels[0].g, Is.EqualTo(0));
            Assert.That(brushCommit.Pixels[0].a, Is.EqualTo(255));

            session.drawingToolSettings.activeTool = DrawToolKind.Eraser;

            PaintStrokeSession eraseStroke = service.Begin(session, null, null);
            service.Apply(eraseStroke, session.drawingToolSettings, 0, 0);
            PaintStrokeCommitResult eraseCommit = service.Commit(eraseStroke, session, null);

            Assert.That(eraseCommit.Changed, Is.True);
            Assert.That(eraseCommit.Pixels[0].r, Is.EqualTo(255));
            Assert.That(eraseCommit.Pixels[0].g, Is.EqualTo(0));
            Assert.That(eraseCommit.Pixels[0].a, Is.EqualTo(0));
        }

        [Test]
        public void ActiveLayerStrokeBrushThenEraserClearsPaintedPixelAlpha()
        {
            LayerTextureSerializationService serializationService = new LayerTextureSerializationService();
            PaintStrokeSessionService service = new PaintStrokeSessionService(serializationService, new RasterPaintService());
            RasterLayer layer = new RasterLayer
            {
                id = "layer",
                pixelData = serializationService.CreateBlank(1, 1)
            };

            PaletteVariantSession session = new PaletteVariantSession
            {
                drawingToolSettings = new DrawingToolSettings
                {
                    paintTarget = PaintEditTarget.ActiveLayer,
                    activeTool = DrawToolKind.Brush,
                    brushSize = 1,
                    strength = 1f,
                    paintOpacity = 1f,
                    paintColor = new Color32(255, 0, 0, 255)
                }
            };

            PaintStrokeSession brushStroke = service.Begin(session, null, layer);
            service.Apply(brushStroke, session.drawingToolSettings, 0, 0);
            PaintStrokeCommitResult brushCommit = service.Commit(brushStroke, session, layer);

            Assert.That(brushCommit.Pixels[0].a, Is.EqualTo(255));

            session.drawingToolSettings.activeTool = DrawToolKind.Eraser;

            PaintStrokeSession eraseStroke = service.Begin(session, null, layer);
            service.Apply(eraseStroke, session.drawingToolSettings, 0, 0);
            PaintStrokeCommitResult eraseCommit = service.Commit(eraseStroke, session, layer);

            Assert.That(eraseCommit.Pixels[0].a, Is.EqualTo(0));
        }
    }
}
