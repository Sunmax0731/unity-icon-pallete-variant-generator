using System.Collections.Generic;
using System.Linq;
using Sunmax0731.IconPaletteVariantGenerator.Models;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Services
{
    /// <summary>
    /// Clears small foreground islands near the outside edge of the main opaque component.
    /// </summary>
    public sealed class EdgeOutsideCleanupService
    {
        private static readonly Vector2Int[] ConnectedOffsets =
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        public EdgeOutsideCleanupResult Apply(Color32[] pixels, int width, int height, PaletteVariantSession session)
        {
            if (pixels == null)
            {
                throw new System.ArgumentNullException(nameof(pixels));
            }

            if (session == null)
            {
                throw new System.ArgumentNullException(nameof(session));
            }

            EdgeOutsideCleanupSettings settings = session.edgeOutsideCleanupSettings ?? new EdgeOutsideCleanupSettings();
            if (!settings.enabled || pixels.Length == 0 || width <= 0 || height <= 0)
            {
                return new EdgeOutsideCleanupResult(0, 0);
            }

            int alphaThreshold = Mathf.Clamp(session.analyzeSettings.alphaThreshold, 0, 255);
            int maxRegionPixels = Mathf.Max(1, settings.maxRegionPixels);
            int maxDistancePixels = Mathf.Max(1, settings.maxDistancePixels);
            bool[] foreground = pixels.Select(pixel => pixel.a > alphaThreshold).ToArray();
            List<List<int>> regions = FindForegroundRegions(foreground, width, height);
            if (regions.Count <= 1)
            {
                return new EdgeOutsideCleanupResult(0, 0);
            }

            HashSet<int> mainRegion = new HashSet<int>(regions.OrderByDescending(region => region.Count).First());
            HashSet<int> nearMain = ExpandRegion(mainRegion, foreground.Length, width, height, maxDistancePixels);
            int clearedRegions = 0;
            int clearedPixels = 0;

            foreach (List<int> region in regions)
            {
                if (region.Count > maxRegionPixels || mainRegion.Contains(region[0]))
                {
                    continue;
                }

                bool isNearMain = region.Any(nearMain.Contains);
                if (!isNearMain)
                {
                    continue;
                }

                foreach (int index in region)
                {
                    Color32 pixel = pixels[index];
                    pixel.a = 0;
                    pixels[index] = pixel;
                }

                clearedRegions++;
                clearedPixels += region.Count;
            }

            return new EdgeOutsideCleanupResult(clearedRegions, clearedPixels);
        }

        private static List<List<int>> FindForegroundRegions(bool[] foreground, int width, int height)
        {
            List<List<int>> regions = new List<List<int>>();
            bool[] visited = new bool[foreground.Length];
            Queue<int> queue = new Queue<int>();

            for (int index = 0; index < foreground.Length; index++)
            {
                if (visited[index] || !foreground[index])
                {
                    continue;
                }

                List<int> region = new List<int>();
                visited[index] = true;
                queue.Enqueue(index);
                while (queue.Count > 0)
                {
                    int current = queue.Dequeue();
                    region.Add(current);
                    int x = current % width;
                    int y = current / width;
                    foreach (Vector2Int offset in ConnectedOffsets)
                    {
                        int nx = x + offset.x;
                        int ny = y + offset.y;
                        if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                        {
                            continue;
                        }

                        int neighbor = (ny * width) + nx;
                        if (visited[neighbor] || !foreground[neighbor])
                        {
                            continue;
                        }

                        visited[neighbor] = true;
                        queue.Enqueue(neighbor);
                    }
                }

                regions.Add(region);
            }

            return regions;
        }

        private static HashSet<int> ExpandRegion(HashSet<int> source, int pixelCount, int width, int height, int distance)
        {
            HashSet<int> result = new HashSet<int>(source);
            Queue<(int Index, int Distance)> queue = new Queue<(int Index, int Distance)>();
            foreach (int index in source)
            {
                queue.Enqueue((index, 0));
            }

            while (queue.Count > 0)
            {
                (int index, int currentDistance) = queue.Dequeue();
                if (currentDistance >= distance)
                {
                    continue;
                }

                int x = index % width;
                int y = index / width;
                foreach (Vector2Int offset in ConnectedOffsets)
                {
                    int nx = x + offset.x;
                    int ny = y + offset.y;
                    if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                    {
                        continue;
                    }

                    int neighbor = (ny * width) + nx;
                    if (neighbor < 0 || neighbor >= pixelCount || !result.Add(neighbor))
                    {
                        continue;
                    }

                    queue.Enqueue((neighbor, currentDistance + 1));
                }
            }

            return result;
        }
    }
}
