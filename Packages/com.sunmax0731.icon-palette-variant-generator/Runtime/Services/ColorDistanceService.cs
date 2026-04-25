using Sunmax0731.IconPaletteVariantGenerator.Models;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Services
{
    /// <summary>
    /// Calculates distance between two colors.
    /// </summary>
    public sealed class ColorDistanceService
    {
        public float Calculate(Color32 a, Color32 b, ColorDistanceMode mode)
        {
            switch (mode)
            {
                case ColorDistanceMode.Hsv:
                    return CalculateHsvDistance(a, b);
                case ColorDistanceMode.Lab:
                    return CalculateLabDistance(a, b);
                case ColorDistanceMode.Rgb:
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

        private static float CalculateLabDistance(Color32 a, Color32 b)
        {
            Vector3 labA = ToLab(a);
            Vector3 labB = ToLab(b);
            return Vector3.Distance(labA, labB);
        }

        private static Vector3 ToLab(Color32 color)
        {
            float r = SrgbToLinear(color.r / 255f);
            float g = SrgbToLinear(color.g / 255f);
            float b = SrgbToLinear(color.b / 255f);

            float x = (r * 0.4124564f) + (g * 0.3575761f) + (b * 0.1804375f);
            float y = (r * 0.2126729f) + (g * 0.7151522f) + (b * 0.0721750f);
            float z = (r * 0.0193339f) + (g * 0.1191920f) + (b * 0.9503041f);

            float fx = LabPivot(x / 0.95047f);
            float fy = LabPivot(y);
            float fz = LabPivot(z / 1.08883f);

            float lightness = (116f * fy) - 16f;
            float greenRed = 500f * (fx - fy);
            float blueYellow = 200f * (fy - fz);
            return new Vector3(lightness, greenRed, blueYellow);
        }

        private static float SrgbToLinear(float value)
        {
            return value <= 0.04045f
                ? value / 12.92f
                : Mathf.Pow((value + 0.055f) / 1.055f, 2.4f);
        }

        private static float LabPivot(float value)
        {
            const float epsilon = 216f / 24389f;
            const float kappa = 24389f / 27f;
            return value > epsilon
                ? Mathf.Pow(value, 1f / 3f)
                : ((kappa * value) + 16f) / 116f;
        }
    }
}
