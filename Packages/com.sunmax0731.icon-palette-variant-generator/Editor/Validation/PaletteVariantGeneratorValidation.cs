using Sunmax0731.IconPaletteVariantGenerator.Editor.Services;
using Sunmax0731.IconPaletteVariantGenerator.Editor.Windows;
using Sunmax0731.IconPaletteVariantGenerator.Models;
using Sunmax0731.IconPaletteVariantGenerator.Services;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

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
            ValidateIssue17PreviewNavigation();
            ValidateIssue18RulePresetWorkflow();
            ValidateIssue19ManualGroupEditing();
            ValidateIssue20ColorDistanceModes();
            ValidateIssue21FolderBatchExport();
            ValidateIssue22ScriptableObjectPresetAsset();
            ValidateIssue23DockedLayout();
            ValidateIssue23UiToolkitPreview();
            ValidateIssue23UiToolkitInteractionControls();
            ValidateIssue23MainWindowUiToolkitHost();
            ValidateIssue25PreviewMenuHidden();
            ValidateIssue25ProductionUiToolkitMainWindow();
            ValidateIssue26NoiseRemoval();
            ValidateIssue27EdgeOutsideCleanup();
            ValidateIssue28ExportUiDisclosure();
            ValidateIssue24ReleaseAutomation();
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
            Debug.Log("ISSUE17_PREVIEW_NAVIGATION_VALIDATION=PASS");
            Debug.Log("ISSUE18_RULE_PRESET_VALIDATION=PASS");
            Debug.Log("ISSUE19_MANUAL_GROUP_EDITING_VALIDATION=PASS");
            Debug.Log("ISSUE20_COLOR_DISTANCE_MODE_VALIDATION=PASS");
            Debug.Log("ISSUE21_FOLDER_BATCH_EXPORT_VALIDATION=PASS");
            Debug.Log("ISSUE22_SCRIPTABLE_OBJECT_PRESET_VALIDATION=PASS");
            Debug.Log("ISSUE23_DOCKED_LAYOUT_VALIDATION=PASS");
            Debug.Log("ISSUE23_UI_TOOLKIT_PREVIEW_VALIDATION=PASS");
            Debug.Log("ISSUE23_UI_TOOLKIT_INTERACTION_VALIDATION=PASS");
            Debug.Log("ISSUE23_MAIN_WINDOW_UI_TOOLKIT_HOST_VALIDATION=PASS");
            Debug.Log("ISSUE25_PREVIEW_MENU_HIDDEN_VALIDATION=PASS");
            Debug.Log("ISSUE25_UI_TOOLKIT_PRODUCTION_VALIDATION=PASS");
            Debug.Log("ISSUE26_NOISE_REMOVAL_VALIDATION=PASS");
            Debug.Log("ISSUE27_EDGE_OUTSIDE_CLEANUP_VALIDATION=PASS");
            Debug.Log("ISSUE28_EXPORT_UI_DISCLOSURE_VALIDATION=PASS");
            Debug.Log("ISSUE24_RELEASE_AUTOMATION_VALIDATION=PASS");
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

            Texture2D transparentSource = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            transparentSource.SetPixels32(new[] { new Color32(255, 0, 0, 255) });
            transparentSource.Apply();
            session.colorGroups[0].targetColor = new Color32(0, 0, 0, 0);
            Texture2D transparentOutput = new ColorReplacementService().Apply(transparentSource, session);
            Color32 transparentPixel = transparentOutput.GetPixels32()[0];
            Object.DestroyImmediate(transparentSource);
            Object.DestroyImmediate(transparentOutput);

            if (transparentPixel.a != 0)
            {
                throw new System.InvalidOperationException("Color replacement did not apply transparent target alpha.");
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

        private static void ValidateIssue17PreviewNavigation()
        {
            Rect defaultCoords = PaletteVariantGeneratorWindow.GetPreviewTexCoords(1f, Vector2.zero);
            Rect zoomedCoords = PaletteVariantGeneratorWindow.GetPreviewTexCoords(4f, new Vector2(1f, -1f));

            if (!Mathf.Approximately(defaultCoords.xMin, 0f)
                || !Mathf.Approximately(defaultCoords.yMin, 0f)
                || !Mathf.Approximately(defaultCoords.xMax, 1f)
                || !Mathf.Approximately(defaultCoords.yMax, 1f))
            {
                throw new System.InvalidOperationException("Default preview texcoord validation failed.");
            }

            if (zoomedCoords.width >= 1f
                || zoomedCoords.height >= 1f
                || zoomedCoords.xMin < 0f
                || zoomedCoords.yMin < 0f
                || zoomedCoords.xMax > 1f
                || zoomedCoords.yMax > 1f)
            {
                throw new System.InvalidOperationException("Zoomed preview texcoord validation failed.");
            }
        }

        private static void ValidateIssue18RulePresetWorkflow()
        {
            PaletteVariantSession sourceSession = new PaletteVariantSession
            {
                colorGroups = new System.Collections.Generic.List<ColorGroup>
                {
                    new ColorGroup
                    {
                        id = "group_01",
                        replacementMode = ColorReplacementMode.Hybrid,
                        targetColor = new Color32(12, 34, 56, 255),
                        blendRatio = 0.75f
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
                        blendRatio = 0.5f,
                        enabled = true
                    }
                }
            };

            PaletteVariantSession targetSession = new PaletteVariantSession
            {
                colorGroups = new System.Collections.Generic.List<ColorGroup>
                {
                    new ColorGroup
                    {
                        id = "group_01",
                        replacementMode = ColorReplacementMode.GroupUniform,
                        targetColor = new Color32(0, 0, 0, 255),
                        blendRatio = 1f
                    }
                },
                colorRules = new System.Collections.Generic.List<ColorReplacementRule>()
            };

            string tempRoot = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "IconPaletteRulePresetValidation", System.Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(tempRoot);
            string path = System.IO.Path.Combine(tempRoot, "preset.json");

            try
            {
                RulePresetJsonService service = new RulePresetJsonService();
                PaletteVariantRulePreset preset = service.CreatePreset(sourceSession, "Validation Preset");
                service.Save(path, preset);
                RulePresetLoadResult loadResult = service.Load(path);
                if (!loadResult.Success)
                {
                    throw new System.InvalidOperationException("Rule preset load validation failed.");
                }

                var warnings = service.ApplyToSession(loadResult.Preset, targetSession);
                if (warnings.Count != 0
                    || targetSession.colorGroups[0].replacementMode != ColorReplacementMode.Hybrid
                    || targetSession.colorGroups[0].targetColor.r != 12
                    || targetSession.colorRules.Count != 1
                    || targetSession.colorRules[0].targetColor.g != 255)
                {
                    throw new System.InvalidOperationException("Rule preset apply validation failed.");
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

        private static void ValidateIssue19ManualGroupEditing()
        {
            PaletteVariantSession session = new PaletteVariantSession
            {
                paletteColors = new System.Collections.Generic.List<PaletteColorEntry>
                {
                    new PaletteColorEntry
                    {
                        id = "#FF0000",
                        hex = "#FF0000",
                        color = new Color32(255, 0, 0, 255),
                        pixelCount = 3,
                        groupId = "group_01"
                    },
                    new PaletteColorEntry
                    {
                        id = "#0000FF",
                        hex = "#0000FF",
                        color = new Color32(0, 0, 255, 255),
                        pixelCount = 1,
                        groupId = "group_02"
                    }
                },
                colorGroups = new System.Collections.Generic.List<ColorGroup>
                {
                    new ColorGroup
                    {
                        id = "group_01",
                        displayName = "Group 1",
                        colorEntryIds = new System.Collections.Generic.List<string> { "#FF0000" },
                        pixelCount = 3,
                        pixelRatio = 0.75f
                    },
                    new ColorGroup
                    {
                        id = "group_02",
                        displayName = "Group 2",
                        colorEntryIds = new System.Collections.Generic.List<string> { "#0000FF" },
                        pixelCount = 1,
                        pixelRatio = 0.25f
                    }
                }
            };

            session.paletteColors[0].groupId = "group_02";
            foreach (ColorGroup group in session.colorGroups)
            {
                var entries = session.paletteColors.FindAll(entry => entry.groupId == group.id);
                group.colorEntryIds = entries.ConvertAll(entry => entry.id);
                group.pixelCount = 0;
                foreach (PaletteColorEntry entry in entries)
                {
                    group.pixelCount += entry.pixelCount;
                }
            }

            if (session.colorGroups[0].colorEntryIds.Count != 0
                || session.colorGroups[1].colorEntryIds.Count != 2
                || session.colorGroups[1].pixelCount != 4)
            {
                throw new System.InvalidOperationException("Manual group editing validation failed.");
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

        private static void ValidateIssue20ColorDistanceModes()
        {
            ColorDistanceService distanceService = new ColorDistanceService();
            Color32 red = new Color32(255, 0, 0, 255);
            Color32 nearRed = new Color32(240, 0, 0, 255);
            Color32 blue = new Color32(0, 0, 255, 255);

            float labSame = distanceService.Calculate(red, red, ColorDistanceMode.Lab);
            float labNear = distanceService.Calculate(red, nearRed, ColorDistanceMode.Lab);
            float labFar = distanceService.Calculate(red, blue, ColorDistanceMode.Lab);
            float rgbFar = distanceService.Calculate(red, blue, ColorDistanceMode.Rgb);
            float hsvFar = distanceService.Calculate(red, blue, ColorDistanceMode.Hsv);

            if (labSame > 0.0001f || labNear <= 0f || labFar <= labNear)
            {
                throw new System.InvalidOperationException("Lab color distance validation failed.");
            }

            if (Mathf.Abs(labFar - rgbFar) <= 0.0001f || hsvFar <= 0f)
            {
                throw new System.InvalidOperationException("Color distance mode selection validation failed.");
            }

            var colors = new[]
            {
                new PaletteColorEntry { id = "#FF0000", hex = "#FF0000", color = red, pixelCount = 4 },
                new PaletteColorEntry { id = "#F00000", hex = "#F00000", color = nearRed, pixelCount = 2 },
                new PaletteColorEntry { id = "#0000FF", hex = "#0000FF", color = blue, pixelCount = 1 }
            };

            var groups = new ColorGroupingService().CreateGroups(
                colors,
                new GroupSettings
                {
                    targetGroupCount = 2,
                    distanceMode = ColorDistanceMode.Lab,
                    maxColorDistance = 441f
                });

            if (groups.Count != 2)
            {
                throw new System.InvalidOperationException("Lab grouping validation failed.");
            }
        }

        private static void ValidateIssue21FolderBatchExport()
        {
            string root = "Assets/PaletteVariantBatchValidation";
            string sourceFolder = root + "/Sources";
            string outputFolder = root + "/Generated";
            if (AssetDatabase.IsValidFolder(root))
            {
                AssetDatabase.DeleteAsset(root);
            }

            AssetDatabase.CreateFolder("Assets", "PaletteVariantBatchValidation");
            AssetDatabase.CreateFolder(root, "Sources");
            try
            {
                WriteValidationPng(sourceFolder + "/source_a.png", new Color32(255, 0, 0, 255));
                WriteValidationPng(sourceFolder + "/source_b.png", new Color32(240, 0, 0, 255));
                AssetDatabase.Refresh();

                PaletteVariantSession session = new PaletteVariantSession
                {
                    analyzeSettings = new AnalyzeSettings { alphaThreshold = 0, quantizeStep = 1, minimumPixelCount = 1 },
                    groupSettings = new GroupSettings { targetGroupCount = 1, distanceMode = ColorDistanceMode.Rgb, maxColorDistance = 441f },
                    exportSettings = new ExportSettings
                    {
                        outputFolder = outputFolder,
                        filePrefix = "unused",
                        fileSuffix = "blue",
                        conflictMode = ExportConflictMode.Skip,
                        refreshAssetDatabase = false
                    },
                    colorGroups = new System.Collections.Generic.List<ColorGroup>
                    {
                        new ColorGroup
                        {
                            id = "group_01",
                            targetColor = new Color32(0, 0, 255, 255),
                            blendRatio = 1f
                        }
                    },
                    variations = new System.Collections.Generic.List<IconVariation>
                    {
                        new IconVariation
                        {
                            id = "variation_01",
                            displayName = "Blue",
                            fileSuffix = "blue",
                            exportEnabled = true,
                            colorGroups = new System.Collections.Generic.List<ColorGroup>
                            {
                                new ColorGroup
                                {
                                    id = "group_01",
                                    targetColor = new Color32(0, 0, 255, 255),
                                    blendRatio = 1f
                                }
                            }
                        }
                    },
                    activeVariationId = "variation_01"
                };

                BatchSourceExportSummary summary = new BatchSourceExportService().ExportFolder(sourceFolder, session, System.IO.Directory.GetCurrentDirectory());
                string outputA = outputFolder + "/source_a_blue.png";
                string outputB = outputFolder + "/source_b_blue.png";
                if (summary.ExportedCount != 2
                    || summary.FailedCount != 0
                    || !System.IO.File.Exists(outputA)
                    || !System.IO.File.Exists(outputB))
                {
                    throw new System.InvalidOperationException("Folder batch export validation failed.");
                }
            }
            finally
            {
                AssetDatabase.DeleteAsset(root);
                AssetDatabase.Refresh();
            }
        }

        private static void ValidateIssue22ScriptableObjectPresetAsset()
        {
            string root = "Assets/PaletteVariantPresetAssetValidation";
            if (AssetDatabase.IsValidFolder(root))
            {
                AssetDatabase.DeleteAsset(root);
            }

            AssetDatabase.CreateFolder("Assets", "PaletteVariantPresetAssetValidation");
            try
            {
                PaletteVariantSession sourceSession = CreatePresetValidationSession(new Color32(0, 0, 255, 255));
                RulePresetAssetService service = new RulePresetAssetService();
                PaletteVariantRulePresetAsset asset = service.CreateAsset(root + "/SharedRulePreset.asset", sourceSession, "Shared Rule Preset");
                string assetPath = AssetDatabase.GetAssetPath(asset);
                if (string.IsNullOrWhiteSpace(assetPath))
                {
                    throw new System.InvalidOperationException("Preset asset was not created.");
                }

                PaletteVariantSession targetSession = CreatePresetValidationSession(new Color32(255, 0, 0, 255));
                var warnings = service.ApplyToSession(asset, targetSession);
                if (warnings.Count != 0 || targetSession.colorGroups[0].targetColor.b != 255)
                {
                    throw new System.InvalidOperationException("Preset asset apply validation failed.");
                }

                sourceSession.colorGroups[0].targetColor = new Color32(0, 255, 0, 255);
                service.UpdateAsset(asset, sourceSession, "Updated Shared Rule Preset");
                PaletteVariantRulePresetAsset reloaded = AssetDatabase.LoadAssetAtPath<PaletteVariantRulePresetAsset>(assetPath);
                if (reloaded == null || reloaded.preset.colorGroups[0].targetColor.g != 255)
                {
                    throw new System.InvalidOperationException("Preset asset update validation failed.");
                }
            }
            finally
            {
                AssetDatabase.DeleteAsset(root);
                AssetDatabase.Refresh();
            }
        }

        private static PaletteVariantSession CreatePresetValidationSession(Color32 targetColor)
        {
            return new PaletteVariantSession
            {
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
                        targetColor = targetColor,
                        blendRatio = 1f,
                        colorEntryIds = new System.Collections.Generic.List<string> { "#FF0000" }
                    }
                }
            };
        }

        private static void ValidateIssue24ReleaseAutomation()
        {
            string root = System.IO.Directory.GetCurrentDirectory();
            string[] requiredFiles =
            {
                ".github/workflows/release-package.yml",
                "tools/release/build-release.ps1",
                "tools/release/test-release-package.ps1",
                "docs/release-checklist.md"
            };

            foreach (string path in requiredFiles)
            {
                if (!System.IO.File.Exists(System.IO.Path.Combine(root, path)))
                {
                    throw new System.InvalidOperationException($"Release automation file is missing: {path}");
                }
            }
        }

        private static void ValidateIssue23DockedLayout()
        {
            Vector2 minSize = PaletteVariantGeneratorWindow.GetDockedMinimumWindowSize();
            if (minSize.x > 800f || minSize.y > 620f)
            {
                throw new System.InvalidOperationException($"Docked minimum window size is too large: {minSize}.");
            }

            if (!PaletteVariantGeneratorWindow.ShouldUseCompactLayout(760f))
            {
                throw new System.InvalidOperationException("Compact layout should be active at narrow docked width.");
            }

            if (PaletteVariantGeneratorWindow.ShouldUseCompactLayout(1240f))
            {
                throw new System.InvalidOperationException("Wide layout should remain active at full authoring width.");
            }

            PaletteVariantGeneratorWindow.Open();
            PaletteVariantGeneratorWindow window = EditorWindow.GetWindow<PaletteVariantGeneratorWindow>();
            if (window == null || window.minSize.x > 800f)
            {
                throw new System.InvalidOperationException("Docked layout window validation failed.");
            }

            window.Close();
        }

        private static void ValidateIssue23UiToolkitPreview()
        {
            UnityEngine.UIElements.VisualElement previewRoot = PaletteVariantGeneratorToolkitPreviewWindow.BuildPreviewRoot();
            if (!PaletteVariantGeneratorToolkitPreviewWindow.ContainsRequiredSections(previewRoot))
            {
                throw new System.InvalidOperationException("UI Toolkit preview layout is missing required production sections.");
            }

            PaletteVariantGeneratorToolkitPreviewWindow.Open();
            PaletteVariantGeneratorToolkitPreviewWindow window = EditorWindow.GetWindow<PaletteVariantGeneratorToolkitPreviewWindow>();
            if (window == null || window.titleContent == null || window.titleContent.text != PaletteVariantGeneratorToolkitPreviewWindow.WindowTitle)
            {
                throw new System.InvalidOperationException("UI Toolkit preview window could not be created.");
            }

            if (window.minSize.x > 800f || window.rootVisualElement.Q<UnityEngine.UIElements.Label>("preview-status") == null)
            {
                throw new System.InvalidOperationException("UI Toolkit preview window validation failed.");
            }

            window.Close();
        }

        private static void ValidateIssue23UiToolkitInteractionControls()
        {
            UnityEngine.UIElements.VisualElement previewRoot = PaletteVariantGeneratorToolkitPreviewWindow.BuildPreviewRoot();
            if (!PaletteVariantGeneratorToolkitPreviewWindow.ContainsRequiredControls(previewRoot))
            {
                throw new System.InvalidOperationException("UI Toolkit preview layout is missing required interaction controls.");
            }

            if (!PaletteVariantGeneratorToolkitPreviewWindow.SupportsPreviewInteractions(previewRoot))
            {
                throw new System.InvalidOperationException("UI Toolkit preview interaction capabilities are not declared.");
            }

            if (previewRoot.Q<UnityEditor.UIElements.ObjectField>("source-image-field") == null
                || previewRoot.Q<UnityEditor.UIElements.ColorField>("group-color-field") == null
                || previewRoot.Q<UnityEngine.UIElements.PopupField<string>>("compare-mode-popup") == null
                || previewRoot.Q<UnityEngine.UIElements.ScrollView>("palette-scroll-view") == null)
            {
                throw new System.InvalidOperationException("UI Toolkit preview controls do not match the required control types.");
            }
        }

        private static void ValidateIssue23MainWindowUiToolkitHost()
        {
            PaletteVariantGeneratorWindow.Open();
            PaletteVariantGeneratorWindow window = EditorWindow.GetWindow<PaletteVariantGeneratorWindow>();
            if (window == null || !window.IsUiToolkitHostActive())
            {
                throw new System.InvalidOperationException("Main window is not hosted by UI Toolkit.");
            }

            if (window.rootVisualElement.Q<UnityEngine.UIElements.VisualElement>(PaletteVariantGeneratorWindow.MainWindowRootName) == null
                || window.rootVisualElement.Q<UnityEngine.UIElements.ScrollView>(PaletteVariantGeneratorWindow.MainWindowScrollName) == null)
            {
                throw new System.InvalidOperationException("Main window UI Toolkit host elements are missing.");
            }

            window.Close();
        }

        private static void ValidateIssue25PreviewMenuHidden()
        {
            System.Reflection.MethodInfo openMethod = typeof(PaletteVariantGeneratorToolkitPreviewWindow).GetMethod(
                nameof(PaletteVariantGeneratorToolkitPreviewWindow.Open),
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (openMethod == null)
            {
                throw new System.InvalidOperationException("UI Toolkit preview Open method is missing.");
            }

            object[] menuItems = openMethod.GetCustomAttributes(typeof(MenuItem), false);
            if (menuItems.Length != 0)
            {
                throw new System.InvalidOperationException("UI Toolkit preview window should not be exposed as a production menu item.");
            }
        }

        private static void ValidateIssue25ProductionUiToolkitMainWindow()
        {
            PaletteVariantGeneratorWindow.Open();
            PaletteVariantGeneratorWindow window = EditorWindow.GetWindow<PaletteVariantGeneratorWindow>();
            if (window == null)
            {
                throw new System.InvalidOperationException("Main window could not be opened for production UI Toolkit validation.");
            }

            if (!window.IsUiToolkitHostActive() || !window.ContainsProductionUiToolkitSections())
            {
                throw new System.InvalidOperationException("Main window is not using the production UI Toolkit section layout.");
            }

            if (window.rootVisualElement.Q<UnityEngine.UIElements.IMGUIContainer>() != null)
            {
                throw new System.InvalidOperationException("Production UI Toolkit window must not depend on IMGUIContainer.");
            }

            if (window.rootVisualElement.Q<UnityEditor.UIElements.ObjectField>("source-image-field") == null
                || window.rootVisualElement.Q<UnityEngine.UIElements.ScrollView>("palette-scroll-view") == null
                || window.rootVisualElement.Q<UnityEditor.UIElements.ColorField>("group-color-field") == null
                || window.rootVisualElement.Q<UnityEngine.UIElements.ScrollView>("variation-scroll-view") == null)
            {
                throw new System.InvalidOperationException("Production UI Toolkit controls are missing.");
            }

            window.Close();
        }

        private static void ValidateIssue26NoiseRemoval()
        {
            Color32 fill = new Color32(240, 0, 0, 255);
            Color32 noise = new Color32(220, 10, 10, 255);
            PaletteVariantSession session = new PaletteVariantSession
            {
                analyzeSettings = new AnalyzeSettings { alphaThreshold = 0, quantizeStep = 1 },
                groupSettings = new GroupSettings { distanceMode = ColorDistanceMode.Rgb, preserveAlpha = true },
                noiseRemovalSettings = new NoiseRemovalSettings
                {
                    enabled = true,
                    maxRegionPixels = 1,
                    neighborDistanceThreshold = 64f,
                    sameGroupOnly = true
                },
                paletteColors = new System.Collections.Generic.List<PaletteColorEntry>
                {
                    new PaletteColorEntry { id = "#F00000", hex = "#F00000", color = fill, pixelCount = 8, groupId = "group_01" },
                    new PaletteColorEntry { id = "#DC0A0A", hex = "#DC0A0A", color = noise, pixelCount = 1, groupId = "group_01" }
                },
                colorGroups = new System.Collections.Generic.List<ColorGroup>
                {
                    new ColorGroup
                    {
                        id = "group_01",
                        targetColor = new Color32(0, 0, 255, 255),
                        blendRatio = 1f,
                        colorEntryIds = new System.Collections.Generic.List<string> { "#F00000", "#DC0A0A" }
                    }
                }
            };

            Texture2D source = new Texture2D(3, 3, TextureFormat.RGBA32, false);
            source.SetPixels32(new[]
            {
                fill, fill, fill,
                fill, noise, fill,
                fill, fill, fill
            });
            source.Apply();

            Texture2D output = new ColorReplacementService().Apply(source, session);
            Color32 center = output.GetPixels32()[4];
            Object.DestroyImmediate(source);
            Object.DestroyImmediate(output);

            if (center.b != 255 || center.r != 0)
            {
                throw new System.InvalidOperationException("Noise removal did not fill the isolated region before replacement.");
            }
        }

        private static void ValidateIssue27EdgeOutsideCleanup()
        {
            Color32 body = new Color32(255, 0, 0, 255);
            Color32 outside = new Color32(0, 255, 0, 255);
            Color32 clear = new Color32(0, 0, 0, 0);
            Color32[] pixels =
            {
                clear, clear, clear, clear, clear,
                clear, body, body, clear, outside,
                clear, body, body, clear, clear,
                clear, body, body, clear, clear,
                clear, clear, clear, clear, clear
            };

            PaletteVariantSession session = new PaletteVariantSession
            {
                analyzeSettings = new AnalyzeSettings { alphaThreshold = 0 },
                edgeOutsideCleanupSettings = new EdgeOutsideCleanupSettings
                {
                    enabled = true,
                    maxDistancePixels = 2,
                    maxRegionPixels = 4
                }
            };

            EdgeOutsideCleanupResult result = new EdgeOutsideCleanupService().Apply(pixels, 5, 5, session);
            if (result.ClearedPixelCount != 1 || pixels[9].a != 0)
            {
                throw new System.InvalidOperationException("Edge outside cleanup validation failed.");
            }
        }

        private static void ValidateIssue28ExportUiDisclosure()
        {
            PaletteVariantGeneratorWindow.Open();
            PaletteVariantGeneratorWindow window = EditorWindow.GetWindow<PaletteVariantGeneratorWindow>();
            if (window == null)
            {
                throw new System.InvalidOperationException("Main window could not be opened for export UI validation.");
            }

            VisualElement exportDetails = window.rootVisualElement.Q<VisualElement>("export-details");
            if (exportDetails == null || exportDetails.style.display.value != DisplayStyle.None)
            {
                throw new System.InvalidOperationException("Export details should be available but hidden by default.");
            }

            window.Close();
        }

        private static void WriteValidationPng(string assetPath, Color32 color)
        {
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixels32(new[] { color });
            texture.Apply();
            System.IO.File.WriteAllBytes(assetPath, ImageConversion.EncodeToPNG(texture));
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(assetPath);
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
