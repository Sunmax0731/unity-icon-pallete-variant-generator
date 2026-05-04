using System;
using System.Collections.Generic;
using Sunmax0731.IconPaletteVariantGenerator.Models;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Services
{
    /// <summary>
    /// Applies raster editing tools to layer pixel buffers.
    /// </summary>
    public sealed class RasterPaintService
    {
        private const uint TransparentNeighborKey = uint.MaxValue;

        /// <summary>
        /// Applies the active tool continuously between two pixel coordinates.
        /// </summary>
        public void ApplyStroke(
            Color32[] pixels,
            int width,
            int height,
            Vector2Int from,
            Vector2Int to,
            DrawingToolSettings settings)
        {
            if (pixels == null)
            {
                throw new ArgumentNullException(nameof(pixels));
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            int radius = Mathf.Max(0, settings.brushSize / 2);
            float spacing = Mathf.Max(1f, radius * 0.5f);
            float distance = Vector2Int.Distance(from, to);
            int steps = Mathf.Max(1, Mathf.CeilToInt(distance / spacing));

            for (int step = 0; step <= steps; step++)
            {
                float t = steps == 0 ? 0f : step / (float)steps;
                int x = Mathf.RoundToInt(Mathf.Lerp(from.x, to.x, t));
                int y = Mathf.RoundToInt(Mathf.Lerp(from.y, to.y, t));
                ApplyTool(pixels, width, height, x, y, settings);
            }
        }

        /// <summary>
        /// Restores pixels continuously from a reference buffer between two coordinates.
        /// </summary>
        public void RestoreStrokeFromReference(
            Color32[] pixels,
            Color32[] referencePixels,
            int width,
            int height,
            Vector2Int from,
            Vector2Int to,
            DrawingToolSettings settings)
        {
            if (pixels == null)
            {
                throw new ArgumentNullException(nameof(pixels));
            }

            if (referencePixels == null)
            {
                throw new ArgumentNullException(nameof(referencePixels));
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            int radius = Mathf.Max(0, settings.brushSize / 2);
            float spacing = Mathf.Max(1f, radius * 0.5f);
            float distance = Vector2Int.Distance(from, to);
            int steps = Mathf.Max(1, Mathf.CeilToInt(distance / spacing));

            for (int step = 0; step <= steps; step++)
            {
                float t = steps == 0 ? 0f : step / (float)steps;
                int x = Mathf.RoundToInt(Mathf.Lerp(from.x, to.x, t));
                int y = Mathf.RoundToInt(Mathf.Lerp(from.y, to.y, t));
                RestoreFromReference(pixels, referencePixels, width, height, x, y, settings);
            }
        }

        public void ApplyTool(
            Color32[] pixels,
            int width,
            int height,
            int centerX,
            int centerY,
            DrawingToolSettings settings)
        {
            if (pixels == null)
            {
                throw new ArgumentNullException(nameof(pixels));
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            int radius = Mathf.Max(0, settings.brushSize / 2);
            switch (settings.activeTool)
            {
                case DrawToolKind.Brush:
                    Paint(pixels, width, height, centerX, centerY, radius, settings.paintColor, settings.paintOpacity, settings.strength);
                    break;
                case DrawToolKind.Eraser:
                    Erase(pixels, width, height, centerX, centerY, radius, settings.strength);
                    break;
                case DrawToolKind.Blur:
                    Blur(pixels, width, height, centerX, centerY, Mathf.Max(1, settings.blurRadius), settings.strength);
                    break;
                case DrawToolKind.Smooth:
                    Smooth(pixels, width, height, centerX, centerY, Mathf.Max(1, settings.blurRadius), Mathf.Max(1, settings.smoothIterations), settings.strength);
                    break;
                case DrawToolKind.NoiseRemoval:
                    RemoveNoise(pixels, width, height, centerX, centerY, radius, settings.noiseRegionPixels, settings.noiseThreshold);
                    break;
            }
        }

        /// <summary>
        /// Restores pixels from a reference buffer within the current brush region.
        /// </summary>
        public void RestoreFromReference(
            Color32[] pixels,
            Color32[] referencePixels,
            int width,
            int height,
            int centerX,
            int centerY,
            DrawingToolSettings settings)
        {
            if (pixels == null)
            {
                throw new ArgumentNullException(nameof(pixels));
            }

            if (referencePixels == null)
            {
                throw new ArgumentNullException(nameof(referencePixels));
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            int radius = Mathf.Max(0, settings.brushSize / 2);
            VisitCircle(width, height, centerX, centerY, radius, (x, y) =>
            {
                int index = (y * width) + x;
                pixels[index] = referencePixels[index];
            });
        }

        private static void Paint(Color32[] pixels, int width, int height, int centerX, int centerY, int radius, Color32 color, float opacity, float strength)
        {
            float alphaRatio = Mathf.Clamp01(opacity) * Mathf.Clamp01(strength);
            VisitCircle(width, height, centerX, centerY, radius, (x, y) =>
            {
                int index = (y * width) + x;
                pixels[index] = Lerp(pixels[index], color, alphaRatio);
            });
        }

        private static void Erase(Color32[] pixels, int width, int height, int centerX, int centerY, int radius, float strength)
        {
            float eraseRatio = Mathf.Clamp01(strength);
            VisitCircle(width, height, centerX, centerY, radius, (x, y) =>
            {
                int index = (y * width) + x;
                Color32 pixel = pixels[index];
                pixel.a = (byte)Mathf.Clamp(Mathf.RoundToInt(pixel.a * (1f - eraseRatio)), 0, 255);
                pixels[index] = pixel;
            });
        }

        private static void Blur(Color32[] pixels, int width, int height, int centerX, int centerY, int radius, float strength)
        {
            Color32[] source = new Color32[pixels.Length];
            Array.Copy(pixels, source, pixels.Length);
            float ratio = Mathf.Clamp01(strength);
            VisitCircle(width, height, centerX, centerY, radius, (x, y) =>
            {
                Color32 average = SampleAverage(source, width, height, x, y, radius);
                int index = (y * width) + x;
                pixels[index] = Lerp(source[index], average, ratio);
            });
        }

        private static void Smooth(Color32[] pixels, int width, int height, int centerX, int centerY, int radius, int iterations, float strength)
        {
            float ratio = Mathf.Clamp01(strength) * 0.5f;
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                Blur(pixels, width, height, centerX, centerY, radius, ratio);
            }
        }

        private static void RemoveNoise(Color32[] pixels, int width, int height, int centerX, int centerY, int radius, int maxRegionPixels, float threshold)
        {
            int minX = Mathf.Max(0, centerX - radius);
            int minY = Mathf.Max(0, centerY - radius);
            int maxX = Mathf.Min(width - 1, centerX + radius);
            int maxY = Mathf.Min(height - 1, centerY + radius);
            bool[] visited = new bool[pixels.Length];
            List<int> region = new List<int>();
            Queue<int> queue = new Queue<int>();

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (!InsideCircle(x, y, centerX, centerY, radius))
                    {
                        continue;
                    }

                    int startIndex = (y * width) + x;
                    if (visited[startIndex] || pixels[startIndex].a == 0)
                    {
                        continue;
                    }

                    region.Clear();
                    queue.Clear();
                    Color32 regionColor = pixels[startIndex];
                    visited[startIndex] = true;
                    queue.Enqueue(startIndex);

                    while (queue.Count > 0 && region.Count <= maxRegionPixels)
                    {
                        int currentIndex = queue.Dequeue();
                        region.Add(currentIndex);
                        int currentX = currentIndex % width;
                        int currentY = currentIndex / width;
                        EnqueueIfMatch(queue, visited, pixels, width, height, currentX + 1, currentY, regionColor);
                        EnqueueIfMatch(queue, visited, pixels, width, height, currentX - 1, currentY, regionColor);
                        EnqueueIfMatch(queue, visited, pixels, width, height, currentX, currentY + 1, regionColor);
                        EnqueueIfMatch(queue, visited, pixels, width, height, currentX, currentY - 1, regionColor);
                    }

                    if (region.Count == 0 || region.Count > maxRegionPixels)
                    {
                        continue;
                    }

                    if (!TryFindDominantNeighbor(pixels, width, height, region, threshold, out Color32 fill))
                    {
                        continue;
                    }

                    foreach (int index in region)
                    {
                        if (fill.a == 0)
                        {
                            pixels[index] = default;
                            continue;
                        }

                        Color32 pixel = fill;
                        pixel.a = pixels[index].a;
                        pixels[index] = pixel;
                    }
                }
            }
        }

        private static void EnqueueIfMatch(Queue<int> queue, bool[] visited, Color32[] pixels, int width, int height, int x, int y, Color32 target)
        {
            if (x < 0 || y < 0 || x >= width || y >= height)
            {
                return;
            }

            int index = (y * width) + x;
            if (visited[index] || !SameColor(pixels[index], target))
            {
                return;
            }

            visited[index] = true;
            queue.Enqueue(index);
        }

        private static bool TryFindDominantNeighbor(Color32[] pixels, int width, int height, IReadOnlyList<int> region, float threshold, out Color32 fill)
        {
            Dictionary<uint, int> counts = new Dictionary<uint, int>();
            Color32 regionColor = pixels[region[0]];
            HashSet<int> lookup = new HashSet<int>(region);
            foreach (int index in region)
            {
                int x = index % width;
                int y = index / width;
                CountNeighbor(counts, lookup, pixels, width, height, x + 1, y);
                CountNeighbor(counts, lookup, pixels, width, height, x - 1, y);
                CountNeighbor(counts, lookup, pixels, width, height, x, y + 1);
                CountNeighbor(counts, lookup, pixels, width, height, x, y - 1);
            }

            int bestCount = 0;
            Color32 bestColor = default;
            foreach ((uint key, int count) in counts)
            {
                Color32 candidate = key == TransparentNeighborKey
                    ? default
                    : FromRgbKey(key, regionColor.a);
                if (count > bestCount && (candidate.a == 0 || ColorDistance(regionColor, candidate) <= threshold))
                {
                    bestCount = count;
                    bestColor = candidate;
                }
            }

            fill = bestColor;
            return bestCount > 0;
        }

        private static void CountNeighbor(Dictionary<uint, int> counts, HashSet<int> regionLookup, Color32[] pixels, int width, int height, int x, int y)
        {
            if (x < 0 || y < 0 || x >= width || y >= height)
            {
                return;
            }

            int index = (y * width) + x;
            if (regionLookup.Contains(index))
            {
                return;
            }

            uint key = pixels[index].a == 0 ? TransparentNeighborKey : ToRgbKey(pixels[index]);
            counts.TryGetValue(key, out int count);
            counts[key] = count + 1;
        }

        private static Color32 SampleAverage(Color32[] pixels, int width, int height, int centerX, int centerY, int radius)
        {
            int totalR = 0;
            int totalG = 0;
            int totalB = 0;
            int totalA = 0;
            int count = 0;
            VisitCircle(width, height, centerX, centerY, radius, (x, y) =>
            {
                Color32 pixel = pixels[(y * width) + x];
                totalR += pixel.r;
                totalG += pixel.g;
                totalB += pixel.b;
                totalA += pixel.a;
                count++;
            });

            if (count == 0)
            {
                return default;
            }

            return new Color32(
                (byte)(totalR / count),
                (byte)(totalG / count),
                (byte)(totalB / count),
                (byte)(totalA / count));
        }

        private static void VisitCircle(int width, int height, int centerX, int centerY, int radius, Action<int, int> visit)
        {
            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                if (y < 0 || y >= height)
                {
                    continue;
                }

                for (int x = centerX - radius; x <= centerX + radius; x++)
                {
                    if (x < 0 || x >= width || !InsideCircle(x, y, centerX, centerY, radius))
                    {
                        continue;
                    }

                    visit(x, y);
                }
            }
        }

        private static bool InsideCircle(int x, int y, int centerX, int centerY, int radius)
        {
            int dx = x - centerX;
            int dy = y - centerY;
            return (dx * dx) + (dy * dy) <= (radius * radius);
        }

        private static Color32 Lerp(Color32 from, Color32 to, float ratio)
        {
            return new Color32(
                (byte)Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(from.r, to.r, ratio)), 0, 255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(from.g, to.g, ratio)), 0, 255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(from.b, to.b, ratio)), 0, 255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(from.a, to.a, ratio)), 0, 255));
        }

        private static bool SameColor(Color32 a, Color32 b)
        {
            return a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
        }

        private static float ColorDistance(Color32 a, Color32 b)
        {
            int dr = a.r - b.r;
            int dg = a.g - b.g;
            int db = a.b - b.b;
            return Mathf.Sqrt((dr * dr) + (dg * dg) + (db * db));
        }

        private static uint ToRgbKey(Color32 color)
        {
            return (uint)((color.r << 16) | (color.g << 8) | color.b);
        }

        private static Color32 FromRgbKey(uint key, byte alpha)
        {
            return new Color32(
                (byte)((key >> 16) & 0xFF),
                (byte)((key >> 8) & 0xFF),
                (byte)(key & 0xFF),
                alpha);
        }
    }
}
