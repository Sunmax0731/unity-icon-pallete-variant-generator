using System.Linq;
using NUnit.Framework;
using Sunmax0731.IconPaletteVariantGenerator.Models;
using Sunmax0731.IconPaletteVariantGenerator.Services;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Editor.Tests
{
    public sealed class ColorGroupingServiceTests
    {
        [Test]
        public void CreateGroupsBuildsRequestedNearbyColorGroups()
        {
            PaletteColorEntry[] colors =
            {
                NewEntry("#FF0000", new Color32(255, 0, 0, 255), 4),
                NewEntry("#F00000", new Color32(240, 0, 0, 255), 2),
                NewEntry("#0000FF", new Color32(0, 0, 255, 255), 3),
                NewEntry("#0000F0", new Color32(0, 0, 240, 255), 1)
            };

            GroupSettings settings = new GroupSettings
            {
                targetGroupCount = 2,
                distanceMode = ColorDistanceMode.Rgb
            };

            var groups = new ColorGroupingService().CreateGroups(colors, settings).ToList();

            Assert.That(groups, Has.Count.EqualTo(2));
            Assert.That(groups.Sum(group => group.colorEntryIds.Count), Is.EqualTo(4));
            Assert.That(groups.Sum(group => group.pixelCount), Is.EqualTo(10));
            Assert.That(colors.All(color => !string.IsNullOrEmpty(color.groupId)), Is.True);
        }

        [Test]
        public void ColorDistanceReturnsZeroForSameColor()
        {
            Color32 color = new Color32(12, 34, 56, 255);

            float distance = new ColorDistanceService().Calculate(color, color, ColorDistanceMode.Rgb);

            Assert.That(distance, Is.EqualTo(0f));
        }

        [Test]
        public void CreateGroupsSplitsOutliersWhenMaxColorDistanceIsStrict()
        {
            PaletteColorEntry[] colors =
            {
                NewEntry("#FF0000", new Color32(255, 0, 0, 255), 1),
                NewEntry("#0000FF", new Color32(0, 0, 255, 255), 1)
            };

            GroupSettings broadSettings = new GroupSettings
            {
                targetGroupCount = 1,
                distanceMode = ColorDistanceMode.Rgb,
                maxColorDistance = 441f
            };
            GroupSettings strictSettings = new GroupSettings
            {
                targetGroupCount = 1,
                distanceMode = ColorDistanceMode.Rgb,
                maxColorDistance = 10f
            };

            var broadGroups = new ColorGroupingService().CreateGroups(colors, broadSettings).ToList();
            var strictGroups = new ColorGroupingService().CreateGroups(colors, strictSettings).ToList();

            Assert.That(broadGroups, Has.Count.EqualTo(1));
            Assert.That(strictGroups, Has.Count.EqualTo(2));
        }

        private static PaletteColorEntry NewEntry(string hex, Color32 color, int pixelCount)
        {
            return new PaletteColorEntry
            {
                id = hex,
                hex = hex,
                color = color,
                pixelCount = pixelCount
            };
        }
    }
}
