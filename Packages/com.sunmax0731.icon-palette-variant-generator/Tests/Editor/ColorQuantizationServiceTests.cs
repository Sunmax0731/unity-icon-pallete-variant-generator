using NUnit.Framework;
using Sunmax0731.IconPaletteVariantGenerator.Services;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Editor.Tests
{
    public sealed class ColorQuantizationServiceTests
    {
        [Test]
        public void QuantizeRoundsRgbAndPreservesAlpha()
        {
            Color32 result = new ColorQuantizationService().Quantize(new Color32(253, 2, 127, 77), 8);

            Assert.That(result.r, Is.EqualTo(255));
            Assert.That(result.g, Is.EqualTo(0));
            Assert.That(result.b, Is.EqualTo(128));
            Assert.That(result.a, Is.EqualTo(77));
        }
    }
}
