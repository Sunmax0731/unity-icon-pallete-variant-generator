using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sunmax0731.IconPaletteVariantGenerator.Models;
using Sunmax0731.IconPaletteVariantGenerator.Services;
using UnityEditor;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Editor.Services
{
    /// <summary>
    /// Applies the current replacement workflow to every Texture2D in a project folder.
    /// </summary>
    public sealed class BatchSourceExportService
    {
        private readonly TextureAssetLoader textureAssetLoader;
        private readonly ColorExtractionService colorExtractionService;
        private readonly ColorGroupingService colorGroupingService;
        private readonly ColorReplacementService colorReplacementService;
        private readonly PngExportService pngExportService;
        private readonly ExportedTextureImportSettingsService exportedTextureImportSettingsService;

        public BatchSourceExportService()
            : this(
                new TextureAssetLoader(),
                new ColorExtractionService(),
                new ColorGroupingService(),
                new ColorReplacementService(),
                new PngExportService(),
                new ExportedTextureImportSettingsService())
        {
        }

        public BatchSourceExportService(
            TextureAssetLoader textureAssetLoader,
            ColorExtractionService colorExtractionService,
            ColorGroupingService colorGroupingService,
            ColorReplacementService colorReplacementService,
            PngExportService pngExportService)
            : this(
                textureAssetLoader,
                colorExtractionService,
                colorGroupingService,
                colorReplacementService,
                pngExportService,
                new ExportedTextureImportSettingsService())
        {
        }

        public BatchSourceExportService(
            TextureAssetLoader textureAssetLoader,
            ColorExtractionService colorExtractionService,
            ColorGroupingService colorGroupingService,
            ColorReplacementService colorReplacementService,
            PngExportService pngExportService,
            ExportedTextureImportSettingsService exportedTextureImportSettingsService)
        {
            this.textureAssetLoader = textureAssetLoader;
            this.colorExtractionService = colorExtractionService;
            this.colorGroupingService = colorGroupingService;
            this.colorReplacementService = colorReplacementService;
            this.pngExportService = pngExportService;
            this.exportedTextureImportSettingsService = exportedTextureImportSettingsService;
        }

        public BatchSourceExportSummary ExportFolder(string folderAssetPath, PaletteVariantSession baseSession, string projectRoot)
        {
            BatchSourceExportSummary summary = new BatchSourceExportSummary();
            if (baseSession == null)
            {
                summary.AddFailure(string.Empty, string.Empty, "Session is missing.");
                return summary;
            }

            if (string.IsNullOrWhiteSpace(folderAssetPath) || !AssetDatabase.IsValidFolder(folderAssetPath))
            {
                summary.AddFailure(folderAssetPath, string.Empty, "Batch source folder is not a valid Unity project folder.");
                return summary;
            }

            List<IconVariation> variations = (baseSession.variations ?? new List<IconVariation>())
                .Where(variation => variation != null && variation.exportEnabled)
                .ToList();
            if (variations.Count == 0)
            {
                summary.AddFailure(folderAssetPath, string.Empty, "No enabled variations are available for batch export.");
                return summary;
            }

            string[] assetPaths = AssetDatabase.FindAssets("t:Texture2D", new[] { folderAssetPath })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Distinct()
                .OrderBy(path => path)
                .ToArray();

            if (assetPaths.Length == 0)
            {
                summary.AddFailure(folderAssetPath, string.Empty, "No Texture2D assets were found in the batch source folder.");
                return summary;
            }

            foreach (string assetPath in assetPaths)
            {
                ExportSourceAsset(assetPath, variations, baseSession, projectRoot, summary);
            }

            if (baseSession.exportSettings.refreshAssetDatabase)
            {
                AssetDatabase.Refresh();
            }

            return summary;
        }

        private void ExportSourceAsset(
            string assetPath,
            IReadOnlyList<IconVariation> variations,
            PaletteVariantSession baseSession,
            string projectRoot,
            BatchSourceExportSummary summary)
        {
            Texture2D source = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (!textureAssetLoader.TryLoadReadableTexture(source, out Texture2D readableTexture, out _, out string error))
            {
                summary.AddFailure(assetPath, string.Empty, error);
                return;
            }

            try
            {
                List<PaletteColorEntry> paletteColors = new List<PaletteColorEntry>(
                    colorExtractionService.Extract(readableTexture, baseSession.analyzeSettings));
                List<ColorGroup> sourceGroups = new List<ColorGroup>(
                    colorGroupingService.CreateGroups(paletteColors, baseSession.groupSettings));

                foreach (IconVariation variation in variations)
                {
                    PaletteVariantSession sourceSession = CreateSourceSession(assetPath, baseSession, variation, paletteColors, sourceGroups);
                    Texture2D preview = colorReplacementService.Apply(readableTexture, sourceSession);
                    PngExportResult result = pngExportService.Export(preview, sourceSession.exportSettings, projectRoot);
                    ApplyExportedTextureImportSettings(result, projectRoot);
                    Object.DestroyImmediate(preview);
                    summary.Add(assetPath, variation.displayName, result);
                }
            }
            finally
            {
                Object.DestroyImmediate(readableTexture);
            }
        }

        private void ApplyExportedTextureImportSettings(PngExportResult result, string projectRoot)
        {
            if (result == null || result.Status != PngExportStatus.Exported)
            {
                return;
            }

            exportedTextureImportSettingsService.ApplyAlphaIsTransparency(result.OutputPath, projectRoot, out _);
        }

        private static PaletteVariantSession CreateSourceSession(
            string assetPath,
            PaletteVariantSession baseSession,
            IconVariation variation,
            IReadOnlyList<PaletteColorEntry> paletteColors,
            IReadOnlyList<ColorGroup> sourceGroups)
        {
            List<ColorGroup> variationGroups = variation.colorGroups ?? new List<ColorGroup>();
            List<ColorGroup> mappedGroups = new List<ColorGroup>();
            for (int index = 0; index < sourceGroups.Count; index++)
            {
                ColorGroup sourceGroup = sourceGroups[index];
                ColorGroup ruleGroup = index < variationGroups.Count ? variationGroups[index] : null;
                mappedGroups.Add(new ColorGroup
                {
                    id = sourceGroup.id,
                    displayName = sourceGroup.displayName,
                    representativeColor = sourceGroup.representativeColor,
                    colorEntryIds = new List<string>(sourceGroup.colorEntryIds ?? new List<string>()),
                    replacementMode = ruleGroup?.replacementMode ?? sourceGroup.replacementMode,
                    targetColor = ruleGroup?.targetColor ?? sourceGroup.targetColor,
                    blendRatio = ruleGroup?.blendRatio ?? sourceGroup.blendRatio,
                    lockedGroup = ruleGroup?.lockedGroup ?? sourceGroup.lockedGroup,
                    pixelCount = sourceGroup.pixelCount,
                    pixelRatio = sourceGroup.pixelRatio
                });
            }

            HashSet<string> paletteIds = new HashSet<string>(paletteColors.Select(color => color.id));
            List<ColorReplacementRule> mappedRules = new List<ColorReplacementRule>();
            foreach (ColorReplacementRule rule in variation.colorRules ?? new List<ColorReplacementRule>())
            {
                if (rule == null || string.IsNullOrEmpty(rule.colorEntryId) || !paletteIds.Contains(rule.colorEntryId))
                {
                    continue;
                }

                mappedRules.Add(new ColorReplacementRule
                {
                    id = rule.id,
                    groupId = rule.groupId,
                    colorEntryId = rule.colorEntryId,
                    scope = rule.scope,
                    targetColor = rule.targetColor,
                    blendRatio = rule.blendRatio,
                    enabled = rule.enabled
                });
            }

            return new PaletteVariantSession
            {
                sourceImageAssetPath = assetPath,
                analyzeSettings = baseSession.analyzeSettings,
                groupSettings = baseSession.groupSettings,
                exportSettings = new ExportSettings
                {
                    outputFolder = baseSession.exportSettings.outputFolder,
                    filePrefix = Path.GetFileNameWithoutExtension(assetPath),
                    fileSuffix = string.IsNullOrWhiteSpace(variation.fileSuffix) ? variation.id : variation.fileSuffix,
                    conflictMode = baseSession.exportSettings.conflictMode,
                    refreshAssetDatabase = false
                },
                paletteColors = new List<PaletteColorEntry>(paletteColors),
                colorGroups = mappedGroups,
                colorRules = mappedRules,
                variations = new List<IconVariation>(),
                activeVariationId = string.Empty
            };
        }
    }

    public sealed class BatchSourceExportSummary
    {
        public readonly List<BatchSourceExportItem> Items = new List<BatchSourceExportItem>();

        public int ExportedCount => Items.Count(item => item.Status == PngExportStatus.Exported);

        public int SkippedCount => Items.Count(item => item.Status == PngExportStatus.Skipped);

        public int FailedCount => Items.Count(item => item.Status == PngExportStatus.Failed);

        public void Add(string assetPath, string variationName, PngExportResult result)
        {
            Items.Add(new BatchSourceExportItem
            {
                AssetPath = assetPath,
                VariationName = variationName,
                Status = result.Status,
                OutputPath = result.OutputPath,
                Message = result.Message
            });
        }

        public void AddFailure(string assetPath, string variationName, string message)
        {
            Items.Add(new BatchSourceExportItem
            {
                AssetPath = assetPath,
                VariationName = variationName,
                Status = PngExportStatus.Failed,
                OutputPath = string.Empty,
                Message = message
            });
        }
    }

    public sealed class BatchSourceExportItem
    {
        public string AssetPath;
        public string VariationName;
        public PngExportStatus Status;
        public string OutputPath;
        public string Message;
    }
}
