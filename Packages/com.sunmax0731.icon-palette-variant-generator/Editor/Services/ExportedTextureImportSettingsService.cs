using System.IO;
using UnityEditor;

namespace Sunmax0731.IconPaletteVariantGenerator.Editor.Services
{
    /// <summary>
    /// Applies Unity import settings to PNG files exported into the current project.
    /// </summary>
    public sealed class ExportedTextureImportSettingsService
    {
        /// <summary>
        /// Imports an exported PNG as a Unity asset and enables Alpha Is Transparency when the file is under Assets.
        /// </summary>
        /// <param name="outputPath">Absolute or project-relative path returned by the export service.</param>
        /// <param name="projectRoot">Absolute Unity project root path.</param>
        /// <param name="message">Result details for diagnostics.</param>
        /// <returns>True when a Unity TextureImporter was found and configured.</returns>
        public bool ApplyAlphaIsTransparency(string outputPath, string projectRoot, out string message)
        {
            message = string.Empty;
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                message = "Export output path is empty.";
                return false;
            }

            if (!TryGetAssetPath(outputPath, projectRoot, out string assetPath))
            {
                message = $"Exported file is outside the Unity Assets folder: {outputPath}";
                return false;
            }

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                message = $"TextureImporter was not found for exported PNG: {assetPath}";
                return false;
            }

            bool changed = false;
            if (importer.alphaSource == TextureImporterAlphaSource.None)
            {
                importer.alphaSource = TextureImporterAlphaSource.FromInput;
                changed = true;
            }

            if (!importer.alphaIsTransparency)
            {
                importer.alphaIsTransparency = true;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }

            message = $"Alpha Is Transparency enabled: {assetPath}";
            return true;
        }

        internal static bool TryGetAssetPath(string outputPath, string projectRoot, out string assetPath)
        {
            assetPath = string.Empty;
            if (string.IsNullOrWhiteSpace(outputPath) || string.IsNullOrWhiteSpace(projectRoot))
            {
                return false;
            }

            string absoluteOutput = Path.GetFullPath(outputPath).Replace('\\', '/');
            string assetsRoot = Path.GetFullPath(Path.Combine(projectRoot, "Assets")).Replace('\\', '/');
            string assetsRootPrefix = assetsRoot.EndsWith("/")
                ? assetsRoot
                : assetsRoot + "/";

            if (!absoluteOutput.StartsWith(assetsRootPrefix, System.StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string relativePath = absoluteOutput.Substring(assetsRootPrefix.Length);
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return false;
            }

            assetPath = "Assets/" + relativePath;
            return true;
        }
    }
}
