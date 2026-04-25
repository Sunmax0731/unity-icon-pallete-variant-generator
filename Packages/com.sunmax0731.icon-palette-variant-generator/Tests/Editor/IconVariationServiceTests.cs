using System.Collections.Generic;
using NUnit.Framework;
using Sunmax0731.IconPaletteVariantGenerator.Models;
using Sunmax0731.IconPaletteVariantGenerator.Services;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Editor.Tests
{
    public sealed class IconVariationServiceTests
    {
        [Test]
        public void DuplicateActiveVariationCreatesIndependentRules()
        {
            PaletteVariantSession session = CreateSession();
            IconVariationService service = new IconVariationService();
            service.EnsureActiveVariation(session);
            service.SyncActiveVariation(session);

            IconVariation duplicate = service.DuplicateActiveVariation(session);
            session.colorGroups[0].targetColor = new Color32(0, 255, 0, 255);
            service.SyncActiveVariation(session);

            IconVariation original = session.variations[0];

            Assert.That(session.variations, Has.Count.EqualTo(2));
            Assert.That(duplicate.id, Is.EqualTo(session.activeVariationId));
            Assert.That(original.colorGroups[0].targetColor.b, Is.EqualTo(255));
            Assert.That(duplicate.colorGroups[0].targetColor.g, Is.EqualTo(255));
        }

        [Test]
        public void ApplyVariationRestoresVariationSnapshot()
        {
            PaletteVariantSession session = CreateSession();
            IconVariationService service = new IconVariationService();
            IconVariation first = service.EnsureActiveVariation(session);
            service.SyncActiveVariation(session);
            IconVariation second = service.AddVariation(session);
            session.colorGroups[0].targetColor = new Color32(0, 255, 0, 255);
            session.exportSettings.fileSuffix = "green";
            second.fileSuffix = "green";
            service.SyncActiveVariation(session);

            service.ApplyVariation(session, first.id);

            Assert.That(session.colorGroups[0].targetColor.b, Is.EqualTo(255));
            Assert.That(session.exportSettings.fileSuffix, Is.EqualTo(first.fileSuffix));
        }

        private static PaletteVariantSession CreateSession()
        {
            return new PaletteVariantSession
            {
                exportSettings = new ExportSettings { fileSuffix = "blue" },
                colorGroups = new List<ColorGroup>
                {
                    new ColorGroup
                    {
                        id = "group_01",
                        displayName = "Group 1",
                        representativeColor = new Color32(255, 0, 0, 255),
                        targetColor = new Color32(0, 0, 255, 255),
                        blendRatio = 1f,
                        colorEntryIds = new List<string> { "#FF0000" }
                    }
                },
                colorRules = new List<ColorReplacementRule>
                {
                    new ColorReplacementRule
                    {
                        id = "rule_01",
                        groupId = "group_01",
                        colorEntryId = "#FF0000",
                        scope = ColorReplacementScope.ColorEntry,
                        targetColor = new Color32(0, 0, 255, 255),
                        blendRatio = 1f,
                        enabled = true
                    }
                }
            };
        }
    }
}
