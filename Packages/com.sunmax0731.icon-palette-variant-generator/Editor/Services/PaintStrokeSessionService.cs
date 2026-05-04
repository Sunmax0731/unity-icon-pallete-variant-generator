using System;
using Sunmax0731.IconPaletteVariantGenerator.Models;
using Sunmax0731.IconPaletteVariantGenerator.Services;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Editor.Services
{
    /// <summary>
    /// Builds and commits paint stroke buffers independently from the EditorWindow UI.
    /// </summary>
    internal sealed class PaintStrokeSessionService
    {
        private readonly LayerTextureSerializationService serializationService;
        private readonly RasterPaintService rasterPaintService;

        internal PaintStrokeSessionService(
            LayerTextureSerializationService serializationService,
            RasterPaintService rasterPaintService)
        {
            this.serializationService = serializationService ?? throw new ArgumentNullException(nameof(serializationService));
            this.rasterPaintService = rasterPaintService ?? throw new ArgumentNullException(nameof(rasterPaintService));
        }

        internal PaintStrokeSession Begin(
            PaletteVariantSession session,
            Texture2D readableSourceImage,
            RasterLayer activeLayer)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            DrawingToolSettings settings = session.drawingToolSettings ?? new DrawingToolSettings();
            if (settings.paintTarget == PaintEditTarget.SourceImage)
            {
                if (session.sourcePixelData == null || session.sourcePixelData.width <= 0 || session.sourcePixelData.height <= 0)
                {
                    return null;
                }

                return new PaintStrokeSession(
                    PaintEditTarget.SourceImage,
                    null,
                    serializationService.Deserialize(session.sourcePixelData),
                    session.sourcePixelData.width,
                    session.sourcePixelData.height);
            }

            if (activeLayer?.pixelData == null)
            {
                return null;
            }

            Color32[] pixels = serializationService.Deserialize(activeLayer.pixelData);
            if (pixels.Length == 0)
            {
                pixels = new Color32[activeLayer.pixelData.width * activeLayer.pixelData.height];
            }

            return new PaintStrokeSession(
                PaintEditTarget.ActiveLayer,
                activeLayer.id,
                pixels,
                activeLayer.pixelData.width,
                activeLayer.pixelData.height);
        }

        internal void Apply(PaintStrokeSession session, DrawingToolSettings settings, int x, int y)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            Vector2Int currentPoint = new Vector2Int(x, y);
            if (ShouldInterpolate(settings.activeTool) && session.HasLastPoint)
            {
                rasterPaintService.ApplyStroke(
                    session.Pixels,
                    session.Width,
                    session.Height,
                    session.LastPoint,
                    currentPoint,
                    settings);
            }
            else
            {
                rasterPaintService.ApplyTool(
                    session.Pixels,
                    session.Width,
                    session.Height,
                    x,
                    y,
                    settings);
            }

            session.MarkDirty(currentPoint);
        }

        internal PaintStrokeCommitResult Commit(
            PaintStrokeSession strokeSession,
            PaletteVariantSession paletteSession,
            RasterLayer activeLayer)
        {
            if (strokeSession == null)
            {
                return PaintStrokeCommitResult.None;
            }

            if (!strokeSession.IsDirty)
            {
                return new PaintStrokeCommitResult(strokeSession.Target, strokeSession.LayerId, strokeSession.Pixels, strokeSession.Width, strokeSession.Height, false);
            }

            LayerPixelData pixelData = serializationService.Serialize(strokeSession.Pixels, strokeSession.Width, strokeSession.Height);
            if (strokeSession.Target == PaintEditTarget.SourceImage)
            {
                paletteSession.sourcePixelData = pixelData;
            }
            else if (activeLayer?.pixelData != null)
            {
                activeLayer.pixelData = pixelData;
            }

            return new PaintStrokeCommitResult(strokeSession.Target, strokeSession.LayerId, strokeSession.Pixels, strokeSession.Width, strokeSession.Height, true);
        }

        private static bool ShouldInterpolate(DrawToolKind tool)
        {
            return tool == DrawToolKind.Brush || tool == DrawToolKind.Eraser;
        }
    }

    /// <summary>
    /// Mutable stroke buffer for the current drag interaction.
    /// </summary>
    internal sealed class PaintStrokeSession
    {
        internal PaintStrokeSession(PaintEditTarget target, string layerId, Color32[] pixels, int width, int height)
        {
            Target = target;
            LayerId = layerId ?? string.Empty;
            Pixels = pixels ?? Array.Empty<Color32>();
            Width = width;
            Height = height;
        }

        internal PaintEditTarget Target { get; }

        internal string LayerId { get; }

        internal Color32[] Pixels { get; }

        internal int Width { get; }

        internal int Height { get; }

        internal bool IsDirty { get; private set; }

        internal bool HasLastPoint { get; private set; }

        internal Vector2Int LastPoint { get; private set; }

        internal void MarkDirty(Vector2Int point)
        {
            IsDirty = true;
            HasLastPoint = true;
            LastPoint = point;
        }
    }

    /// <summary>
    /// Result of committing a stroke buffer back into session storage.
    /// </summary>
    internal readonly struct PaintStrokeCommitResult
    {
        internal static PaintStrokeCommitResult None => new PaintStrokeCommitResult(PaintEditTarget.ActiveLayer, string.Empty, null, 0, 0, false);

        internal PaintStrokeCommitResult(PaintEditTarget target, string layerId, Color32[] pixels, int width, int height, bool changed)
        {
            Target = target;
            LayerId = layerId ?? string.Empty;
            Pixels = pixels;
            Width = width;
            Height = height;
            Changed = changed;
        }

        internal PaintEditTarget Target { get; }

        internal string LayerId { get; }

        internal Color32[] Pixels { get; }

        internal int Width { get; }

        internal int Height { get; }

        internal bool Changed { get; }
    }
}
