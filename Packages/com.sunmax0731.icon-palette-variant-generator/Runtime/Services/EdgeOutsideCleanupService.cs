using System.Collections.Generic;
using System.Linq;
using Sunmax0731.IconPaletteVariantGenerator.Models;
using Sunmax0731.IconPaletteVariantGenerator.Utilities;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Services
{
    /// <summary>
    /// Clears small foreground islands near the outside edge of the main opaque component.
    /// </summary>
    public sealed class EdgeOutsideCleanupService
    {
        private const float OpaqueImageTransparentRatioThreshold = 0.01f;
        private const float MinimumOutsideGroupPixelRatio = 0.03f;
        private const float MinimumOutsideGroupBorderRatio = 0.02f;

        private static readonly Vector2Int[] ConnectedOffsets =
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        private readonly ColorQuantizationService quantizationService;

        public EdgeOutsideCleanupService()
            : this(new ColorQuantizationService())
        {
        }

        public EdgeOutsideCleanupService(ColorQuantizationService quantizationService)
        {
            this.quantizationService = quantizationService;
        }

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
            bool[] foreground = BuildForegroundMask(pixels, width, height, session, alphaThreshold);
            if (settings.mode == EdgeOutsideCleanupMode.BoundaryTrim)
            {
                return ApplyBoundaryTrim(pixels, width, height, foreground, Mathf.Max(1, settings.trimDistancePixels));
            }

            List<List<int>> regions = FindForegroundRegions(foreground, width, height);
            if (regions.Count <= 1)
            {
                return new EdgeOutsideCleanupResult(0, 0);
            }

            HashSet<int> mainRegion = new HashSet<int>(regions.OrderByDescending(region => region.Count).First());
            HashSet<int> nearMain = ExpandRegion(mainRegion, foreground.Length, width, height, maxDistancePixels);
            List<int> clearedPixelIndices = new List<int>();
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
                    clearedPixelIndices.Add(index);
                }

                clearedRegions++;
                clearedPixels += region.Count;
            }

            return new EdgeOutsideCleanupResult(clearedRegions, clearedPixels, clearedPixelIndices);
        }

        private static EdgeOutsideCleanupResult ApplyBoundaryTrim(Color32[] pixels, int width, int height, bool[] foreground, int trimDistancePixels)
        {
            HashSet<int> trimCandidates = new HashSet<int>();
            for (int index = 0; index < foreground.Length; index++)
            {
                if (!foreground[index])
                {
                    continue;
                }

                int x = index % width;
                int y = index / width;
                if (DistanceToOutside(x, y, width, height, foreground, trimDistancePixels) <= trimDistancePixels)
                {
                    trimCandidates.Add(index);
                }
            }

            foreach (int index in trimCandidates)
            {
                Color32 pixel = pixels[index];
                pixel.a = 0;
                pixels[index] = pixel;
            }

            return new EdgeOutsideCleanupResult(trimCandidates.Count > 0 ? 1 : 0, trimCandidates.Count, trimCandidates.ToList());
        }

        private static int DistanceToOutside(int x, int y, int width, int height, bool[] foreground, int maxDistance)
        {
            for (int distance = 1; distance <= maxDistance; distance++)
            {
                foreach (Vector2Int offset in ConnectedOffsets)
                {
                    int nx = x + (offset.x * distance);
                    int ny = y + (offset.y * distance);
                    if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                    {
                        return distance;
                    }

                    int neighborIndex = (ny * width) + nx;
                    if (!foreground[neighborIndex])
                    {
                        return distance;
                    }
                }
            }

            return maxDistance + 1;
        }

        private bool[] BuildForegroundMask(Color32[] pixels, int width, int height, PaletteVariantSession session, int alphaThreshold)
        {
            int transparentPixels = pixels.Count(pixel => pixel.a <= alphaThreshold);
            float transparentRatio = pixels.Length == 0 ? 0f : (float)transparentPixels / pixels.Length;
            if (transparentRatio >= OpaqueImageTransparentRatioThreshold)
            {
                return pixels.Select(pixel => pixel.a > alphaThreshold).ToArray();
            }

            bool[] inferredMask = BuildOpaqueImageForegroundMask(pixels, width, height, session, alphaThreshold);
            return inferredMask ?? pixels.Select(pixel => pixel.a > alphaThreshold).ToArray();
        }

        private bool[] BuildOpaqueImageForegroundMask(Color32[] pixels, int width, int height, PaletteVariantSession session, int alphaThreshold)
        {
            Dictionary<uint, PaletteColorEntry> entriesByColorKey = BuildEntryLookup(session);
            Dictionary<string, ColorGroup> groupsById = BuildGroupLookup(session);
            if (entriesByColorKey.Count == 0 || groupsById.Count == 0)
            {
                return null;
            }

            string[] groupIdsByPixel = new string[pixels.Length];
            Dictionary<string, int> borderCountsByGroupId = new Dictionary<string, int>();
            int borderPixelCount = 0;
            int quantizeStep = Mathf.Clamp(session.analyzeSettings.quantizeStep, 1, 64);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = (y * width) + x;
                    if (pixels[index].a <= alphaThreshold)
                    {
                        continue;
                    }

                    Color32 quantized = quantizationService.Quantize(pixels[index], quantizeStep);
                    if (!entriesByColorKey.TryGetValue(ColorCodeUtility.ToRgbKey(quantized), out PaletteColorEntry entry)
                        || entry == null
                        || string.IsNullOrEmpty(entry.groupId)
                        || !groupsById.ContainsKey(entry.groupId))
                    {
                        continue;
                    }

                    groupIdsByPixel[index] = entry.groupId;
                    if (!IsBorderPixel(x, y, width, height))
                    {
                        continue;
                    }

                    borderPixelCount++;
                    borderCountsByGroupId.TryGetValue(entry.groupId, out int count);
                    borderCountsByGroupId[entry.groupId] = count + 1;
                }
            }

            if (borderCountsByGroupId.Count == 0 || borderPixelCount == 0)
            {
                return null;
            }

            int minimumBorderPixels = Mathf.Max(4, Mathf.CeilToInt(borderPixelCount * MinimumOutsideGroupBorderRatio));
            HashSet<string> outsideGroupIds = new HashSet<string>(borderCountsByGroupId
                .Where(pair => pair.Value >= minimumBorderPixels
                    && groupsById[pair.Key].pixelRatio >= MinimumOutsideGroupPixelRatio)
                .Select(pair => pair.Key));

            if (outsideGroupIds.Count == 0)
            {
                outsideGroupIds.Add(borderCountsByGroupId.OrderByDescending(pair => pair.Value).First().Key);
            }

            bool[] foreground = new bool[pixels.Length];
            int foregroundCount = 0;
            for (int index = 0; index < pixels.Length; index++)
            {
                string groupId = groupIdsByPixel[index];
                bool isForeground = !string.IsNullOrEmpty(groupId) && !outsideGroupIds.Contains(groupId);
                foreground[index] = isForeground;
                if (isForeground)
                {
                    foregroundCount++;
                }
            }

            return foregroundCount == 0 ? null : foreground;
        }

        private static Dictionary<uint, PaletteColorEntry> BuildEntryLookup(PaletteVariantSession session)
        {
            Dictionary<uint, PaletteColorEntry> entriesByColorKey = new Dictionary<uint, PaletteColorEntry>();
            foreach (PaletteColorEntry entry in session.paletteColors ?? new List<PaletteColorEntry>())
            {
                if (entry == null || entry.ignored)
                {
                    continue;
                }

                entriesByColorKey[ColorCodeUtility.ToRgbKey(entry.color)] = entry;
            }

            return entriesByColorKey;
        }

        private static Dictionary<string, ColorGroup> BuildGroupLookup(PaletteVariantSession session)
        {
            return (session.colorGroups ?? new List<ColorGroup>())
                .Where(group => group != null && !string.IsNullOrEmpty(group.id))
                .ToDictionary(group => group.id, group => group);
        }

        private static bool IsBorderPixel(int x, int y, int width, int height)
        {
            return x == 0 || y == 0 || x == width - 1 || y == height - 1;
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
