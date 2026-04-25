using System.IO;
using UnityEditor;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Editor.Services
{
    /// <summary>
    /// Loads project Texture2D assets into readable analysis textures.
    /// </summary>
    public sealed class TextureAssetLoader
    {
        public bool TryLoadReadableTexture(Texture2D source, out Texture2D readableTexture, out string assetPath, out string error)
        {
            readableTexture = null;
            assetPath = string.Empty;
            error = string.Empty;

            if (source == null)
            {
                error = "Source image is not selected.";
                return false;
            }

            assetPath = AssetDatabase.GetAssetPath(source);
            if (string.IsNullOrEmpty(assetPath))
            {
                error = "Source image must be a Unity project asset.";
                return false;
            }

            string absolutePath = Path.GetFullPath(assetPath);
            if (!File.Exists(absolutePath))
            {
                error = $"Source image file was not found: {assetPath}";
                return false;
            }

            try
            {
                byte[] bytes = File.ReadAllBytes(absolutePath);
                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
                {
                    name = $"{source.name}_Readable",
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp
                };

                if (!ImageConversion.LoadImage(texture, bytes, false))
                {
                    Object.DestroyImmediate(texture);
                    error = $"Source image could not be decoded: {assetPath}";
                    return false;
                }

                readableTexture = texture;
                return true;
            }
            catch (IOException ex)
            {
                error = ex.Message;
                return false;
            }
            catch (System.UnauthorizedAccessException ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}
