using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Utilities
{
    /// <summary>
    /// Converts Color32 values to stable display and dictionary keys.
    /// </summary>
    public static class ColorCodeUtility
    {
        public static string ToHex(Color32 color)
        {
            return $"#{color.r:X2}{color.g:X2}{color.b:X2}";
        }

        public static uint ToRgbKey(Color32 color)
        {
            return ((uint)color.r << 16) | ((uint)color.g << 8) | color.b;
        }
    }
}
