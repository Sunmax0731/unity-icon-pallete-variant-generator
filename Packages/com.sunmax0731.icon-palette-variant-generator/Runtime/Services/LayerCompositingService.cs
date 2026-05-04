using System;
using System.Collections.Generic;
using Sunmax0731.IconPaletteVariantGenerator.Models;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Services
{
    /// <summary>
    /// Composites raster layers over a generated preview texture.
    /// </summary>
    public sealed class LayerCompositingService
    {
        private readonly LayerTextureSerializationService serializationService;

        public LayerCompositingService()
            : this(new LayerTextureSerializationService())
        {
        }

        public LayerCompositingService(LayerTextureSerializationService serializationService)
        {
            this.serializationService = serializationService;
        }

        public Texture2D Compose(Texture2D baseTexture, IReadOnlyList<RasterLayer> layers)
        {
            if (baseTexture == null)
            {
                throw new ArgumentNullException(nameof(baseTexture));
            }

            Texture2D output = new Texture2D(baseTexture.width, baseTexture.height, TextureFormat.RGBA32, false)
            {
                name = $"{baseTexture.name}_Composite",
                filterMode = baseTexture.filterMode,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            UpdateCompositeTexture(output, baseTexture, layers);
            return output;
        }

        public void UpdateCompositeTexture(Texture2D targetTexture, Texture2D baseTexture, IReadOnlyList<RasterLayer> layers)
        {
            UpdateCompositeTexture(targetTexture, baseTexture, layers, null, null, 0, 0);
        }

        public void UpdateCompositeTexture(
            Texture2D targetTexture,
            Texture2D baseTexture,
            IReadOnlyList<RasterLayer> layers,
            string overrideLayerId,
            Color32[] overridePixels,
            int overrideWidth,
            int overrideHeight)
        {
            if (targetTexture == null)
            {
                throw new ArgumentNullException(nameof(targetTexture));
            }

            if (baseTexture == null)
            {
                throw new ArgumentNullException(nameof(baseTexture));
            }

            if (targetTexture.width != baseTexture.width || targetTexture.height != baseTexture.height)
            {
                throw new ArgumentException("Target texture size must match the base texture.", nameof(targetTexture));
            }

            Color32[] basePixels = baseTexture.GetPixels32();
            Color32[] composedPixels = new Color32[basePixels.Length];
            Array.Copy(basePixels, composedPixels, basePixels.Length);

            if (layers != null)
            {
                foreach (RasterLayer layer in layers)
                {
                    if (layer == null || !layer.visible || layer.opacity <= 0f)
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(overrideLayerId)
                        && layer.id == overrideLayerId
                        && overridePixels != null
                        && overridePixels.Length > 0
                        && overrideWidth > 0
                        && overrideHeight > 0)
                    {
                        BlendPixels(composedPixels, baseTexture.width, baseTexture.height, overridePixels, overrideWidth, overrideHeight, layer.offsetX, layer.offsetY, layer.opacity);
                        continue;
                    }

                    BlendLayer(composedPixels, baseTexture.width, baseTexture.height, layer);
                }
            }

            targetTexture.SetPixels32(composedPixels);
            targetTexture.Apply(false, false);
        }

        public RasterLayer CreatePaintLayer(string id, string displayName, int width, int height)
        {
            return new RasterLayer
            {
                id = id,
                displayName = displayName,
                kind = LayerKind.Paint,
                pixelData = serializationService.CreateBlank(width, height)
            };
        }

        public RasterLayer CreateImageLayer(string id, string displayName, Texture2D texture, string sourceAssetPath, int width, int height)
        {
            if (texture == null)
            {
                throw new ArgumentNullException(nameof(texture));
            }

            Color32[] pixels = ResizePixels(texture.GetPixels32(), texture.width, texture.height, width, height);
            return new RasterLayer
            {
                id = id,
                displayName = displayName,
                kind = LayerKind.Image,
                sourceAssetPath = sourceAssetPath ?? string.Empty,
                pixelData = serializationService.Serialize(pixels, width, height)
            };
        }

        private void BlendLayer(Color32[] destinationPixels, int destinationWidth, int destinationHeight, RasterLayer layer)
        {
            Color32[] sourcePixels = serializationService.Deserialize(layer.pixelData);
            if (sourcePixels.Length == 0)
            {
                return;
            }

            BlendPixels(destinationPixels, destinationWidth, destinationHeight, sourcePixels, layer.pixelData.width, layer.pixelData.height, layer.offsetX, layer.offsetY, layer.opacity);
        }

        private static void BlendPixels(Color32[] destinationPixels, int destinationWidth, int destinationHeight, Color32[] sourcePixels, int sourceWidth, int sourceHeight, int offsetX, int offsetY, float opacityValue)
        {
            float opacity = Mathf.Clamp01(opacityValue);

            for (int y = 0; y < sourceHeight; y++)
            {
                int destinationY = y + offsetY;
                if (destinationY < 0 || destinationY >= destinationHeight)
                {
                    continue;
                }

                for (int x = 0; x < sourceWidth; x++)
                {
                    int destinationX = x + offsetX;
                    if (destinationX < 0 || destinationX >= destinationWidth)
                    {
                        continue;
                    }

                    int sourceIndex = (y * sourceWidth) + x;
                    int destinationIndex = (destinationY * destinationWidth) + destinationX;
                    destinationPixels[destinationIndex] = BlendNormal(destinationPixels[destinationIndex], sourcePixels[sourceIndex], opacity);
                }
            }
        }

        private static Color32[] ResizePixels(Color32[] sourcePixels, int sourceWidth, int sourceHeight, int targetWidth, int targetHeight)
        {
            Color32[] pixels = new Color32[Mathf.Max(0, targetWidth * targetHeight)];
            if (sourcePixels == null || sourcePixels.Length == 0 || sourceWidth <= 0 || sourceHeight <= 0)
            {
                return pixels;
            }

            for (int y = 0; y < targetHeight; y++)
            {
                float v = targetHeight <= 1 ? 0f : y / (float)(targetHeight - 1);
                int sourceY = Mathf.Clamp(Mathf.RoundToInt(v * (sourceHeight - 1)), 0, sourceHeight - 1);
                for (int x = 0; x < targetWidth; x++)
                {
                    float u = targetWidth <= 1 ? 0f : x / (float)(targetWidth - 1);
                    int sourceX = Mathf.Clamp(Mathf.RoundToInt(u * (sourceWidth - 1)), 0, sourceWidth - 1);
                    pixels[(y * targetWidth) + x] = sourcePixels[(sourceY * sourceWidth) + sourceX];
                }
            }

            return pixels;
        }

        private static Color32 BlendNormal(Color32 destination, Color32 source, float opacity)
        {
            float sourceAlpha = (source.a / 255f) * opacity;
            if (sourceAlpha <= 0f)
            {
                return destination;
            }

            float destinationAlpha = destination.a / 255f;
            float outAlpha = sourceAlpha + (destinationAlpha * (1f - sourceAlpha));
            if (outAlpha <= 0f)
            {
                return default;
            }

            float outR = ((source.r / 255f) * sourceAlpha) + ((destination.r / 255f) * destinationAlpha * (1f - sourceAlpha));
            float outG = ((source.g / 255f) * sourceAlpha) + ((destination.g / 255f) * destinationAlpha * (1f - sourceAlpha));
            float outB = ((source.b / 255f) * sourceAlpha) + ((destination.b / 255f) * destinationAlpha * (1f - sourceAlpha));

            return new Color32(
                (byte)Mathf.Clamp(Mathf.RoundToInt(outR / outAlpha * 255f), 0, 255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(outG / outAlpha * 255f), 0, 255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(outB / outAlpha * 255f), 0, 255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(outAlpha * 255f), 0, 255));
        }
    }
}
