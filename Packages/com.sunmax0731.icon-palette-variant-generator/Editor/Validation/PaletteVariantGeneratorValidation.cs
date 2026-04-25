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
            ValidateVersionLicenseMenus();
            ValidateColorExtraction();
            ValidateColorGrouping();
            ValidateColorReplacement();
            ValidatePngExport();
            ValidateSessionJson();
            ValidateIssue10UiPolish();
            ValidateIssue7Variations();
            ValidateIssue12VariationUx();
            ValidateIssue13AutoPreviewDebounce();
            ValidateIssue8Samples();
            Debug.Log("ISSUE1_SCAFFOLD_VALIDATION=PASS");
            Debug.Log("ISSUE2_IMAGE_PALETTE_VALIDATION=PASS");
            Debug.Log("ISSUE3_COLOR_GROUPING_VALIDATION=PASS");
            Debug.Log("ISSUE4_REPLACEMENT_PREVIEW_VALIDATION=PASS");
            Debug.Log("ISSUE5_PNG_EXPORT_VALIDATION=PASS");
            Debug.Log("ISSUE6_SESSION_JSON_VALIDATION=PASS");
            Debug.Log("ISSUE7_VARIATION_BATCH_EXPORT_VALIDATION=PASS");
            Debug.Log("ISSUE8_SAMPLE_QA_VALIDATION=PASS");
            Debug.Log("ISSUE10_UI_POLISH_VALIDATION=PASS");
            Debug.Log("ISSUE12_VARIATION_UX_VALIDATION=PASS");
            Debug.Log("ISSUE13_AUTO_PREVIEW_DEBOUNCE_VALIDATION=PASS");
        }

        private static void ValidateVersionLicenseMenus()
        {
            PaletteVariantGeneratorWindow.OpenVersionInfo();
            PaletteVariantInfoWindow infoWindow = EditorWindow.GetWindow<PaletteVariantInfoWindow>();
            if (infoWindow == null || infoWindow.titleContent == null || infoWindow.titleContent.text != "バージョン情報")
            {
                throw new System.InvalidOperationException("Version info window could not be created.");
            }

            infoWindow.Close();

            PaletteVariantGeneratorWindow.OpenLicense();
            PaletteVariantInfoWindow licenseWindow = EditorWindow.GetWindow<PaletteVariantInfoWindow>();
            if (licenseWindow == null || licenseWindow.titleContent == null || licenseWindow.titleContent.text != "ライセンス")
            {
                throw new System.InvalidOperationException("License window could not be created.");
            }

            licenseWindow.Close();
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

        private static void ValidateSessionJson()
        {
            string tempRoot = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "IconPaletteSessionValidation", System.Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(tempRoot);
            string path = System.IO.Path.Combine(tempRoot, "session.json");

            try
            {
                PaletteVariantSession session = new PaletteVariantSession
                {
                    sourceImageAssetPath = "Assets/Icons/source.png",
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
                            blendRatio = 0.5f
                        }
                    }
                };

                SessionJsonService service = new SessionJsonService();
                service.Save(path, session);
                SessionLoadResult result = service.Load(path);
                if (!result.Success || result.Session.colorGroups.Count != 1)
                {
                    throw new System.InvalidOperationException("Session JSON validation failed.");
                }
            }
            finally
            {
                if (System.IO.Directory.Exists(tempRoot))
                {
                    System.IO.Directory.Delete(tempRoot, true);
                }
            }
        }

        private static void ValidateIssue10UiPolish()
        {
            ValidateMaxColorDistance();
            ValidatePerColorHybridReplacement();
        }

        private static void ValidateIssue7Variations()
        {
            PaletteVariantSession session = new PaletteVariantSession
            {
                exportSettings = new ExportSettings { fileSuffix = "blue" },
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

            IconVariationService service = new IconVariationService();
            IconVariation first = service.EnsureActiveVariation(session);
            service.SyncActiveVariation(session);
            IconVariation second = service.DuplicateActiveVariation(session);
            session.colorGroups[0].targetColor = new Color32(0, 255, 0, 255);
            second.fileSuffix = "green";
            service.SyncActiveVariation(session);
            service.ApplyVariation(session, first.id);

            if (session.variations.Count != 2
                || session.colorGroups[0].targetColor.b != 255
                || session.exportSettings.fileSuffix != first.fileSuffix)
            {
                throw new System.InvalidOperationException("Variation snapshot validation failed.");
            }
        }

        private static void ValidateIssue8Samples()
        {
            string sampleRoot = System.IO.Path.Combine(
                System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "..")),
                "Packages",
                "com.sunmax0731.icon-palette-variant-generator",
                "Samples~",
                "SampleIcons");

            string[] sampleNames =
            {
                "transparent_64.png",
                "transparent_128.png",
                "antialias_128.png",
                "pixel_art_64.png"
            };

            foreach (string sampleName in sampleNames)
            {
                string path = System.IO.Path.Combine(sampleRoot, sampleName);
                if (!System.IO.File.Exists(path))
                {
                    throw new System.InvalidOperationException($"Sample icon is missing: {path}");
                }

                byte[] beforeBytes = System.IO.File.ReadAllBytes(path);
                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!ImageConversion.LoadImage(texture, beforeBytes))
                {
                    Object.DestroyImmediate(texture);
                    throw new System.InvalidOperationException($"Sample icon could not be loaded: {path}");
                }

                var entries = new ColorExtractionService().Extract(
                    texture,
                    new AnalyzeSettings
                    {
                        alphaThreshold = 8,
                        minimumPixelCount = 1,
                        quantizeStep = 8,
                        maxPaletteColors = 64
                    });

                Object.DestroyImmediate(texture);

                byte[] afterBytes = System.IO.File.ReadAllBytes(path);
                if (entries.Count == 0 || beforeBytes.Length != afterBytes.Length)
                {
                    throw new System.InvalidOperationException($"Sample icon validation failed: {path}");
                }

                for (int index = 0; index < beforeBytes.Length; index++)
                {
                    if (beforeBytes[index] != afterBytes[index])
                    {
                        throw new System.InvalidOperationException($"Sample icon was modified during validation: {path}");
                    }
                }
            }
        }

        private static void ValidateIssue12VariationUx()
        {
            PaletteVariantSession session = new PaletteVariantSession
            {
                exportSettings = new ExportSettings
                {
                    filePrefix = "icon",
                    fileSuffix = "blue"
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

            IconVariationService service = new IconVariationService();
            IconVariation first = service.EnsureActiveVariation(session);
            first.displayName = "Blue";
            first.fileSuffix = "blue";
            first.exportEnabled = true;
            service.SyncActiveVariation(session);

            IconVariation duplicate = service.DuplicateActiveVariation(session);
            duplicate.exportEnabled = false;

            string firstFileName = PaletteVariantGeneratorWindow.BuildVariationOutputFileName(session.exportSettings, first);
            string duplicateFileName = PaletteVariantGeneratorWindow.BuildVariationOutputFileName(session.exportSettings, duplicate);

            if (session.activeVariationId != duplicate.id
                || firstFileName != "icon_blue.png"
                || duplicateFileName != "icon_blue_copy.png"
                || duplicate.exportEnabled)
            {
                throw new System.InvalidOperationException("Variation UX validation failed.");
            }
        }

        private static void ValidateIssue13AutoPreviewDebounce()
        {
            Texture2D smallTexture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Texture2D largeTexture = new Texture2D(1024, 1024, TextureFormat.RGBA32, false);

            try
            {
                double smallDelay = PaletteVariantGeneratorWindow.GetAutoPreviewDebounceSeconds(smallTexture);
                double largeDelay = PaletteVariantGeneratorWindow.GetAutoPreviewDebounceSeconds(largeTexture);
                if (smallDelay <= 0d || largeDelay <= smallDelay)
                {
                    throw new System.InvalidOperationException("Auto Preview debounce validation failed.");
                }
            }
            finally
            {
                Object.DestroyImmediate(smallTexture);
                Object.DestroyImmediate(largeTexture);
            }
        }

        private static void ValidateMaxColorDistance()
        {
            var colors = new[]
            {
                new PaletteColorEntry { id = "#FF0000", hex = "#FF0000", color = new Color32(255, 0, 0, 255), pixelCount = 1 },
                new PaletteColorEntry { id = "#0000FF", hex = "#0000FF", color = new Color32(0, 0, 255, 255), pixelCount = 1 }
            };

            var broadGroups = new ColorGroupingService().CreateGroups(
                colors,
                new GroupSettings
                {
                    targetGroupCount = 1,
                    distanceMode = ColorDistanceMode.Rgb,
                    maxColorDistance = 441f
                });

            var strictGroups = new ColorGroupingService().CreateGroups(
                colors,
                new GroupSettings
                {
                    targetGroupCount = 1,
                    distanceMode = ColorDistanceMode.Rgb,
                    maxColorDistance = 10f
                });

            if (broadGroups.Count != 1 || strictGroups.Count != 2)
            {
                throw new System.InvalidOperationException("Max color distance validation failed.");
            }
        }

        private static void ValidatePerColorHybridReplacement()
        {
            Texture2D source = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            source.SetPixels32(new[] { new Color32(255, 0, 0, 255) });
            source.Apply();

            PaletteVariantSession session = new PaletteVariantSession
            {
                analyzeSettings = new AnalyzeSettings { alphaThreshold = 0, quantizeStep = 1 },
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
                        blendRatio = 1f,
                        replacementMode = ColorReplacementMode.Hybrid
                    }
                },
                colorRules = new System.Collections.Generic.List<ColorReplacementRule>
                {
                    new ColorReplacementRule
                    {
                        id = "rule_01",
                        groupId = "group_01",
                        colorEntryId = "#FF0000",
                        scope = ColorReplacementScope.ColorEntry,
                        targetColor = new Color32(0, 255, 0, 255),
                        blendRatio = 1f,
                        enabled = true
                    }
                }
            };

            Texture2D output = new ColorReplacementService().Apply(source, session);
            Color32 pixel = output.GetPixels32()[0];
            Object.DestroyImmediate(source);
            Object.DestroyImmediate(output);

            if (pixel.g != 255 || pixel.b != 0)
            {
                throw new System.InvalidOperationException("Hybrid color replacement validation failed.");
            }
        }
    }
}
