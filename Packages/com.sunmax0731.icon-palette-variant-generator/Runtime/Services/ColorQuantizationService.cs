using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Services
{
    /// <summary>
    /// Rounds RGB channels to reduce near-identical anti-aliased colors.
    /// </summary>
    public sealed class ColorQuantizationService
    {
        public Color32 Quantize(Color32 color, int step)
        {
            int safeStep = Mathf.Clamp(step, 1, 64);
            return new Color32(
                QuantizeChannel(color.r, safeStep),
                QuantizeChannel(color.g, safeStep),
                QuantizeChannel(color.b, safeStep),
                color.a);
        }

        private static byte QuantizeChannel(byte value, int step)
        {
            int rounded = Mathf.RoundToInt((float)value / step) * step;
            return (byte)Mathf.Clamp(rounded, 0, 255);
        }
    }
}
