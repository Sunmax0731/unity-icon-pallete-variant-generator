using Sunmax0731.IconPaletteVariantGenerator.Models;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Services
{
    /// <summary>
    /// Calculates distance between two RGB colors.
    /// </summary>
    public sealed class ColorDistanceService
    {
        public float Calculate(Color32 a, Color32 b, ColorDistanceMode mode)
        {
            switch (mode)
            {
                case ColorDistanceMode.Hsv:
                    return CalculateHsvDistance(a, b);
                case ColorDistanceMode.Rgb:
                case ColorDistanceMode.Lab:
                default:
                    return CalculateRgbDistance(a, b);
            }
        }

        private static float CalculateRgbDistance(Color32 a, Color32 b)
        {
            float r = a.r - b.r;
            float g = a.g - b.g;
            float blue = a.b - b.b;
            return Mathf.Sqrt((r * r) + (g * g) + (blue * blue));
        }

        private static float CalculateHsvDistance(Color32 a, Color32 b)
        {
            Color.RGBToHSV(a, out float hueA, out float saturationA, out float valueA);
            Color.RGBToHSV(b, out float hueB, out float saturationB, out float valueB);

            float hueDistance = Mathf.Abs(hueA - hueB);
            hueDistance = Mathf.Min(hueDistance, 1f - hueDistance);
            float saturationDistance = saturationA - saturationB;
            float valueDistance = valueA - valueB;

            return Mathf.Sqrt(
                (hueDistance * hueDistance * 4f) +
                (saturationDistance * saturationDistance) +
                (valueDistance * valueDistance));
        }
    }
}
