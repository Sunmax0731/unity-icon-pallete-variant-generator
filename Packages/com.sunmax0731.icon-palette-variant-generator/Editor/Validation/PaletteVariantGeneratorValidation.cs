using Sunmax0731.IconPaletteVariantGenerator.Editor.Windows;
using UnityEditor;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Editor.Validation
{
    /// <summary>
    /// Headless validation entry points for package scaffold checks.
    /// </summary>
    public static class PaletteVariantGeneratorValidation
    {
        public static void RunScaffoldValidation()
        {
            PaletteVariantGeneratorWindow.Open();
            PaletteVariantGeneratorWindow window = EditorWindow.GetWindow<PaletteVariantGeneratorWindow>();

            if (window == null)
            {
                throw new System.InvalidOperationException("Palette Variant Generator window could not be created.");
            }

            if (window.titleContent == null || window.titleContent.text != "Palette Variant Generator")
            {
                throw new System.InvalidOperationException("Palette Variant Generator window title is invalid.");
            }

            window.Close();
            Debug.Log("ISSUE1_SCAFFOLD_VALIDATION=PASS");
        }
    }
}
