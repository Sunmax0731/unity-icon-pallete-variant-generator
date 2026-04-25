using System.Collections.Generic;
using System.Linq;
using Sunmax0731.IconPaletteVariantGenerator.Models;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Services
{
    /// <summary>
    /// Groups extracted palette colors by nearest representative color.
    /// </summary>
    public sealed class ColorGroupingService
    {
        private const int IterationCount = 8;
        private readonly ColorDistanceService distanceService;

        public ColorGroupingService()
            : this(new ColorDistanceService())
        {
        }

        public ColorGroupingService(ColorDistanceService distanceService)
        {
            this.distanceService = distanceService;
        }

        public IReadOnlyList<ColorGroup> CreateGroups(IReadOnlyList<PaletteColorEntry> colors, GroupSettings settings)
        {
            if (colors == null)
            {
                throw new System.ArgumentNullException(nameof(colors));
            }

            if (settings == null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            List<PaletteColorEntry> activeColors = colors
                .Where(color => color != null && !color.ignored && color.pixelCount > 0)
                .OrderByDescending(color => color.pixelCount)
                .ThenBy(color => color.hex)
                .ToList();

            if (activeColors.Count == 0)
            {
                return new List<ColorGroup>();
            }

            int groupCount = Mathf.Clamp(settings.targetGroupCount, 1, activeColors.Count);
            List<Color32> centers = activeColors.Take(groupCount).Select(color => color.color).ToList();
            Dictionary<int, List<PaletteColorEntry>> assignments = new Dictionary<int, List<PaletteColorEntry>>();

            for (int iteration = 0; iteration < IterationCount; iteration++)
            {
                assignments = AssignColors(activeColors, centers, settings.distanceMode);
                centers = RecalculateCenters(assignments, centers);
            }

            assignments = AssignColors(activeColors, centers, settings.distanceMode);
            int totalPixels = activeColors.Sum(color => color.pixelCount);
            List<ColorGroup> groups = new List<ColorGroup>();

            float maxColorDistance = Mathf.Clamp(settings.maxColorDistance, 0f, 441.7f);
            for (int index = 0; index < centers.Count; index++)
            {
                if (!assignments.TryGetValue(index, out List<PaletteColorEntry> groupColors) || groupColors.Count == 0)
                {
                    continue;
                }

                Color32 representative = CalculateWeightedAverage(groupColors);
                List<PaletteColorEntry> nearbyColors = new List<PaletteColorEntry>();
                List<PaletteColorEntry> outlierColors = new List<PaletteColorEntry>();

                foreach (PaletteColorEntry color in groupColors)
                {
                    float distance = distanceService.Calculate(color.color, representative, settings.distanceMode);
                    if (distance <= maxColorDistance)
                    {
                        nearbyColors.Add(color);
                    }
                    else
                    {
                        outlierColors.Add(color);
                    }
                }

                if (nearbyColors.Count > 0)
                {
                    AddGroup(groups, nearbyColors, totalPixels);
                }

                foreach (PaletteColorEntry outlierColor in outlierColors)
                {
                    AddGroup(groups, new[] { outlierColor }, totalPixels);
                }
            }

            return groups
                .OrderByDescending(group => group.pixelCount)
                .ThenBy(group => group.displayName)
                .ToList();
        }

        private static void AddGroup(
            List<ColorGroup> groups,
            IReadOnlyList<PaletteColorEntry> groupColors,
            int totalPixels)
        {
            string groupId = $"group_{groups.Count + 1:00}";
            int pixelCount = groupColors.Sum(color => color.pixelCount);
            Color32 representative = CalculateWeightedAverage(groupColors);

            foreach (PaletteColorEntry color in groupColors)
            {
                color.groupId = groupId;
            }

            groups.Add(new ColorGroup
            {
                id = groupId,
                displayName = $"Group {groups.Count + 1}",
                representativeColor = representative,
                targetColor = representative,
                colorEntryIds = groupColors.Select(color => color.id).ToList(),
                pixelCount = pixelCount,
                pixelRatio = totalPixels == 0 ? 0f : (float)pixelCount / totalPixels
            });
        }

        private Dictionary<int, List<PaletteColorEntry>> AssignColors(
            IReadOnlyList<PaletteColorEntry> colors,
            IReadOnlyList<Color32> centers,
            ColorDistanceMode distanceMode)
        {
            Dictionary<int, List<PaletteColorEntry>> assignments = new Dictionary<int, List<PaletteColorEntry>>();

            foreach (PaletteColorEntry color in colors)
            {
                int nearestIndex = 0;
                float nearestDistance = float.MaxValue;

                for (int index = 0; index < centers.Count; index++)
                {
                    float distance = distanceService.Calculate(color.color, centers[index], distanceMode);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestIndex = index;
                    }
                }

                if (!assignments.TryGetValue(nearestIndex, out List<PaletteColorEntry> groupColors))
                {
                    groupColors = new List<PaletteColorEntry>();
                    assignments.Add(nearestIndex, groupColors);
                }

                groupColors.Add(color);
            }

            return assignments;
        }

        private static List<Color32> RecalculateCenters(
            IReadOnlyDictionary<int, List<PaletteColorEntry>> assignments,
            IReadOnlyList<Color32> previousCenters)
        {
            List<Color32> centers = new List<Color32>();

            for (int index = 0; index < previousCenters.Count; index++)
            {
                if (assignments.TryGetValue(index, out List<PaletteColorEntry> colors) && colors.Count > 0)
                {
                    centers.Add(CalculateWeightedAverage(colors));
                }
                else
                {
                    centers.Add(previousCenters[index]);
                }
            }

            return centers;
        }

        private static Color32 CalculateWeightedAverage(IReadOnlyList<PaletteColorEntry> colors)
        {
            int totalPixels = colors.Sum(color => color.pixelCount);
            if (totalPixels <= 0)
            {
                return colors[0].color;
            }

            float r = 0f;
            float g = 0f;
            float b = 0f;

            foreach (PaletteColorEntry color in colors)
            {
                r += color.color.r * color.pixelCount;
                g += color.color.g * color.pixelCount;
                b += color.color.b * color.pixelCount;
            }

            return new Color32(
                (byte)Mathf.Clamp(Mathf.RoundToInt(r / totalPixels), 0, 255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(g / totalPixels), 0, 255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(b / totalPixels), 0, 255),
                255);
        }
    }
}
