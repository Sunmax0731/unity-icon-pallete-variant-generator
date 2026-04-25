using System.Collections.Generic;
using System.Linq;
using Sunmax0731.IconPaletteVariantGenerator.Models;
using Sunmax0731.IconPaletteVariantGenerator.Utilities;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Services
{
    /// <summary>
    /// Fills small isolated color regions with nearby surrounding colors before replacement.
    /// </summary>
    public sealed class NoiseRemovalService
    {
        private static readonly Vector2Int[] ConnectedOffsets =
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        private static readonly Vector2Int[] NeighborOffsets =
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, -1)
        };

        private readonly ColorQuantizationService quantizationService;
        private readonly ColorDistanceService distanceService;

        public NoiseRemovalService()
            : this(new ColorQuantizationService(), new ColorDistanceService())
        {
        }

        public NoiseRemovalService(ColorQuantizationService quantizationService, ColorDistanceService distanceService)
        {
            this.quantizationService = quantizationService;
            this.distanceService = distanceService;
        }

        public NoiseRemovalResult Apply(
            Color32[] pixels,
            int width,
            int height,
            PaletteVariantSession session)
        {
            if (pixels == null)
            {
                throw new System.ArgumentNullException(nameof(pixels));
            }

            if (session == null)
            {
                throw new System.ArgumentNullException(nameof(session));
            }

            NoiseRemovalSettings settings = session.noiseRemovalSettings ?? new NoiseRemovalSettings();
            if (!settings.enabled || pixels.Length == 0 || width <= 0 || height <= 0)
            {
                return new NoiseRemovalResult(0, 0);
            }

            int maxRegionPixels = Mathf.Max(1, settings.maxRegionPixels);
            int alphaThreshold = Mathf.Clamp(session.analyzeSettings.alphaThreshold, 0, 255);
            int quantizeStep = Mathf.Clamp(session.analyzeSettings.quantizeStep, 1, 64);
            Dictionary<uint, PaletteColorEntry> entriesByColorKey = BuildEntryLookup(session);
            Dictionary<string, ColorGroup> groupsById = BuildGroupLookup(session);
            PixelInfo[] pixelInfos = BuildPixelInfos(pixels, alphaThreshold, quantizeStep, entriesByColorKey, groupsById);
            bool[] visited = new bool[pixels.Length];
            int[] queue = new int[Mathf.Min(pixels.Length, maxRegionPixels + 1)];
            List<int> region = new List<int>(maxRegionPixels + 1);
            List<int> filledPixelIndices = new List<int>();
            int filledRegionCount = 0;
            int filledPixelCount = 0;

            for (int index = 0; index < pixels.Length; index++)
            {
                if (visited[index] || !pixelInfos[index].IsReplaceable)
                {
                    continue;
                }

                FloodRegion(index, width, height, pixelInfos, visited, queue, region, maxRegionPixels);
                if (region.Count == 0 || region.Count > maxRegionPixels)
                {
                    continue;
                }

                if (!TryFindFillColor(region, width, height, pixelInfos, settings, session.groupSettings.distanceMode, out Color32 fillColor))
                {
                    continue;
                }

                foreach (int regionIndex in region)
                {
                    pixels[regionIndex] = PreserveAlpha(fillColor, pixels[regionIndex]);
                    filledPixelIndices.Add(regionIndex);
                }

                filledRegionCount++;
                filledPixelCount += region.Count;
            }

            return new NoiseRemovalResult(filledRegionCount, filledPixelCount, filledPixelIndices);
        }

        private PixelInfo[] BuildPixelInfos(
            Color32[] pixels,
            int alphaThreshold,
            int quantizeStep,
            Dictionary<uint, PaletteColorEntry> entriesByColorKey,
            Dictionary<string, ColorGroup> groupsById)
        {
            PixelInfo[] pixelInfos = new PixelInfo[pixels.Length];
            for (int index = 0; index < pixels.Length; index++)
            {
                Color32 pixel = pixels[index];
                if (pixel.a <= alphaThreshold)
                {
                    pixelInfos[index] = PixelInfo.Transparent;
                    continue;
                }

                Color32 quantized = quantizationService.Quantize(pixel, quantizeStep);
                uint colorKey = ColorCodeUtility.ToRgbKey(quantized);
                entriesByColorKey.TryGetValue(colorKey, out PaletteColorEntry entry);
                ColorGroup group = null;
                if (entry != null && !string.IsNullOrEmpty(entry.groupId))
                {
                    groupsById.TryGetValue(entry.groupId, out group);
                }

                pixelInfos[index] = new PixelInfo(colorKey, quantized, entry, group);
            }

            return pixelInfos;
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

        private static void FloodRegion(
            int startIndex,
            int width,
            int height,
            PixelInfo[] pixelInfos,
            bool[] visited,
            int[] queue,
            List<int> region,
            int maxRegionPixels)
        {
            region.Clear();
            int queueStart = 0;
            int queueEnd = 0;
            uint colorKey = pixelInfos[startIndex].ColorKey;
            visited[startIndex] = true;
            queue[queueEnd++] = startIndex;

            while (queueStart < queueEnd)
            {
                int index = queue[queueStart++];
                region.Add(index);
                if (region.Count > maxRegionPixels)
                {
                    DrainLargeRegion(index, width, height, pixelInfos, visited, colorKey, queue, ref queueStart, ref queueEnd);
                    return;
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

                    int neighborIndex = (ny * width) + nx;
                    if (visited[neighborIndex] || !pixelInfos[neighborIndex].IsReplaceable || pixelInfos[neighborIndex].ColorKey != colorKey)
                    {
                        continue;
                    }

                    visited[neighborIndex] = true;
                    if (queueEnd >= queue.Length)
                    {
                        System.Array.Resize(ref queue, queue.Length * 2);
                    }

                    queue[queueEnd++] = neighborIndex;
                }
            }
        }

        private static void DrainLargeRegion(
            int currentIndex,
            int width,
            int height,
            PixelInfo[] pixelInfos,
            bool[] visited,
            uint colorKey,
            int[] queue,
            ref int queueStart,
            ref int queueEnd)
        {
            int x = currentIndex % width;
            int y = currentIndex / width;
            foreach (Vector2Int offset in ConnectedOffsets)
            {
                int nx = x + offset.x;
                int ny = y + offset.y;
                if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                {
                    continue;
                }

                int neighborIndex = (ny * width) + nx;
                if (visited[neighborIndex] || !pixelInfos[neighborIndex].IsReplaceable || pixelInfos[neighborIndex].ColorKey != colorKey)
                {
                    continue;
                }

                visited[neighborIndex] = true;
                if (queueEnd >= queue.Length)
                {
                    System.Array.Resize(ref queue, queue.Length * 2);
                }

                queue[queueEnd++] = neighborIndex;
            }

            while (queueStart < queueEnd)
            {
                int index = queue[queueStart++];
                int ix = index % width;
                int iy = index / width;
                foreach (Vector2Int offset in ConnectedOffsets)
                {
                    int nx = ix + offset.x;
                    int ny = iy + offset.y;
                    if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                    {
                        continue;
                    }

                    int neighborIndex = (ny * width) + nx;
                    if (visited[neighborIndex] || !pixelInfos[neighborIndex].IsReplaceable || pixelInfos[neighborIndex].ColorKey != colorKey)
                    {
                        continue;
                    }

                    visited[neighborIndex] = true;
                    if (queueEnd >= queue.Length)
                    {
                        System.Array.Resize(ref queue, queue.Length * 2);
                    }

                    queue[queueEnd++] = neighborIndex;
                }
            }
        }

        private bool TryFindFillColor(
            IReadOnlyList<int> region,
            int width,
            int height,
            PixelInfo[] pixelInfos,
            NoiseRemovalSettings settings,
            ColorDistanceMode distanceMode,
            out Color32 fillColor)
        {
            Dictionary<uint, NeighborCandidate> candidatesByColorKey = new Dictionary<uint, NeighborCandidate>();
            HashSet<int> regionLookup = new HashSet<int>(region);
            PixelInfo regionInfo = pixelInfos[region[0]];
            foreach (int regionIndex in region)
            {
                int x = regionIndex % width;
                int y = regionIndex / width;
                foreach (Vector2Int offset in NeighborOffsets)
                {
                    int nx = x + offset.x;
                    int ny = y + offset.y;
                    if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                    {
                        continue;
                    }

                    int neighborIndex = (ny * width) + nx;
                    if (regionLookup.Contains(neighborIndex))
                    {
                        continue;
                    }

                    PixelInfo neighborInfo = pixelInfos[neighborIndex];
                    if (!neighborInfo.IsReplaceable || neighborInfo.ColorKey == regionInfo.ColorKey)
                    {
                        continue;
                    }

                    if (settings.sameGroupOnly && !SameGroup(regionInfo.Group, neighborInfo.Group))
                    {
                        continue;
                    }

                    if (!candidatesByColorKey.TryGetValue(neighborInfo.ColorKey, out NeighborCandidate candidate))
                    {
                        candidate = new NeighborCandidate(neighborInfo.Color, neighborInfo.Group);
                    }

                    candidate.Count++;
                    candidatesByColorKey[neighborInfo.ColorKey] = candidate;
                }
            }

            if (candidatesByColorKey.Count == 0)
            {
                fillColor = default;
                return false;
            }

            NeighborCandidate selected = candidatesByColorKey.Values
                .OrderByDescending(candidate => candidate.Count)
                .First();
            float distance = distanceService.Calculate(regionInfo.Color, selected.Color, distanceMode);
            if (distance > Mathf.Max(0f, settings.neighborDistanceThreshold))
            {
                fillColor = default;
                return false;
            }

            fillColor = selected.Color;
            return true;
        }

        private static bool SameGroup(ColorGroup a, ColorGroup b)
        {
            return a != null && b != null && a.id == b.id;
        }

        private static Color32 PreserveAlpha(Color32 fillColor, Color32 original)
        {
            fillColor.a = original.a;
            return fillColor;
        }

        private readonly struct PixelInfo
        {
            public static readonly PixelInfo Transparent = new PixelInfo(0, default, null, null);

            public PixelInfo(uint colorKey, Color32 color, PaletteColorEntry entry, ColorGroup group)
            {
                ColorKey = colorKey;
                Color = color;
                Entry = entry;
                Group = group;
            }

            public uint ColorKey { get; }
            public Color32 Color { get; }
            public PaletteColorEntry Entry { get; }
            public ColorGroup Group { get; }
            public bool IsReplaceable => Entry != null && Group != null;
        }

        private struct NeighborCandidate
        {
            public NeighborCandidate(Color32 color, ColorGroup group)
            {
                Color = color;
                Group = group;
                Count = 0;
            }

            public Color32 Color { get; }
            public ColorGroup Group { get; }
            public int Count { get; set; }
        }
    }
}
