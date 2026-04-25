using System.Collections.Generic;
using System.Linq;
using Sunmax0731.IconPaletteVariantGenerator.Editor.Services;
using Sunmax0731.IconPaletteVariantGenerator.Models;
using Sunmax0731.IconPaletteVariantGenerator.Services;
using Sunmax0731.IconPaletteVariantGenerator.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sunmax0731.IconPaletteVariantGenerator.Editor.Windows
{
    /// <summary>
    /// Entry EditorWindow for the Icon Palette Variant Generator.
    /// </summary>
    public sealed class PaletteVariantGeneratorWindow : EditorWindow
    {
        private const float LeftPaneWidth = 300f;
        private const float RightPaneWidth = 420f;
        private const float PaneGap = 14f;
        private const float MinPreviewHeight = 260f;
        private const float MinPreviewZoom = 1f;
        private const float MaxPreviewZoom = 8f;
        private const float PaletteListHeight = 170f;
        private const float VariationListHeight = 126f;
        private const double AutoPreviewDebounceSeconds = 0.25d;
        private const double LargeImageAutoPreviewDebounceSeconds = 0.75d;
        private const int LargeImageAutoPreviewPixelCount = 1024 * 1024;
        internal const string ProductName = "Unity Icon Palette Variant Generator";
        internal const string PackageName = "com.sunmax0731.icon-palette-variant-generator";
        internal const string PackageVersion = "1.0.0";
        internal const string ValidatedUnityVersion = "6000.4.0f1";
        internal const string ReleaseUrl = "https://github.com/Sunmax0731/unity-icon-pallete-variant-generator/releases/tag/v1.0.0";
        private const string LanguageModePrefsKey = "Sunmax.IconPaletteVariantGenerator.LanguageMode";
        private const string AutoPreviewPrefsKey = "Sunmax.IconPaletteVariantGenerator.AutoPreview";
        private static readonly Color SeparatorColor = new Color(0.25f, 0.25f, 0.25f, 0.8f);
        private static readonly Color OverlayColor = new Color(0.1f, 0.65f, 1f, 0.34f);
        private static readonly Color OverlayBorderColor = new Color(0.1f, 0.65f, 1f, 0.85f);

        private PaletteVariantSession session = new PaletteVariantSession();
        private readonly TextureAssetLoader textureAssetLoader = new TextureAssetLoader();
        private readonly ColorExtractionService colorExtractionService = new ColorExtractionService();
        private readonly ColorGroupingService colorGroupingService = new ColorGroupingService();
        private readonly ColorQuantizationService colorQuantizationService = new ColorQuantizationService();
        private readonly ColorReplacementService colorReplacementService = new ColorReplacementService();
        private readonly IconVariationService variationService = new IconVariationService();
        private readonly PngExportService pngExportService = new PngExportService();
        private readonly SessionJsonService sessionJsonService = new SessionJsonService();
        private readonly RulePresetJsonService rulePresetJsonService = new RulePresetJsonService();
        private Texture2D sourceImage;
        private Texture2D readableSourceImage;
        private Texture2D afterPreview;
        private Texture2D checkerboardTexture;
        private Texture2D selectionOverlayTexture;
        private string selectionOverlayCacheKey = string.Empty;
        private Vector2 leftScroll;
        private Vector2 paletteScroll;
        private Vector2 variationScroll;
        private Vector2 rightScroll;
        private string sourceAssetPath = string.Empty;
        private string reportMessage = "Select a project PNG or Texture2D asset, then click Analyze.";
        private MessageType reportType = MessageType.Info;
        private PaletteVariantLanguageMode languageMode = PaletteVariantLanguageMode.Auto;
        private PaletteVariantDisplayLanguage displayLanguage = PaletteVariantDisplayLanguage.English;
        private ParameterHelpWindow parameterHelpWindow;
        private bool autoPreviewEnabled = true;
        private bool autoPreviewPending;
        private double autoPreviewScheduledTime;
        private PreviewCompareMode previewCompareMode = PreviewCompareMode.SideBySide;
        private float previewZoom = 1f;
        private float previewSplit = 0.5f;
        private Vector2 previewPan;
        private string selectedGroupId = string.Empty;
        private string selectedColorEntryId = string.Empty;

        [MenuItem("Tools/Palette Variant Generator/開く")]
        public static void Open()
        {
            OpenWindow();
        }

        private static void OpenWindow()
        {
            PaletteVariantGeneratorWindow window = GetWindow<PaletteVariantGeneratorWindow>();
            window.titleContent = new GUIContent("Palette Variant Generator");
            window.minSize = new Vector2(1240f, 620f);
            window.Show();
        }

        [MenuItem("Tools/Palette Variant Generator/バージョン情報")]
        public static void OpenVersionInfo()
        {
            PaletteVariantInfoWindow.OpenInfo();
        }

        [MenuItem("Tools/Palette Variant Generator/ライセンス")]
        public static void OpenLicense()
        {
            PaletteVariantInfoWindow.OpenLicense();
        }

        private void OnEnable()
        {
            checkerboardTexture = CreateCheckerboardTexture();
            autoPreviewEnabled = EditorPrefs.GetBool(AutoPreviewPrefsKey, true);
            LoadLanguageMode();
        }

        private void OnGUI()
        {
            DrawToolbar();
            EditorGUILayout.HelpBox(reportMessage, reportType);

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawLeftPane();
                DrawPaneSeparator();
                GUILayout.Space(PaneGap);
                DrawCenterPane();
                GUILayout.Space(PaneGap);
                DrawPaneSeparator();
                DrawRightPane();
            }
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUILayout.LabelField(T("sourceImage", "Source Image"), GUILayout.Width(86f));
                sourceImage = (Texture2D)EditorGUILayout.ObjectField(sourceImage, typeof(Texture2D), false, GUILayout.MinWidth(220f));

                using (new EditorGUI.DisabledScope(sourceImage == null))
                {
                    if (GUILayout.Button(T("analyze", "Analyze"), EditorStyles.toolbarButton, GUILayout.Width(76f)))
                    {
                        AnalyzeSourceImage();
                    }
                }

                using (new EditorGUI.DisabledScope(session.paletteColors.Count == 0))
                {
                    if (GUILayout.Button(T("autoGroup", "Auto Group"), EditorStyles.toolbarButton, GUILayout.Width(88f)))
                    {
                        AutoGroupPalette();
                    }
                }

                using (new EditorGUI.DisabledScope(afterPreview == null))
                {
                    if (GUILayout.Button(T("export", "Export"), EditorStyles.toolbarButton, GUILayout.Width(70f)))
                    {
                        ExportPreview();
                    }
                }

                using (new EditorGUI.DisabledScope(readableSourceImage == null || session.variations.Count == 0))
                {
                    if (GUILayout.Button(T("exportAll", "Export All"), EditorStyles.toolbarButton, GUILayout.Width(78f)))
                    {
                        ExportAllVariations();
                    }
                }

                using (new EditorGUI.DisabledScope(false))
                {
                    if (GUILayout.Button(T("saveSession", "Save Session"), EditorStyles.toolbarButton, GUILayout.Width(104f)))
                    {
                        SaveSession();
                    }

                    if (GUILayout.Button(T("loadSession", "Load Session"), EditorStyles.toolbarButton, GUILayout.Width(104f)))
                    {
                        LoadSession();
                    }
                }

                using (new EditorGUI.DisabledScope(session.colorGroups.Count == 0))
                {
                    if (GUILayout.Button(T("exportPreset", "Export Preset"), EditorStyles.toolbarButton, GUILayout.Width(104f)))
                    {
                        ExportRulePreset();
                    }

                    if (GUILayout.Button(T("importPreset", "Import Preset"), EditorStyles.toolbarButton, GUILayout.Width(104f)))
                    {
                        ImportRulePreset();
                    }
                }

                using (new EditorGUI.DisabledScope(readableSourceImage == null || session.colorGroups.Count == 0))
                {
                    if (GUILayout.Button(T("preview", "Preview"), EditorStyles.toolbarButton, GUILayout.Width(78f)))
                    {
                        CancelScheduledAutoPreview();
                        RefreshAfterPreview();
                    }
                }

                bool nextAutoPreview = GUILayout.Toggle(
                    autoPreviewEnabled,
                    T("autoPreview", "Auto Preview"),
                    EditorStyles.toolbarButton,
                    GUILayout.Width(104f));
                if (nextAutoPreview != autoPreviewEnabled)
                {
                    autoPreviewEnabled = nextAutoPreview;
                    EditorPrefs.SetBool(AutoPreviewPrefsKey, autoPreviewEnabled);
                    if (!autoPreviewEnabled)
                    {
                        CancelScheduledAutoPreview();
                    }
                }

                if (GUILayout.Button(T("help", "Help"), EditorStyles.toolbarButton, GUILayout.Width(58f)))
                {
                    OpenHelpWindow();
                }

                DrawLanguagePopup();
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(sourceAssetPath, EditorStyles.miniLabel, GUILayout.MinWidth(160f));
            }
        }

        private void DrawLeftPane()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(LeftPaneWidth)))
            {
                leftScroll = EditorGUILayout.BeginScrollView(leftScroll);
                DrawSectionHeader(T("analyzeSettings", "Analyze Settings"));
                session.analyzeSettings.alphaThreshold = EditorGUILayout.IntSlider(T("alphaThreshold", "Alpha Threshold"), session.analyzeSettings.alphaThreshold, 0, 255);
                session.analyzeSettings.minimumPixelCount = Mathf.Max(1, EditorGUILayout.IntField(T("minimumPixelCount", "Minimum Pixel Count"), session.analyzeSettings.minimumPixelCount));
                session.analyzeSettings.quantizeStep = EditorGUILayout.IntSlider(T("quantizeStep", "Quantize Step"), session.analyzeSettings.quantizeStep, 1, 64);
                session.analyzeSettings.maxPaletteColors = Mathf.Max(1, EditorGUILayout.IntField(T("maxPaletteColors", "Max Palette Colors"), session.analyzeSettings.maxPaletteColors));

                DrawSectionSeparator();
                DrawSectionHeader(T("groupSettings", "Group Settings"));
                session.groupSettings.targetGroupCount = EditorGUILayout.IntSlider(T("targetGroupCount", "Target Group Count"), session.groupSettings.targetGroupCount, 1, 64);
                session.groupSettings.distanceMode = (ColorDistanceMode)EditorGUILayout.EnumPopup(T("distanceMode", "Distance Mode"), session.groupSettings.distanceMode);
                EditorGUILayout.HelpBox(T("distanceModeHint", "RGB is fast and stable. HSV keeps hue relationships. Lab groups colors closer to human perception."), MessageType.None);
                session.groupSettings.maxColorDistance = EditorGUILayout.Slider(T("maxColorDistance", "Max Color Distance"), session.groupSettings.maxColorDistance, 0f, 441f);
                session.groupSettings.preserveDarkOutline = EditorGUILayout.Toggle(T("preserveDarkOutline", "Preserve Dark Outline"), session.groupSettings.preserveDarkOutline);
                session.groupSettings.preserveAlpha = EditorGUILayout.Toggle(T("preserveAlpha", "Preserve Alpha"), session.groupSettings.preserveAlpha);

                DrawSectionSeparator();
                DrawSectionHeader(T("exportSettings", "Export Settings"));
                using (new EditorGUILayout.HorizontalScope())
                {
                    session.exportSettings.outputFolder = EditorGUILayout.TextField(T("outputFolder", "Output Folder"), session.exportSettings.outputFolder);
                    if (GUILayout.Button("...", GUILayout.Width(28f)))
                    {
                        SelectOutputFolder();
                    }
                }

                session.exportSettings.filePrefix = EditorGUILayout.TextField(T("filePrefix", "File Prefix"), session.exportSettings.filePrefix);
                using (var change = new EditorGUI.ChangeCheckScope())
                {
                    session.exportSettings.fileSuffix = EditorGUILayout.TextField(T("fileSuffix", "File Suffix"), session.exportSettings.fileSuffix);
                    if (change.changed)
                    {
                        IconVariation activeVariation = variationService.GetActiveVariation(session);
                        if (activeVariation != null)
                        {
                            activeVariation.fileSuffix = session.exportSettings.fileSuffix;
                        }
                    }
                }
                session.exportSettings.conflictMode = (ExportConflictMode)EditorGUILayout.EnumPopup(T("conflictMode", "Conflict Mode"), session.exportSettings.conflictMode);
                session.exportSettings.refreshAssetDatabase = EditorGUILayout.Toggle(T("refreshAssetDatabase", "Refresh AssetDatabase"), session.exportSettings.refreshAssetDatabase);

                DrawSectionSeparator();
                DrawSectionHeader(T("sourceInfo", "Source Info"));
                if (readableSourceImage != null)
                {
                    EditorGUILayout.LabelField(T("assetPath", "Asset Path"), sourceAssetPath);
                    EditorGUILayout.LabelField(T("size", "Size"), $"{readableSourceImage.width} x {readableSourceImage.height}");
                    EditorGUILayout.LabelField(T("paletteColors", "Palette Colors"), session.paletteColors.Count.ToString());
                    EditorGUILayout.LabelField(T("groups", "Groups"), session.colorGroups.Count.ToString());
                }
                else
                {
                    EditorGUILayout.HelpBox(T("noAnalyzedImage", "No source image has been analyzed yet."), MessageType.None);
                }

                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawCenterPane()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.MinWidth(360f), GUILayout.ExpandWidth(true)))
            {
                DrawSectionHeader(T("preview", "Preview"));
                DrawPreviewControls();
                if (previewCompareMode == PreviewCompareMode.Split)
                {
                    DrawSplitPreviewPanel();
                }
                else
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        DrawPreviewPanel(T("before", "Before"), readableSourceImage);
                        DrawPreviewPanel(T("after", "After"), afterPreview);
                    }
                }

                DrawSectionSeparator();
                DrawSectionHeader(T("palette", "Palette"));
                if (session.paletteColors.Count == 0)
                {
                    EditorGUILayout.HelpBox(T("paletteEmpty", "Palette colors will appear here after analysis."), MessageType.None);
                    return;
                }

                DrawPaletteList(session.paletteColors);
            }
        }

        private void DrawPreviewControls()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                previewCompareMode = (PreviewCompareMode)EditorGUILayout.EnumPopup(
                    T("compareMode", "Compare"),
                    previewCompareMode,
                    GUILayout.MaxWidth(220f));

                EditorGUI.BeginChangeCheck();
                previewZoom = EditorGUILayout.Slider(T("zoom", "Zoom"), previewZoom, MinPreviewZoom, MaxPreviewZoom);
                if (EditorGUI.EndChangeCheck())
                {
                    previewZoom = Mathf.Clamp(previewZoom, MinPreviewZoom, MaxPreviewZoom);
                    ClampPreviewPan();
                }

                if (GUILayout.Button(T("resetView", "Reset View"), GUILayout.Width(92f)))
                {
                    ResetPreviewView();
                }
            }

            if (previewCompareMode == PreviewCompareMode.Split)
            {
                previewSplit = EditorGUILayout.Slider(T("split", "Split"), previewSplit, 0.05f, 0.95f);
            }
        }

        private void DrawRightPane()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(RightPaneWidth)))
            {
                rightScroll = EditorGUILayout.BeginScrollView(rightScroll);
                DrawSectionHeader(T("variations", "Variations"));
                DrawVariationList();
                DrawSectionSeparator();
                DrawSectionHeader(T("replacementRules", "Replacement Rules"));
                if (session.colorGroups.Count == 0)
                {
                    EditorGUILayout.HelpBox(T("groupsEmpty", "Color groups will appear here after Auto Group."), MessageType.None);
                    EditorGUILayout.EndScrollView();
                    return;
                }

                DrawGroupList(session.colorGroups);
                DrawSelectedColorRules();
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawPreviewPanel(string title, Texture2D texture)
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
            {
                EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
                Rect previewRect = GUILayoutUtility.GetRect(10f, 10000f, MinPreviewHeight, MinPreviewHeight, GUILayout.ExpandWidth(true));
                EditorGUI.DrawRect(previewRect, new Color(0.13f, 0.13f, 0.13f));
                DrawCheckerboard(previewRect);
                HandlePreviewPan(previewRect);

                if (texture == null)
                {
                    DrawCenteredLabel(previewRect, T("noPreview", "No preview"));
                    return;
                }

                Rect imageRect = FitRect(previewRect, texture.width, texture.height);
                Rect texCoords = GetPreviewTexCoords();
                GUI.DrawTextureWithTexCoords(imageRect, texture, texCoords, true);
                DrawSelectionOverlay(imageRect, texCoords);
            }
        }

        private void DrawSplitPreviewPanel()
        {
            Rect previewRect = GUILayoutUtility.GetRect(10f, 10000f, MinPreviewHeight, MinPreviewHeight, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(previewRect, new Color(0.13f, 0.13f, 0.13f));
            DrawCheckerboard(previewRect);
            HandlePreviewPan(previewRect);

            Texture2D basisTexture = readableSourceImage != null ? readableSourceImage : afterPreview;
            if (basisTexture == null)
            {
                DrawCenteredLabel(previewRect, T("noPreview", "No preview"));
                return;
            }

            Rect imageRect = FitRect(previewRect, basisTexture.width, basisTexture.height);
            Rect texCoords = GetPreviewTexCoords();
            float splitX = Mathf.Lerp(imageRect.x, imageRect.xMax, previewSplit);

            DrawSplitTexture(readableSourceImage, imageRect, texCoords, new Rect(imageRect.x, imageRect.y, splitX - imageRect.x, imageRect.height));
            DrawSplitTexture(afterPreview, imageRect, texCoords, new Rect(splitX, imageRect.y, imageRect.xMax - splitX, imageRect.height));
            DrawSelectionOverlay(imageRect, texCoords);

            EditorGUI.DrawRect(new Rect(splitX - 1f, imageRect.y, 2f, imageRect.height), Color.white);
            GUI.Label(new Rect(imageRect.x + 6f, imageRect.y + 4f, 80f, 18f), T("before", "Before"), EditorStyles.miniBoldLabel);
            GUI.Label(new Rect(imageRect.xMax - 58f, imageRect.y + 4f, 54f, 18f), T("after", "After"), EditorStyles.miniBoldLabel);
        }

        private static void DrawSplitTexture(Texture2D texture, Rect imageRect, Rect texCoords, Rect visibleRect)
        {
            if (texture == null || visibleRect.width <= 0f || visibleRect.height <= 0f)
            {
                return;
            }

            Rect clippedRect = Rect.MinMaxRect(
                Mathf.Max(imageRect.x, visibleRect.x),
                Mathf.Max(imageRect.y, visibleRect.y),
                Mathf.Min(imageRect.xMax, visibleRect.xMax),
                Mathf.Min(imageRect.yMax, visibleRect.yMax));

            if (clippedRect.width <= 0f || clippedRect.height <= 0f)
            {
                return;
            }

            float xMin = Mathf.InverseLerp(imageRect.x, imageRect.xMax, clippedRect.x);
            float xMax = Mathf.InverseLerp(imageRect.x, imageRect.xMax, clippedRect.xMax);
            float yMin = Mathf.InverseLerp(imageRect.y, imageRect.yMax, clippedRect.y);
            float yMax = Mathf.InverseLerp(imageRect.y, imageRect.yMax, clippedRect.yMax);
            Rect clippedTexCoords = Rect.MinMaxRect(
                Mathf.Lerp(texCoords.xMin, texCoords.xMax, xMin),
                Mathf.Lerp(texCoords.yMin, texCoords.yMax, yMin),
                Mathf.Lerp(texCoords.xMin, texCoords.xMax, xMax),
                Mathf.Lerp(texCoords.yMin, texCoords.yMax, yMax));

            GUI.DrawTextureWithTexCoords(clippedRect, texture, clippedTexCoords, true);
        }

        private void DrawPaletteList(IReadOnlyList<PaletteColorEntry> paletteColors)
        {
            paletteScroll = EditorGUILayout.BeginScrollView(
                paletteScroll,
                false,
                true,
                GUIStyle.none,
                GUI.skin.verticalScrollbar,
                GUI.skin.box,
                GUILayout.Height(PaletteListHeight),
                GUILayout.ExpandWidth(true));

            using (new EditorGUILayout.VerticalScope())
            {
                foreach (PaletteColorEntry entry in paletteColors)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        Rect swatchRect = GUILayoutUtility.GetRect(22f, 18f, GUILayout.Width(22f));
                        EditorGUI.DrawRect(swatchRect, entry.color);
                        bool isSelected = selectedColorEntryId == entry.id;
                        bool nextSelected = GUILayout.Toggle(isSelected, string.Empty, GUILayout.Width(18f));
                        if (nextSelected && !isSelected)
                        {
                            selectedColorEntryId = entry.id;
                            selectedGroupId = entry.groupId;
                            InvalidateSelectionOverlay();
                            Repaint();
                        }

                        EditorGUILayout.LabelField(entry.hex, EditorStyles.boldLabel, GUILayout.Width(78f));
                        EditorGUILayout.LabelField($"{entry.pixelCount} px", GUILayout.Width(58f));
                        EditorGUILayout.LabelField($"{entry.pixelRatio:P1}", GUILayout.Width(58f));
                        DrawPaletteGroupPopup(entry);
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawPaletteGroupPopup(PaletteColorEntry entry)
        {
            if (session.colorGroups.Count == 0)
            {
                EditorGUILayout.LabelField(entry.groupId, EditorStyles.miniLabel, GUILayout.MinWidth(64f), GUILayout.ExpandWidth(true));
                return;
            }

            string[] groupNames = session.colorGroups
                .Select(group => group == null ? string.Empty : $"{group.displayName} ({group.id})")
                .ToArray();
            int currentIndex = Mathf.Max(0, session.colorGroups.FindIndex(group => group != null && group.id == entry.groupId));
            ColorGroup sourceGroup = session.colorGroups.FirstOrDefault(group => group != null && group.id == entry.groupId);
            using (new EditorGUI.DisabledScope(sourceGroup != null && sourceGroup.lockedGroup))
            {
                int nextIndex = EditorGUILayout.Popup(currentIndex, groupNames, GUILayout.MinWidth(132f), GUILayout.ExpandWidth(true));
                if (nextIndex != currentIndex && nextIndex >= 0 && nextIndex < session.colorGroups.Count)
                {
                    MovePaletteEntryToGroup(entry, session.colorGroups[nextIndex].id);
                }
            }
        }

        private void DrawGroupList(IReadOnlyList<ColorGroup> colorGroups)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                foreach (ColorGroup group in colorGroups)
                {
                    DrawGroupRule(group);
                }
            }
        }

        private void DrawGroupRule(ColorGroup group)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    Rect representativeRect = GUILayoutUtility.GetRect(20f, 18f, GUILayout.Width(20f));
                    EditorGUI.DrawRect(representativeRect, group.representativeColor);
                    bool isSelected = selectedGroupId == group.id && string.IsNullOrEmpty(selectedColorEntryId);
                    bool nextSelected = GUILayout.Toggle(isSelected, string.Empty, GUILayout.Width(18f));
                    if (nextSelected && !isSelected)
                    {
                        selectedGroupId = group.id;
                        selectedColorEntryId = string.Empty;
                        InvalidateSelectionOverlay();
                        Repaint();
                    }

                    EditorGUILayout.LabelField(group.displayName, EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"{group.colorEntryIds.Count} colors", GUILayout.Width(76f));
                    EditorGUILayout.LabelField($"{group.pixelRatio:P1}", GUILayout.Width(56f));
                    group.lockedGroup = GUILayout.Toggle(group.lockedGroup, T("locked", "Locked"), EditorStyles.miniButton, GUILayout.Width(62f));
                }

                float previousLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 84f;
                using (var change = new EditorGUI.ChangeCheckScope())
                {
                    group.targetColor = EditorGUILayout.ColorField(T("targetColor", "Target Color"), group.targetColor);
                    group.blendRatio = EditorGUILayout.Slider(T("blendRatio", "Blend Ratio"), group.blendRatio, 0f, 1f);
                    group.replacementMode = (ColorReplacementMode)EditorGUILayout.EnumPopup(T("mode", "Mode"), group.replacementMode);
                    if (change.changed && afterPreview != null)
                    {
                        HandlePreviewSettingChanged();
                    }
                }

                EditorGUIUtility.labelWidth = previousLabelWidth;
            }
        }

        private void DrawSelectedColorRules()
        {
            if (string.IsNullOrEmpty(selectedGroupId))
            {
                return;
            }

            ColorGroup selectedGroup = session.colorGroups.FirstOrDefault(group => group.id == selectedGroupId);
            if (selectedGroup == null)
            {
                return;
            }

            DrawSectionSeparator();
            DrawSectionHeader(T("colorRules", "Color Rules"));
            foreach (PaletteColorEntry entry in session.paletteColors.Where(color => color.groupId == selectedGroupId))
            {
                ColorReplacementRule rule = GetOrCreateColorRule(entry);
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        Rect swatchRect = GUILayoutUtility.GetRect(20f, 18f, GUILayout.Width(20f));
                        EditorGUI.DrawRect(swatchRect, entry.color);
                        bool isSelected = selectedColorEntryId == entry.id;
                        bool nextSelected = GUILayout.Toggle(isSelected, string.Empty, GUILayout.Width(18f));
                        if (nextSelected && !isSelected)
                        {
                            selectedColorEntryId = entry.id;
                            selectedGroupId = entry.groupId;
                            InvalidateSelectionOverlay();
                            Repaint();
                        }

                        EditorGUILayout.LabelField(entry.hex, EditorStyles.boldLabel);
                        using (var change = new EditorGUI.ChangeCheckScope())
                        {
                            rule.enabled = EditorGUILayout.ToggleLeft(T("enabled", "Enabled"), rule.enabled, GUILayout.Width(84f));
                            if (change.changed && afterPreview != null)
                            {
                                HandlePreviewSettingChanged();
                            }
                        }
                    }

                    float previousLabelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 84f;
                    using (var change = new EditorGUI.ChangeCheckScope())
                    {
                        rule.targetColor = EditorGUILayout.ColorField(T("targetColor", "Target Color"), rule.targetColor);
                        rule.blendRatio = EditorGUILayout.Slider(T("blendRatio", "Blend Ratio"), rule.blendRatio, 0f, 1f);
                        if (change.changed && afterPreview != null)
                        {
                            HandlePreviewSettingChanged();
                        }
                    }

                    EditorGUIUtility.labelWidth = previousLabelWidth;
                }
            }
        }

        private void AnalyzeSourceImage()
        {
            if (!textureAssetLoader.TryLoadReadableTexture(sourceImage, out Texture2D loadedTexture, out string assetPath, out string error))
            {
                reportMessage = error;
                return;
            }

            DestroyReadableSourceImage();
            readableSourceImage = loadedTexture;
            sourceAssetPath = assetPath;
            session.sourceImageAssetPath = assetPath;
            session.paletteColors = new List<PaletteColorEntry>(colorExtractionService.Extract(readableSourceImage, session.analyzeSettings));
            session.colorGroups.Clear();
            session.colorRules.Clear();
            session.variations.Clear();
            session.activeVariationId = string.Empty;
            selectedGroupId = string.Empty;
            selectedColorEntryId = string.Empty;
            InvalidateSelectionOverlay();
            DestroyAfterPreview();
            reportMessage = $"Analyzed {session.paletteColors.Count} palette colors from {assetPath}.";
            reportType = MessageType.Info;
        }

        private void AutoGroupPalette()
        {
            session.colorGroups = new List<ColorGroup>(colorGroupingService.CreateGroups(session.paletteColors, session.groupSettings));
            session.colorRules.Clear();
            session.variations.Clear();
            session.activeVariationId = string.Empty;
            variationService.EnsureActiveVariation(session);
            variationService.SyncActiveVariation(session);
            selectedGroupId = session.colorGroups.Count > 0 ? session.colorGroups[0].id : string.Empty;
            selectedColorEntryId = string.Empty;
            InvalidateSelectionOverlay();
            reportMessage = $"Created {session.colorGroups.Count} color groups.";
            reportType = session.colorGroups.Count == 0 ? MessageType.Warning : MessageType.Info;
            RefreshAfterPreview();
        }

        private void RefreshAfterPreview()
        {
            if (readableSourceImage == null)
            {
                reportMessage = "Analyze a source image before preview.";
                reportType = MessageType.Warning;
                return;
            }

            if (session.colorGroups.Count == 0)
            {
                reportMessage = "Create color groups before preview.";
                reportType = MessageType.Warning;
                return;
            }

            DestroyAfterPreview();
            afterPreview = colorReplacementService.Apply(readableSourceImage, session);
            reportMessage = "After preview updated.";
            reportType = MessageType.Info;
        }

        private void HandlePreviewSettingChanged()
        {
            variationService.SyncActiveVariation(session);
            if (autoPreviewEnabled)
            {
                ScheduleAutoPreviewUpdate();
                return;
            }

            CancelScheduledAutoPreview();
            reportMessage = "Preview settings changed. Click Preview to update.";
            reportType = MessageType.Info;
        }

        private void ScheduleAutoPreviewUpdate()
        {
            if (!CanAutoPreview())
            {
                return;
            }

            double debounceSeconds = GetAutoPreviewDebounceSeconds(readableSourceImage);
            autoPreviewPending = true;
            autoPreviewScheduledTime = EditorApplication.timeSinceStartup + debounceSeconds;
            EditorApplication.update -= ProcessScheduledAutoPreview;
            EditorApplication.update += ProcessScheduledAutoPreview;

            if (IsLargeAutoPreviewSource(readableSourceImage))
            {
                reportMessage = "Large image detected. Auto Preview will update with a longer delay.";
                reportType = MessageType.Warning;
            }
        }

        private bool CanAutoPreview()
        {
            return autoPreviewEnabled
                && readableSourceImage != null
                && session.colorGroups.Count > 0;
        }

        private void ProcessScheduledAutoPreview()
        {
            if (!autoPreviewPending || EditorApplication.timeSinceStartup < autoPreviewScheduledTime)
            {
                return;
            }

            CancelScheduledAutoPreview();
            RefreshAfterPreview();
            Repaint();
        }

        private void CancelScheduledAutoPreview()
        {
            autoPreviewPending = false;
            EditorApplication.update -= ProcessScheduledAutoPreview;
        }

        internal static double GetAutoPreviewDebounceSeconds(Texture2D texture)
        {
            return IsLargeAutoPreviewSource(texture)
                ? LargeImageAutoPreviewDebounceSeconds
                : AutoPreviewDebounceSeconds;
        }

        internal static Rect GetPreviewTexCoords(float zoom, Vector2 pan)
        {
            float safeZoom = Mathf.Clamp(zoom, MinPreviewZoom, MaxPreviewZoom);
            float size = 1f / safeZoom;
            float maxOffset = (1f - size) * 0.5f;
            float centerX = 0.5f + Mathf.Clamp(pan.x, -maxOffset, maxOffset);
            float centerY = 0.5f + Mathf.Clamp(pan.y, -maxOffset, maxOffset);
            return Rect.MinMaxRect(
                centerX - (size * 0.5f),
                centerY - (size * 0.5f),
                centerX + (size * 0.5f),
                centerY + (size * 0.5f));
        }

        private static bool IsLargeAutoPreviewSource(Texture2D texture)
        {
            return texture != null && texture.width * texture.height >= LargeImageAutoPreviewPixelCount;
        }

        private Rect GetPreviewTexCoords()
        {
            return GetPreviewTexCoords(previewZoom, previewPan);
        }

        private void HandlePreviewPan(Rect previewRect)
        {
            Event current = Event.current;
            if (previewZoom <= MinPreviewZoom || !previewRect.Contains(current.mousePosition))
            {
                return;
            }

            if (current.type == EventType.MouseDrag && current.button == 0)
            {
                previewPan += new Vector2(
                    -current.delta.x / previewRect.width / previewZoom,
                    -current.delta.y / previewRect.height / previewZoom);
                ClampPreviewPan();
                current.Use();
                Repaint();
            }
        }

        private void ClampPreviewPan()
        {
            float safeZoom = Mathf.Clamp(previewZoom, MinPreviewZoom, MaxPreviewZoom);
            float size = 1f / safeZoom;
            float maxOffset = (1f - size) * 0.5f;
            previewPan = new Vector2(
                Mathf.Clamp(previewPan.x, -maxOffset, maxOffset),
                Mathf.Clamp(previewPan.y, -maxOffset, maxOffset));
        }

        private void ResetPreviewView()
        {
            previewZoom = MinPreviewZoom;
            previewPan = Vector2.zero;
            previewSplit = 0.5f;
        }

        private void ExportPreview()
        {
            if (afterPreview == null)
            {
                reportMessage = "Preview is required before export.";
                reportType = MessageType.Warning;
                return;
            }

            variationService.SyncActiveVariation(session);
            PngExportResult result = pngExportService.Export(afterPreview, session.exportSettings, GetProjectRoot());
            if (session.exportSettings.refreshAssetDatabase)
            {
                AssetDatabase.Refresh();
            }

            if (result.Status == PngExportStatus.Exported)
            {
                SelectOutputAsset(result.OutputPath);
                reportMessage = result.Message;
                reportType = MessageType.Info;
            }
            else if (result.Status == PngExportStatus.Skipped)
            {
                reportMessage = result.Message;
                reportType = MessageType.Warning;
            }
            else
            {
                reportMessage = result.Message;
                reportType = MessageType.Error;
            }
        }

        private void ExportAllVariations()
        {
            if (readableSourceImage == null)
            {
                reportMessage = "Analyze a source image before batch export.";
                reportType = MessageType.Warning;
                return;
            }

            if (session.variations.Count == 0)
            {
                reportMessage = "Create at least one variation before batch export.";
                reportType = MessageType.Warning;
                return;
            }

            variationService.SyncActiveVariation(session);
            string originalVariationId = session.activeVariationId;
            int exported = 0;
            int skipped = 0;
            List<string> failures = new List<string>();

            foreach (IconVariation variation in session.variations.Where(variation => variation != null && variation.exportEnabled))
            {
                variationService.ApplyVariation(session, variation.id);
                Texture2D preview = colorReplacementService.Apply(readableSourceImage, session);
                ExportSettings exportSettings = CreateExportSettingsForVariation(variation);
                PngExportResult result = pngExportService.Export(preview, exportSettings, GetProjectRoot());
                DestroyImmediate(preview);

                if (result.Status == PngExportStatus.Exported)
                {
                    exported++;
                }
                else if (result.Status == PngExportStatus.Skipped)
                {
                    skipped++;
                }
                else
                {
                    failures.Add(result.Message);
                }
            }

            variationService.ApplyVariation(session, originalVariationId);
            RefreshAfterPreview();
            if (session.exportSettings.refreshAssetDatabase)
            {
                AssetDatabase.Refresh();
            }

            if (failures.Count > 0)
            {
                reportMessage = $"Batch export completed with {failures.Count} failure(s): {string.Join("; ", failures)}";
                reportType = MessageType.Error;
            }
            else
            {
                reportMessage = $"Batch export completed. Exported: {exported}, Skipped: {skipped}.";
                reportType = skipped > 0 ? MessageType.Warning : MessageType.Info;
            }
        }

        private void SaveSession()
        {
            string path = EditorUtility.SaveFilePanel(
                "Save Palette Variant Session",
                GetProjectRoot(),
                "palette-variant-session.json",
                "json");

            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            try
            {
                variationService.SyncActiveVariation(session);
                sessionJsonService.Save(path, session);
                reportMessage = $"Session saved: {path}";
                reportType = MessageType.Info;
            }
            catch (System.Exception ex)
            {
                reportMessage = ex.Message;
                reportType = MessageType.Error;
            }
        }

        private void LoadSession()
        {
            string path = EditorUtility.OpenFilePanel("Load Palette Variant Session", GetProjectRoot(), "json");
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            SessionLoadResult result = sessionJsonService.Load(path);
            if (!result.Success)
            {
                reportMessage = string.Join("\n", result.Warnings);
                reportType = MessageType.Error;
                return;
            }

            session = result.Session;
            session.colorRules ??= new List<ColorReplacementRule>();
            session.variations ??= new List<IconVariation>();
            if (session.variations.Count > 0)
            {
                variationService.ApplyVariation(session, string.IsNullOrWhiteSpace(session.activeVariationId)
                    ? session.variations[0].id
                    : session.activeVariationId);
            }
            else if (session.colorGroups.Count > 0)
            {
                variationService.EnsureActiveVariation(session);
                variationService.SyncActiveVariation(session);
            }
            sourceAssetPath = session.sourceImageAssetPath;
            sourceImage = string.IsNullOrWhiteSpace(sourceAssetPath)
                ? null
                : AssetDatabase.LoadAssetAtPath<Texture2D>(sourceAssetPath);
            selectedGroupId = session.colorGroups.Count > 0 ? session.colorGroups[0].id : string.Empty;
            selectedColorEntryId = string.Empty;
            InvalidateSelectionOverlay();

            DestroyReadableSourceImage();
            DestroyAfterPreview();
            string loadError = string.Empty;
            if (sourceImage != null && textureAssetLoader.TryLoadReadableTexture(sourceImage, out Texture2D loadedTexture, out _, out loadError))
            {
                readableSourceImage = loadedTexture;
                if (session.colorGroups.Count > 0)
                {
                    RefreshAfterPreview();
                }
            }
            else if (sourceImage != null)
            {
                reportMessage = loadError;
                reportType = MessageType.Warning;
                return;
            }

            reportMessage = result.Warnings.Count == 0
                ? $"Session loaded: {path}"
                : string.Join("\n", result.Warnings);
            reportType = result.Warnings.Count == 0 ? MessageType.Info : MessageType.Warning;
        }

        internal void SaveSessionForValidation(string path)
        {
            sessionJsonService.Save(path, session);
        }

        private void ExportRulePreset()
        {
            string path = EditorUtility.SaveFilePanel(
                "Export Rule Preset",
                GetProjectRoot(),
                "palette-rule-preset.json",
                "json");

            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            try
            {
                variationService.SyncActiveVariation(session);
                PaletteVariantRulePreset preset = rulePresetJsonService.CreatePreset(session, "Rule Preset");
                rulePresetJsonService.Save(path, preset);
                reportMessage = $"Rule preset exported: {path}";
                reportType = MessageType.Info;
            }
            catch (System.Exception ex)
            {
                reportMessage = ex.Message;
                reportType = MessageType.Error;
            }
        }

        private void ImportRulePreset()
        {
            string path = EditorUtility.OpenFilePanel("Import Rule Preset", GetProjectRoot(), "json");
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            RulePresetLoadResult result = rulePresetJsonService.Load(path);
            if (!result.Success)
            {
                reportMessage = string.Join("\n", result.Warnings);
                reportType = MessageType.Error;
                return;
            }

            IReadOnlyList<string> warnings = rulePresetJsonService.ApplyToSession(result.Preset, session);
            variationService.SyncActiveVariation(session);
            if (afterPreview != null)
            {
                RefreshAfterPreview();
            }

            reportMessage = warnings.Count == 0
                ? $"Rule preset imported: {path}"
                : $"Rule preset imported with warnings:\n{string.Join("\n", warnings)}";
            reportType = warnings.Count == 0 ? MessageType.Info : MessageType.Warning;
        }

        private void SelectOutputFolder()
        {
            string currentFolder = ResolveOutputFolderForPanel(session.exportSettings.outputFolder);
            string selectedFolder = EditorUtility.OpenFolderPanel("Output Folder", currentFolder, string.Empty);
            if (string.IsNullOrWhiteSpace(selectedFolder))
            {
                return;
            }

            session.exportSettings.outputFolder = ToProjectRelativePath(selectedFolder);
        }

        private void DrawVariationList()
        {
            if (session.colorGroups.Count == 0 && session.variations.Count == 0)
            {
                EditorGUILayout.HelpBox(T("variationsEmpty", "Variations will appear after Auto Group."), MessageType.None);
                return;
            }

            variationService.EnsureActiveVariation(session);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(T("add", "Add"), GUILayout.Width(58f)))
                {
                    variationService.AddVariation(session);
                    RefreshAfterPreview();
                }

                if (GUILayout.Button(T("duplicate", "Duplicate"), GUILayout.Width(82f)))
                {
                    variationService.DuplicateActiveVariation(session);
                    RefreshAfterPreview();
                }

                using (new EditorGUI.DisabledScope(session.variations.Count <= 1))
                {
                    if (GUILayout.Button(T("remove", "Remove"), GUILayout.Width(72f)))
                    {
                        variationService.RemoveActiveVariation(session);
                        RefreshAfterPreview();
                    }
                }
            }

            variationScroll = EditorGUILayout.BeginScrollView(
                variationScroll,
                false,
                true,
                GUIStyle.none,
                GUI.skin.verticalScrollbar,
                GUI.skin.box,
                GUILayout.Height(VariationListHeight));

            foreach (IconVariation variation in session.variations)
            {
                if (variation == null)
                {
                    continue;
                }

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        bool isActive = variation.id == session.activeVariationId;
                        GUIStyle activeStyle = isActive ? EditorStyles.miniButtonMid : EditorStyles.miniButton;
                        string activeLabel = isActive ? T("activeVariation", "Active") : T("useVariation", "Use");
                        using (new EditorGUI.DisabledScope(isActive))
                        {
                            if (GUILayout.Button(activeLabel, activeStyle, GUILayout.Width(58f)))
                            {
                                variationService.SyncActiveVariation(session);
                                variationService.ApplyVariation(session, variation.id);
                                RefreshAfterPreview();
                            }
                        }

                        using (var change = new EditorGUI.ChangeCheckScope())
                        {
                            string exportLabel = variation.exportEnabled ? T("export", "Export") : T("skip", "Skip");
                            variation.exportEnabled = GUILayout.Toggle(
                                variation.exportEnabled,
                                exportLabel,
                                EditorStyles.miniButton,
                                GUILayout.Width(62f));

                            variation.displayName = EditorGUILayout.TextField(variation.displayName, GUILayout.MinWidth(120f), GUILayout.ExpandWidth(true));
                            if (change.changed)
                            {
                                variationService.SyncActiveVariation(session);
                            }
                        }
                    }

                    float previousLabelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 78f;
                    using (var change = new EditorGUI.ChangeCheckScope())
                    {
                        variation.fileSuffix = EditorGUILayout.TextField(T("fileSuffix", "File Suffix"), variation.fileSuffix);
                        if (variation.id == session.activeVariationId)
                        {
                            session.exportSettings.fileSuffix = variation.fileSuffix;
                        }

                        if (change.changed)
                        {
                            variationService.SyncActiveVariation(session);
                        }
                    }

                    EditorGUILayout.LabelField(
                        T("outputFile", "Output File"),
                        BuildVariationOutputFileName(session.exportSettings, variation),
                        EditorStyles.miniLabel);

                    EditorGUIUtility.labelWidth = previousLabelWidth;
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private ExportSettings CreateExportSettingsForVariation(IconVariation variation)
        {
            return new ExportSettings
            {
                outputFolder = session.exportSettings.outputFolder,
                filePrefix = session.exportSettings.filePrefix,
                fileSuffix = string.IsNullOrWhiteSpace(variation.fileSuffix) ? variation.id : variation.fileSuffix,
                conflictMode = session.exportSettings.conflictMode,
                refreshAssetDatabase = false
            };
        }

        internal static string BuildVariationOutputFileName(ExportSettings settings, IconVariation variation)
        {
            string prefix = settings == null || string.IsNullOrWhiteSpace(settings.filePrefix)
                ? "icon"
                : settings.filePrefix.Trim();
            string suffix = variation == null || string.IsNullOrWhiteSpace(variation.fileSuffix)
                ? variation?.id ?? "variant"
                : variation.fileSuffix.Trim();
            string fileName = $"{prefix}_{suffix}";
            return fileName.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase)
                ? fileName
                : fileName + ".png";
        }

        private ColorReplacementRule GetOrCreateColorRule(PaletteColorEntry entry)
        {
            session.colorRules ??= new List<ColorReplacementRule>();
            ColorReplacementRule rule = session.colorRules.FirstOrDefault(candidate =>
                candidate != null
                && candidate.scope == ColorReplacementScope.ColorEntry
                && candidate.colorEntryId == entry.id);

            if (rule != null)
            {
                return rule;
            }

            rule = new ColorReplacementRule
            {
                id = $"rule_{entry.id}",
                groupId = entry.groupId,
                colorEntryId = entry.id,
                scope = ColorReplacementScope.ColorEntry,
                targetColor = entry.color,
                blendRatio = 1f,
                enabled = false
            };
            session.colorRules.Add(rule);
            return rule;
        }

        private void MovePaletteEntryToGroup(PaletteColorEntry entry, string targetGroupId)
        {
            if (entry == null || string.IsNullOrWhiteSpace(targetGroupId) || entry.groupId == targetGroupId)
            {
                return;
            }

            ColorGroup sourceGroup = session.colorGroups.FirstOrDefault(group => group != null && group.id == entry.groupId);
            ColorGroup targetGroup = session.colorGroups.FirstOrDefault(group => group != null && group.id == targetGroupId);
            if (targetGroup == null)
            {
                return;
            }

            if ((sourceGroup != null && sourceGroup.lockedGroup) || targetGroup.lockedGroup)
            {
                reportMessage = "Locked groups cannot be edited.";
                reportType = MessageType.Warning;
                return;
            }

            if (sourceGroup != null)
            {
                sourceGroup.colorEntryIds.Remove(entry.id);
            }

            if (!targetGroup.colorEntryIds.Contains(entry.id))
            {
                targetGroup.colorEntryIds.Add(entry.id);
            }

            entry.groupId = targetGroup.id;
            selectedGroupId = targetGroup.id;
            selectedColorEntryId = entry.id;
            RecalculateGroupStats();
            variationService.SyncActiveVariation(session);
            InvalidateSelectionOverlay();
            if (afterPreview != null)
            {
                RefreshAfterPreview();
            }

            reportMessage = $"Moved {entry.hex} to {targetGroup.displayName}.";
            reportType = MessageType.Info;
        }

        private void RecalculateGroupStats()
        {
            int totalPixels = session.paletteColors == null ? 0 : session.paletteColors.Sum(entry => entry == null ? 0 : entry.pixelCount);
            foreach (ColorGroup group in session.colorGroups)
            {
                if (group == null)
                {
                    continue;
                }

                List<PaletteColorEntry> entries = session.paletteColors
                    .Where(entry => entry != null && entry.groupId == group.id)
                    .ToList();
                group.colorEntryIds = entries.Select(entry => entry.id).ToList();
                group.pixelCount = entries.Sum(entry => entry.pixelCount);
                group.pixelRatio = totalPixels <= 0 ? 0f : (float)group.pixelCount / totalPixels;
                if (entries.Count > 0)
                {
                    PaletteColorEntry representative = entries.OrderByDescending(entry => entry.pixelCount).First();
                    group.representativeColor = representative.color;
                }
            }
        }

        private void DrawSelectionOverlay(Rect imageRect, Rect texCoords)
        {
            if (Event.current.type != EventType.Repaint || readableSourceImage == null)
            {
                return;
            }

            EnsureSelectionOverlayTexture();
            if (selectionOverlayTexture == null)
            {
                return;
            }

            GUI.DrawTextureWithTexCoords(imageRect, selectionOverlayTexture, texCoords, true);
            EditorGUI.DrawRect(new Rect(imageRect.x, imageRect.y, imageRect.width, 1f), OverlayBorderColor);
            EditorGUI.DrawRect(new Rect(imageRect.x, imageRect.yMax - 1f, imageRect.width, 1f), OverlayBorderColor);
            EditorGUI.DrawRect(new Rect(imageRect.x, imageRect.y, 1f, imageRect.height), OverlayBorderColor);
            EditorGUI.DrawRect(new Rect(imageRect.xMax - 1f, imageRect.y, 1f, imageRect.height), OverlayBorderColor);
        }

        private void EnsureSelectionOverlayTexture()
        {
            string cacheKey = BuildSelectionOverlayCacheKey();
            if (cacheKey == selectionOverlayCacheKey)
            {
                return;
            }

            selectionOverlayCacheKey = cacheKey;
            DestroySelectionOverlayTexture();
            if (string.IsNullOrEmpty(cacheKey))
            {
                return;
            }

            HashSet<uint> selectedKeys = GetSelectedColorKeys();
            if (selectedKeys.Count == 0)
            {
                return;
            }

            int quantizeStep = Mathf.Clamp(session.analyzeSettings.quantizeStep, 1, 64);
            int alphaThreshold = Mathf.Clamp(session.analyzeSettings.alphaThreshold, 0, 255);
            Color32[] sourcePixels = readableSourceImage.GetPixels32();
            Color32[] overlayPixels = new Color32[sourcePixels.Length];
            Color32 overlayColor = OverlayColor;

            for (int y = 0; y < readableSourceImage.height; y++)
            {
                int sourceY = readableSourceImage.height - 1 - y;
                for (int x = 0; x < readableSourceImage.width; x++)
                {
                    int sourceIndex = (sourceY * readableSourceImage.width) + x;
                    Color32 pixel = sourcePixels[sourceIndex];
                    if (pixel.a <= alphaThreshold)
                    {
                        continue;
                    }

                    uint key = ColorCodeUtility.ToRgbKey(colorQuantizationService.Quantize(pixel, quantizeStep));
                    if (!selectedKeys.Contains(key))
                    {
                        continue;
                    }

                    int overlayIndex = ((readableSourceImage.height - 1 - sourceY) * readableSourceImage.width) + x;
                    overlayPixels[overlayIndex] = overlayColor;
                }
            }

            selectionOverlayTexture = new Texture2D(readableSourceImage.width, readableSourceImage.height, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            selectionOverlayTexture.SetPixels32(overlayPixels);
            selectionOverlayTexture.Apply(false, true);
        }

        private string BuildSelectionOverlayCacheKey()
        {
            if (readableSourceImage == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(selectedColorEntryId) && string.IsNullOrEmpty(selectedGroupId))
            {
                return string.Empty;
            }

            return string.Join(
                "|",
                readableSourceImage.GetInstanceID().ToString(),
                readableSourceImage.width.ToString(),
                readableSourceImage.height.ToString(),
                session.analyzeSettings.alphaThreshold.ToString(),
                session.analyzeSettings.quantizeStep.ToString(),
                selectedGroupId,
                selectedColorEntryId,
                session.paletteColors.Count.ToString());
        }

        private void InvalidateSelectionOverlay()
        {
            selectionOverlayCacheKey = string.Empty;
        }

        private void DestroySelectionOverlayTexture()
        {
            if (selectionOverlayTexture == null)
            {
                return;
            }

            DestroyImmediate(selectionOverlayTexture);
            selectionOverlayTexture = null;
        }

        private HashSet<uint> GetSelectedColorKeys()
        {
            HashSet<uint> selectedKeys = new HashSet<uint>();
            foreach (PaletteColorEntry entry in session.paletteColors)
            {
                if (entry == null)
                {
                    continue;
                }

                bool selected = !string.IsNullOrEmpty(selectedColorEntryId)
                    ? entry.id == selectedColorEntryId
                    : !string.IsNullOrEmpty(selectedGroupId) && entry.groupId == selectedGroupId;

                if (selected)
                {
                    selectedKeys.Add(ColorCodeUtility.ToRgbKey(entry.color));
                }
            }

            return selectedKeys;
        }

        private void DrawLanguagePopup()
        {
            PaletteVariantLanguageMode nextMode = (PaletteVariantLanguageMode)EditorGUILayout.EnumPopup(
                languageMode,
                EditorStyles.toolbarPopup,
                GUILayout.Width(92f));

            if (nextMode == languageMode)
            {
                return;
            }

            languageMode = nextMode;
            displayLanguage = ResolveDisplayLanguage(languageMode);
            EditorPrefs.SetInt(LanguageModePrefsKey, (int)languageMode);
            Repaint();
            parameterHelpWindow?.Repaint();
        }

        private void OpenHelpWindow()
        {
            parameterHelpWindow = GetWindow<ParameterHelpWindow>("Palette Variant Help");
            parameterHelpWindow.SetOwner(this);
            parameterHelpWindow.minSize = new Vector2(420f, 360f);
            parameterHelpWindow.Show();
            parameterHelpWindow.Focus();
        }

        private void LoadLanguageMode()
        {
            int rawMode = EditorPrefs.GetInt(LanguageModePrefsKey, (int)PaletteVariantLanguageMode.Auto);
            languageMode = System.Enum.IsDefined(typeof(PaletteVariantLanguageMode), rawMode)
                ? (PaletteVariantLanguageMode)rawMode
                : PaletteVariantLanguageMode.Auto;
            displayLanguage = ResolveDisplayLanguage(languageMode);
        }

        private static PaletteVariantDisplayLanguage ResolveDisplayLanguage(PaletteVariantLanguageMode mode)
        {
            if (mode == PaletteVariantLanguageMode.Japanese)
            {
                return PaletteVariantDisplayLanguage.Japanese;
            }

            if (mode == PaletteVariantLanguageMode.English)
            {
                return PaletteVariantDisplayLanguage.English;
            }

            return Application.systemLanguage == SystemLanguage.Japanese
                ? PaletteVariantDisplayLanguage.Japanese
                : PaletteVariantDisplayLanguage.English;
        }

        internal string T(string key, string english)
        {
            if (displayLanguage != PaletteVariantDisplayLanguage.Japanese)
            {
                return english;
            }

            return key switch
            {
                "sourceImage" => "Source Image",
                "analyze" => "Analyze",
                "autoGroup" => "Auto Group",
                "export" => "Export",
                "exportAll" => "Export All",
                "saveSession" => "Save Session",
                "loadSession" => "Load Session",
                "exportPreset" => "Export Preset",
                "importPreset" => "Import Preset",
                "preview" => "Preview",
                "autoPreview" => "Auto Preview",
                "compareMode" => "比較",
                "zoom" => "ズーム",
                "resetView" => "表示リセット",
                "split" => "分割位置",
                "help" => "Help",
                "analyzeSettings" => "解析設定",
                "alphaThreshold" => "透明度しきい値",
                "minimumPixelCount" => "最小ピクセル数",
                "quantizeStep" => "量子化ステップ",
                "maxPaletteColors" => "最大パレット色数",
                "groupSettings" => "グループ設定",
                "targetGroupCount" => "目標グループ数",
                "distanceMode" => "距離計算",
                "distanceModeHint" => "RGB は高速で安定、HSV は色相の関係を保ちやすく、Lab は人の見た目に近い近傍色判定に向いています。",
                "maxColorDistance" => "近傍色しきい値",
                "preserveDarkOutline" => "暗色輪郭を保持",
                "preserveAlpha" => "アルファを保持",
                "exportSettings" => "書き出し設定",
                "outputFolder" => "出力フォルダ",
                "filePrefix" => "ファイル接頭辞",
                "fileSuffix" => "ファイル接尾辞",
                "conflictMode" => "競合時の処理",
                "refreshAssetDatabase" => "AssetDatabase更新",
                "sourceInfo" => "ソース情報",
                "assetPath" => "アセットパス",
                "size" => "サイズ",
                "paletteColors" => "パレット色数",
                "groups" => "グループ数",
                "noAnalyzedImage" => "まだソース画像が解析されていません。",
                "before" => "Before",
                "after" => "After",
                "palette" => "パレット",
                "paletteEmpty" => "解析後にパレット色がここに表示されます。",
                "replacementRules" => "置換ルール",
                "variations" => "バリエーション",
                "variationsEmpty" => "Auto Group後にバリエーションが表示されます。",
                "locked" => "固定",
                "activeVariation" => "選択中",
                "useVariation" => "選択",
                "skip" => "Skip",
                "outputFile" => "出力ファイル",
                "add" => "追加",
                "duplicate" => "複製",
                "remove" => "削除",
                "groupsEmpty" => "Auto Group後にカラーグループがここに表示されます。",
                "noPreview" => "プレビューなし",
                "targetColor" => "置換色",
                "blendRatio" => "ブレンド率",
                "mode" => "モード",
                "colorRules" => "色別ルール",
                "enabled" => "有効",
                "helpOverview" => "画像を解析し、近い色をグループ化して、置換色のプレビューとPNG書き出しを行います。",
                "helpAnalysis" => "透明度しきい値、最小ピクセル数、量子化ステップで抽出するパレット色を調整します。",
                "helpGrouping" => "目標グループ数と近傍色しきい値で、似た色をどこまで同じグループに含めるかを調整します。",
                "helpPalette" => "パレット行またはグループ行を選択すると、プレビュー上で該当色がハイライトされます。",
                "helpRules" => "Group Uniformはグループ単位、Per Colorは色別ルールのみ、Hybridは色別ルールを優先して不足分をグループ設定で補います。",
                "helpExport" => "Previewを更新してからExportすると、設定したフォルダにPNGを書き出します。",
                _ => english
            };
        }

        private void OnDisable()
        {
            CancelScheduledAutoPreview();
            DestroyReadableSourceImage();
            DestroyAfterPreview();
            DestroySelectionOverlayTexture();
            if (checkerboardTexture != null)
            {
                DestroyImmediate(checkerboardTexture);
                checkerboardTexture = null;
            }
        }

        private void DestroyReadableSourceImage()
        {
            if (readableSourceImage == null)
            {
                return;
            }

            DestroyImmediate(readableSourceImage);
            readableSourceImage = null;
            InvalidateSelectionOverlay();
        }

        private void DestroyAfterPreview()
        {
            if (afterPreview == null)
            {
                return;
            }

            DestroyImmediate(afterPreview);
            afterPreview = null;
        }

        private void DrawSectionHeader(string title)
        {
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        }

        private void DrawSectionSeparator()
        {
            EditorGUILayout.Space(6f);
            Rect rect = GUILayoutUtility.GetRect(1f, 1f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, SeparatorColor);
            EditorGUILayout.Space(6f);
        }

        private void DrawPaneSeparator()
        {
            Rect rect = GUILayoutUtility.GetRect(1f, 1f, GUILayout.Width(1f), GUILayout.ExpandHeight(true));
            EditorGUI.DrawRect(rect, SeparatorColor);
        }

        private void DrawCheckerboard(Rect rect)
        {
            if (checkerboardTexture == null)
            {
                return;
            }

            GUI.DrawTextureWithTexCoords(rect, checkerboardTexture, new Rect(0f, 0f, rect.width / 16f, rect.height / 16f));
        }

        private static Rect FitRect(Rect container, float width, float height)
        {
            float scale = Mathf.Min(container.width / width, container.height / height);
            float fittedWidth = width * scale;
            float fittedHeight = height * scale;
            return new Rect(
                container.x + ((container.width - fittedWidth) * 0.5f),
                container.y + ((container.height - fittedHeight) * 0.5f),
                fittedWidth,
                fittedHeight);
        }

        private static void DrawCenteredLabel(Rect rect, string text)
        {
            GUIStyle style = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                alignment = TextAnchor.MiddleCenter
            };
            GUI.Label(rect, text, style);
        }

        private static Texture2D CreateCheckerboardTexture()
        {
            Texture2D texture = new Texture2D(16, 16, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Point
            };

            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    bool light = ((x / 4) + (y / 4)) % 2 == 0;
                    texture.SetPixel(x, y, light ? new Color(0.72f, 0.72f, 0.72f) : new Color(0.48f, 0.48f, 0.48f));
                }
            }

            texture.Apply();
            return texture;
        }

        private static string GetProjectRoot()
        {
            return System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, ".."));
        }

        private static string ResolveOutputFolderForPanel(string outputFolder)
        {
            if (string.IsNullOrWhiteSpace(outputFolder))
            {
                return Application.dataPath;
            }

            if (System.IO.Path.IsPathRooted(outputFolder))
            {
                return outputFolder;
            }

            return System.IO.Path.GetFullPath(System.IO.Path.Combine(GetProjectRoot(), outputFolder));
        }

        private static string ToProjectRelativePath(string folderPath)
        {
            string projectRoot = GetProjectRoot();
            string fullFolderPath = System.IO.Path.GetFullPath(folderPath);
            string relativePath = System.IO.Path.GetRelativePath(projectRoot, fullFolderPath);
            if (!relativePath.StartsWith("..", System.StringComparison.Ordinal) && !System.IO.Path.IsPathRooted(relativePath))
            {
                return relativePath.Replace('\\', '/');
            }

            return fullFolderPath;
        }

        private static void SelectOutputAsset(string outputPath)
        {
            string projectRoot = GetProjectRoot();
            string fullOutputPath = System.IO.Path.GetFullPath(outputPath);
            if (!fullOutputPath.StartsWith(projectRoot, System.StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string assetPath = System.IO.Path.GetRelativePath(projectRoot, fullOutputPath).Replace('\\', '/');
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (asset == null)
            {
                return;
            }

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
    }

    internal enum PreviewCompareMode
    {
        SideBySide,
        Split
    }

    internal enum PaletteVariantLanguageMode
    {
        Auto,
        English,
        Japanese
    }

    internal enum PaletteVariantDisplayLanguage
    {
        English,
        Japanese
    }

    internal sealed class ParameterHelpWindow : EditorWindow
    {
        private PaletteVariantGeneratorWindow owner;
        private Vector2 scroll;

        public void SetOwner(PaletteVariantGeneratorWindow window)
        {
            owner = window;
        }

        private void OnGUI()
        {
            if (owner == null)
            {
                EditorGUILayout.HelpBox("Open Palette Variant Generator first.", MessageType.Info);
                return;
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);
            DrawHelpSection("Overview", owner.T("helpOverview", "Analyze an image, group nearby colors, preview replacement colors, and export a PNG variant."));
            DrawHelpSection("Analyze", owner.T("helpAnalysis", "Adjust alpha threshold, minimum pixel count, and quantize step to control extracted palette colors."));
            DrawHelpSection("Grouping", owner.T("helpGrouping", "Use target group count and max color distance to control how far nearby colors can be merged into the same group."));
            DrawHelpSection("Palette", owner.T("helpPalette", "Select a palette row or group row to highlight the matching pixels in the preview."));
            DrawHelpSection("Rules", owner.T("helpRules", "Group Uniform uses group rules, Per Color uses individual color rules only, and Hybrid lets color rules override the group fallback."));
            DrawHelpSection("Export", owner.T("helpExport", "Update Preview before Export to write the generated PNG into the configured output folder."));
            EditorGUILayout.EndScrollView();
        }

        private static void DrawHelpSection(string title, string body)
        {
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(body, MessageType.None);
            EditorGUILayout.Space(4f);
        }
    }

    internal sealed class PaletteVariantInfoWindow : EditorWindow
    {
        private InfoMode mode;
        private Vector2 scroll;

        public static void OpenInfo()
        {
            PaletteVariantInfoWindow window = GetWindow<PaletteVariantInfoWindow>("バージョン情報");
            window.mode = InfoMode.Version;
            window.minSize = new Vector2(460f, 320f);
            window.Show();
            window.Focus();
        }

        public static void OpenLicense()
        {
            PaletteVariantInfoWindow window = GetWindow<PaletteVariantInfoWindow>("ライセンス");
            window.mode = InfoMode.License;
            window.minSize = new Vector2(520f, 420f);
            window.Show();
            window.Focus();
        }

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            if (mode == InfoMode.License)
            {
                DrawLicense();
            }
            else
            {
                DrawVersionInfo();
            }

            EditorGUILayout.EndScrollView();
        }

        private static void DrawVersionInfo()
        {
            EditorGUILayout.LabelField(PaletteVariantGeneratorWindow.ProductName, EditorStyles.boldLabel);
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("パッケージ", PaletteVariantGeneratorWindow.PackageName);
            EditorGUILayout.LabelField("バージョン", PaletteVariantGeneratorWindow.PackageVersion);
            EditorGUILayout.LabelField("検証済み Unity", PaletteVariantGeneratorWindow.ValidatedUnityVersion);
            EditorGUILayout.LabelField("メニュー", "Tools > Palette Variant Generator");
            EditorGUILayout.Space(8f);
            EditorGUILayout.TextField("リリース", PaletteVariantGeneratorWindow.ReleaseUrl);
            EditorGUILayout.Space(8f);
            EditorGUILayout.HelpBox(
                "このエディタ拡張は、元画像を変更せずにアイコンのパレット抽出、近傍色グループ化、色置換プレビュー、PNG バリエーション出力を行います。",
                MessageType.None);
        }

        private static void DrawLicense()
        {
            EditorGUILayout.LabelField("ライセンスと利用条件", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "個人・商用の Unity プロジェクトで本ツールを利用できます。本ツールで生成した PNG アセットは配布できます。また、自身のプロジェクト向けにパッケージを改変できます。",
                MessageType.None);
            EditorGUILayout.HelpBox(
                "許可なく本パッケージを競合する単体製品として再配布したり、元のパッケージを自身の著作物として主張したりすることはできません。",
                MessageType.Warning);

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("免責事項", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "本パッケージは現状有姿で提供されます。本パッケージの利用により発生したプロジェクトデータの消失、制作遅延、その他の損害について、作者は責任を負いません。元画像とプロジェクトデータは必ずバックアップしてください。",
                MessageType.None);

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("元画像の扱い", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "本ツールは元画像を上書きしない設計です。出力先とファイル競合時の設定は、利用者が確認してください。",
                MessageType.Info);
        }

        private enum InfoMode
        {
            Version,
            License
        }
    }
}
