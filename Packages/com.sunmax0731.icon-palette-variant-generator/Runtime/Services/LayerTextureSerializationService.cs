using System;
using Sunmax0731.IconPaletteVariantGenerator.Models;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Services
{
    /// <summary>
    /// Converts raster layer pixels between Color32 arrays and serializable payloads.
    /// </summary>
    public sealed class LayerTextureSerializationService
    {
        public LayerPixelData Serialize(Color32[] pixels, int width, int height)
        {
            if (pixels == null)
            {
                throw new ArgumentNullException(nameof(pixels));
            }

            byte[] bytes = new byte[pixels.Length * 4];
            for (int index = 0; index < pixels.Length; index++)
            {
                int offset = index * 4;
                bytes[offset] = pixels[index].r;
                bytes[offset + 1] = pixels[index].g;
                bytes[offset + 2] = pixels[index].b;
                bytes[offset + 3] = pixels[index].a;
            }

            return new LayerPixelData
            {
                width = width,
                height = height,
                rgbaBytesBase64 = Convert.ToBase64String(bytes)
            };
        }

        public Color32[] Deserialize(LayerPixelData pixelData)
        {
            if (pixelData == null || pixelData.width <= 0 || pixelData.height <= 0)
            {
                return Array.Empty<Color32>();
            }

            if (string.IsNullOrWhiteSpace(pixelData.rgbaBytesBase64))
            {
                return new Color32[pixelData.width * pixelData.height];
            }

            byte[] bytes = Convert.FromBase64String(pixelData.rgbaBytesBase64);
            int pixelCount = pixelData.width * pixelData.height;
            Color32[] pixels = new Color32[pixelCount];
            for (int index = 0; index < pixelCount; index++)
            {
                int offset = index * 4;
                if (offset + 3 >= bytes.Length)
                {
                    break;
                }

                pixels[index] = new Color32(
                    bytes[offset],
                    bytes[offset + 1],
                    bytes[offset + 2],
                    bytes[offset + 3]);
            }

            return pixels;
        }

        public LayerPixelData CreateBlank(int width, int height)
        {
            return Serialize(new Color32[Mathf.Max(0, width * height)], width, height);
        }
    }
}
