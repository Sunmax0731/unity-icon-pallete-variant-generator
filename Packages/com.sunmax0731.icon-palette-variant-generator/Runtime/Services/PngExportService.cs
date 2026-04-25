using System.IO;
using Sunmax0731.IconPaletteVariantGenerator.Models;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Services
{
    /// <summary>
    /// Writes generated icon preview textures as PNG files.
    /// </summary>
    public sealed class PngExportService
    {
        public PngExportResult Export(Texture2D texture, ExportSettings settings, string projectRoot)
        {
            if (texture == null)
            {
                return new PngExportResult(PngExportStatus.Failed, string.Empty, "Preview texture is missing.");
            }

            if (settings == null)
            {
                return new PngExportResult(PngExportStatus.Failed, string.Empty, "Export settings are missing.");
            }

            if (string.IsNullOrWhiteSpace(settings.outputFolder))
            {
                return new PngExportResult(PngExportStatus.Failed, string.Empty, "Output folder is required.");
            }

            string outputFolder = ResolveOutputFolder(settings.outputFolder, projectRoot);
            string fileName = BuildFileName(settings);
            string outputPath = Path.Combine(outputFolder, fileName);
            outputPath = ResolveConflictPath(outputPath, settings.conflictMode, out bool skipped);

            if (skipped)
            {
                return new PngExportResult(PngExportStatus.Skipped, outputPath, $"Skipped existing file: {outputPath}");
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                File.WriteAllBytes(outputPath, ImageConversion.EncodeToPNG(texture));
                return new PngExportResult(PngExportStatus.Exported, outputPath, $"Exported: {outputPath}");
            }
            catch (IOException ex)
            {
                return new PngExportResult(PngExportStatus.Failed, outputPath, ex.Message);
            }
            catch (System.UnauthorizedAccessException ex)
            {
                return new PngExportResult(PngExportStatus.Failed, outputPath, ex.Message);
            }
        }

        private static string ResolveOutputFolder(string outputFolder, string projectRoot)
        {
            if (Path.IsPathRooted(outputFolder))
            {
                return Path.GetFullPath(outputFolder);
            }

            return Path.GetFullPath(Path.Combine(projectRoot, outputFolder));
        }

        private static string BuildFileName(ExportSettings settings)
        {
            string prefix = string.IsNullOrWhiteSpace(settings.filePrefix) ? "icon" : settings.filePrefix.Trim();
            string suffix = string.IsNullOrWhiteSpace(settings.fileSuffix) ? "variant" : settings.fileSuffix.Trim();
            string fileName = $"{prefix}_{suffix}";
            return Path.GetExtension(fileName).Equals(".png", System.StringComparison.OrdinalIgnoreCase)
                ? fileName
                : fileName + ".png";
        }

        private static string ResolveConflictPath(string outputPath, ExportConflictMode conflictMode, out bool skipped)
        {
            skipped = false;
            if (!File.Exists(outputPath))
            {
                return outputPath;
            }

            if (conflictMode == ExportConflictMode.Overwrite)
            {
                return outputPath;
            }

            if (conflictMode == ExportConflictMode.Skip)
            {
                skipped = true;
                return outputPath;
            }

            string folder = Path.GetDirectoryName(outputPath);
            string name = Path.GetFileNameWithoutExtension(outputPath);
            string extension = Path.GetExtension(outputPath);

            for (int index = 1; index < 10000; index++)
            {
                string candidate = Path.Combine(folder, $"{name}_{index:000}{extension}");
                if (!File.Exists(candidate))
                {
                    return candidate;
                }
            }

            return outputPath;
        }
    }
}
