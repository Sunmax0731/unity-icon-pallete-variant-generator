using Sunmax0731.IconPaletteVariantGenerator.Editor.Windows;
using Sunmax0731.IconPaletteVariantGenerator.Models;
using Sunmax0731.IconPaletteVariantGenerator.Services;
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
            ValidateColorExtraction();
            ValidateColorGrouping();
            ValidateColorReplacement();
            ValidatePngExport();
            Debug.Log("ISSUE1_SCAFFOLD_VALIDATION=PASS");
            Debug.Log("ISSUE2_IMAGE_PALETTE_VALIDATION=PASS");
            Debug.Log("ISSUE3_COLOR_GROUPING_VALIDATION=PASS");
            Debug.Log("ISSUE4_REPLACEMENT_PREVIEW_VALIDATION=PASS");
            Debug.Log("ISSUE5_PNG_EXPORT_VALIDATION=PASS");
        }

        private static void ValidateColorExtraction()
        {
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.SetPixels32(new[]
            {
                new Color32(255, 0, 0, 255),
                new Color32(255, 0, 0, 255),
                new Color32(0, 0, 255, 255),
                new Color32(0, 0, 0, 0)
            });
            texture.Apply();

            AnalyzeSettings settings = new AnalyzeSettings
            {
                alphaThreshold = 8,
                minimumPixelCount = 1,
                quantizeStep = 1,
                maxPaletteColors = 16
            };

            var entries = new ColorExtractionService().Extract(texture, settings);
            Object.DestroyImmediate(texture);

            if (entries.Count != 2)
            {
                throw new System.InvalidOperationException($"Expected 2 palette colors, got {entries.Count}.");
            }
        }

        private static void ValidateColorGrouping()
        {
            var colors = new[]
            {
                new PaletteColorEntry { id = "#FF0000", hex = "#FF0000", color = new Color32(255, 0, 0, 255), pixelCount = 4 },
                new PaletteColorEntry { id = "#F00000", hex = "#F00000", color = new Color32(240, 0, 0, 255), pixelCount = 2 },
                new PaletteColorEntry { id = "#0000FF", hex = "#0000FF", color = new Color32(0, 0, 255, 255), pixelCount = 3 },
                new PaletteColorEntry { id = "#0000F0", hex = "#0000F0", color = new Color32(0, 0, 240, 255), pixelCount = 1 }
            };

            GroupSettings settings = new GroupSettings
            {
                targetGroupCount = 2,
                distanceMode = ColorDistanceMode.Rgb
            };

            var groups = new ColorGroupingService().CreateGroups(colors, settings);
            if (groups.Count != 2)
            {
                throw new System.InvalidOperationException($"Expected 2 color groups, got {groups.Count}.");
            }
        }

        private static void ValidateColorReplacement()
        {
            Texture2D source = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            source.SetPixels32(new[] { new Color32(255, 0, 0, 128) });
            source.Apply();

            PaletteVariantSession session = new PaletteVariantSession
            {
                analyzeSettings = new AnalyzeSettings { alphaThreshold = 8, quantizeStep = 1 },
                groupSettings = new GroupSettings { preserveAlpha = true },
                paletteColors = new System.Collections.Generic.List<PaletteColorEntry>
                {
                    new PaletteColorEntry
                    {
                        id = "#FF0000",
                        hex = "#FF0000",
                        color = new Color32(255, 0, 0, 255),
                        pixelCount = 1,
                        groupId = "group_01"
                    }
                },
                colorGroups = new System.Collections.Generic.List<ColorGroup>
                {
                    new ColorGroup
                    {
                        id = "group_01",
                        targetColor = new Color32(0, 0, 255, 255),
                        blendRatio = 1f
                    }
                }
            };

            Texture2D output = new ColorReplacementService().Apply(source, session);
            Color32 pixel = output.GetPixels32()[0];
            Object.DestroyImmediate(source);
            Object.DestroyImmediate(output);

            if (pixel.b != 255 || pixel.a != 128)
            {
                throw new System.InvalidOperationException("Color replacement did not preserve expected preview color and alpha.");
            }
        }

        private static void ValidatePngExport()
        {
            string tempRoot = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "IconPaletteVariantGeneratorValidation", System.Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(tempRoot);
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixels32(new[] { new Color32(0, 0, 255, 255) });
            texture.Apply();

            try
            {
                PngExportResult result = new PngExportService().Export(
                    texture,
                    new ExportSettings
                    {
                        outputFolder = "Generated",
                        filePrefix = "preview",
                        fileSuffix = "validation",
                        conflictMode = ExportConflictMode.Duplicate
                    },
                    tempRoot);

                if (result.Status != PngExportStatus.Exported || !System.IO.File.Exists(result.OutputPath))
                {
                    throw new System.InvalidOperationException("PNG export validation did not produce an output file.");
                }
            }
            finally
            {
                Object.DestroyImmediate(texture);
                if (System.IO.Directory.Exists(tempRoot))
                {
                    System.IO.Directory.Delete(tempRoot, true);
                }
            }
        }
    }
}
