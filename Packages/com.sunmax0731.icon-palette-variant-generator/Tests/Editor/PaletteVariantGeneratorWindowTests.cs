using NUnit.Framework;
using Sunmax0731.IconPaletteVariantGenerator.Editor.Windows;
using UnityEditor;

namespace Sunmax0731.IconPaletteVariantGenerator.Editor.Tests
{
    public sealed class PaletteVariantGeneratorWindowTests
    {
        [Test]
        public void OpenCreatesWindowWithExpectedTitle()
        {
            PaletteVariantGeneratorWindow.Open();

            PaletteVariantGeneratorWindow window = EditorWindow.GetWindow<PaletteVariantGeneratorWindow>();

            Assert.That(window, Is.Not.Null);
            Assert.That(window.titleContent.text, Is.EqualTo("Palette Variant Generator"));

            window.Close();
        }
    }
}
