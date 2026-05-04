using System;
using System.Collections.Generic;
using System.Linq;
using Sunmax0731.IconPaletteVariantGenerator.Editor.Services;
using Sunmax0731.IconPaletteVariantGenerator.Models;
using Sunmax0731.IconPaletteVariantGenerator.Services;
using Sunmax0731.IconPaletteVariantGenerator.Utilities;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sunmax0731.IconPaletteVariantGenerator.Editor.Windows
{
    /// <summary>
    /// Entry EditorWindow for the Icon Palette Variant Generator.
    /// </summary>
    public sealed class PaletteVariantGeneratorWindow : EditorWindow
    {
        private const float LeftPaneWidth = 300f;
        private const float RightPaneWidth = 500f;
        private const float PaneGap = 14f;
        private const float CompactLayoutWidth = 1220f;
        private const float DockedMinWidth = 760f;
        private const float DockedMinHeight = 540f;
        private const float MinPreviewHeight = 260f;
        private const float MinPreviewCanvasHeight = 180f;
        private const float MaxPreviewCanvasHeight = 720f;
        private const float MinPreviewZoom = 1f;
        private const float MaxPreviewZoom = 8f;
        private const float MaxRgbColorDistance = 442f;
        private const float PaletteListHeight = 170f;
        private const float LayerListHeight = 150f;
        private const float VariationListHeight = 126f;
        private const double AutoPreviewDebounceSeconds = 0.25d;
        private const double LargeImageAutoPreviewDebounceSeconds = 0.75d;
        private const int LargeImageAutoPreviewPixelCount = 1024 * 1024;
        internal const string ProductName = "Unity Icon Palette Variant Generator";
        internal const string PackageName = "com.sunmax0731.icon-palette-variant-generator";
        internal const string PackageVersion = "1.0.4";
        internal const string ValidatedUnityVersion = "6000.4.0f1";
        internal const string ReleaseUrl = "https://github.com/Sunmax0731/unity-icon-pallete-variant-generator/releases/tag/v1.0.4";
        internal const string RepositoryUrl = "https://github.com/Sunmax0731/unity-icon-pallete-variant-generator";
        internal const string MainWindowRootName = "palette-variant-main-root";
        internal const string MainWindowScrollName = "palette-variant-main-scroll";
        private const string LanguageModePrefsKey = "Sunmax.IconPaletteVariantGenerator.LanguageMode";
        private const string AutoPreviewPrefsKey = "Sunmax.IconPaletteVariantGenerator.AutoPreview";
        private const string SelectionHighlightPrefsKey = "Sunmax.IconPaletteVariantGenerator.SelectionHighlight";
        private const int DefaultBrushSize = 5;
        private static readonly Color SeparatorColor = new Color(0.25f, 0.25f, 0.25f, 0.8f);
        private static readonly Color OverlayColor = new Color(0.1f, 0.65f, 1f, 0.34f);
        private static readonly Color OverlayBorderColor = new Color(0.1f, 0.65f, 1f, 0.85f);
        private static readonly Color NoiseEffectHighlightColor = new Color(1f, 0.76f, 0.1f, 0.55f);
        private static readonly Color EdgeEffectHighlightColor = new Color(1f, 0.2f, 0.16f, 0.62f);
        private static readonly Color BadgeNeutralColor = new Color(0.32f, 0.36f, 0.42f, 1f);
        private static readonly Color BadgeActiveColor = new Color(0.12f, 0.5f, 0.27f, 1f);
        private static readonly Color BadgeWarningColor = new Color(0.74f, 0.42f, 0.08f, 1f);
        private static readonly Color BadgeTransparentColor = new Color(0.35f, 0.24f, 0.62f, 1f);

        private PaletteVariantSession session = new PaletteVariantSession();
        private readonly TextureAssetLoader textureAssetLoader = new TextureAssetLoader();
        private readonly ColorExtractionService colorExtractionService = new ColorExtractionService();
        private readonly ColorGroupingService colorGroupingService = new ColorGroupingService();
        private readonly ColorQuantizationService colorQuantizationService = new ColorQuantizationService();
        private readonly ColorReplacementService colorReplacementService = new ColorReplacementService();
        private readonly LayerCompositingService layerCompositingService = new LayerCompositingService();
        private readonly LayerTextureSerializationService layerTextureSerializationService = new LayerTextureSerializationService();
        private readonly RasterPaintService rasterPaintService = new RasterPaintService();
        private readonly PaintStrokeSessionService paintStrokeSessionService;
        private readonly IconVariationService variationService = new IconVariationService();
        private readonly PngExportService pngExportService = new PngExportService();
        private readonly SessionJsonService sessionJsonService = new SessionJsonService();
        private readonly RulePresetJsonService rulePresetJsonService = new RulePresetJsonService();
        private readonly BatchSourceExportService batchSourceExportService = new BatchSourceExportService();
        private readonly RulePresetAssetService rulePresetAssetService = new RulePresetAssetService();
        private Texture2D sourceImage;
        private Texture2D readableSourceImage;
        private Texture2D afterPreview;
        private Texture2D replacementPreview;
        private Texture2D splitPreviewTexture;
        private Texture2D highlightedBeforePreviewTexture;
        private Texture2D highlightedAfterPreviewTexture;
        private Texture2D highlightedSplitPreviewTexture;
        private Texture2D zoomedBeforePreviewTexture;
        private Texture2D zoomedAfterPreviewTexture;
        private Texture2D zoomedSplitPreviewTexture;
        private Texture2D diffPreviewTexture;
        private Texture2D sourceEditingPreviewTexture;
        private string highlightedBeforePreviewKey = string.Empty;
        private string highlightedAfterPreviewKey = string.Empty;
        private string highlightedSplitPreviewKey = string.Empty;
        private string zoomedBeforePreviewKey = string.Empty;
        private string zoomedAfterPreviewKey = string.Empty;
        private string zoomedSplitPreviewKey = string.Empty;
        private string diffPreviewKey = string.Empty;
        private string splitPreviewCacheKey = string.Empty;
        private string sourceEditingPreviewKey = string.Empty;
        private Texture2D checkerboardTexture;
        private Texture2D selectionOverlayTexture;
        private string selectionOverlayCacheKey = string.Empty;
        private Vector2 leftScroll;
        private Vector2 paletteScroll;
        private Vector2 variationScroll;
        private Vector2 batchResultScroll;
        private Vector2 compactLayoutScroll;
        private Vector2 rightScroll;
        private string sourceAssetPath = string.Empty;
        private string batchSourceFolder = "Assets";
        private List<BatchSourceExportItem> batchResults = new List<BatchSourceExportItem>();
        private PaletteVariantRulePresetAsset presetAsset;
        private string reportMessage = "Select a project PNG or Texture2D asset, then click Analyze.";
        private MessageType reportType = MessageType.Info;
        private PaletteVariantLanguageMode languageMode = PaletteVariantLanguageMode.Auto;
        private PaletteVariantDisplayLanguage displayLanguage = PaletteVariantDisplayLanguage.English;
        private ParameterHelpWindow parameterHelpWindow;
        private bool autoPreviewEnabled = true;
        private bool selectionHighlightEnabled = true;
        private bool effectHighlightEnabled = true;
        private bool showExportOptions;
        private bool autoPreviewPending;
        private double autoPreviewScheduledTime;
        private PreviewCompareMode previewCompareMode = PreviewCompareMode.SideBySide;
        private PreviewInteractionMode previewInteractionMode = PreviewInteractionMode.Pick;
        private AnalysisCategoryPreset selectedAnalysisPreset = AnalysisCategoryPreset.TransparentPng;
        private float previewZoom = 1f;
        private float previewSplit = 0.5f;
        private int previewBrushSize = DefaultBrushSize;
        private Vector2 previewPan;
        private bool previewDragActive;
        private bool previewDragMoved;
        private bool previewBrushActive;
        private bool previewPaintActive;
        private bool previewResizeActive;
        private int previewDragPointerId = -1;
        private Vector2 previewDragStartPosition;
        private Vector2 previewDragStartPan;
        private Vector2 previewResizeStartPosition;
        private float previewResizeStartHeight;
        private readonly HashSet<string> brushedColorEntryIds = new HashSet<string>();
        private bool previewRefreshQueued;
        private bool previewRefreshProcessing;
        private int previewRefreshRequestVersion;
        private int scheduledPreviewRefreshVersion;
        private bool showSourceSection = true;
        private bool showAnalyzeSection = true;
        private bool showGroupSection = true;
        private bool showToolSection = true;
        private bool showExportSection = true;
        private bool showPresetSection = true;
        private float previewCanvasHeight = MinPreviewHeight;
        private readonly Stack<string> undoSnapshots = new Stack<string>();
        private readonly Stack<string> redoSnapshots = new Stack<string>();
        private string selectedGroupId = string.Empty;
        private string selectedColorEntryId = string.Empty;
        private Label reportLabel;
        private Label sourcePathLabel;
        private Label sourceSizeLabel;
        private Label sourcePaletteLabel;
        private PopupField<string> paintTargetPopup;
        private PopupField<string> drawToolPopup;
        private PopupField<string> previewInteractionModePopup;
        private PopupField<string> previewCompareModePopup;
        private Image beforePreviewImage;
        private Image afterPreviewImage;
        private ScrollView layerListElement;
        private Button addPaintLayerButton;
        private Button addImageLayerButton;
        private Button duplicateLayerButton;
        private Button moveLayerUpButton;
        private Button moveLayerDownButton;
        private Button deleteLayerButton;
        private ScrollView paletteListElement;
        private ScrollView groupListElement;
        private ScrollView variationListElement;
        private VisualElement replacementRuleContainer;
        private VisualElement colorRuleContainer;
        private VisualElement selectedColorInfoContainer;
        private bool isRefreshingUiToolkit;
        private List<int> lastNoiseEffectHighlightIndices = new List<int>();
        private List<int> lastEdgeEffectHighlightIndices = new List<int>();
        private PaintStrokeSession activePaintSession;

        public PaletteVariantGeneratorWindow()
        {
            paintStrokeSessionService = new PaintStrokeSessionService(layerTextureSerializationService, rasterPaintService);
        }

        [MenuItem("Tools/Palette Variant Generator/メイン画面")]
        public static void Open()
        {
            OpenWindow();
        }

        private static void OpenWindow()
        {
            PaletteVariantGeneratorWindow window = GetWindow<PaletteVariantGeneratorWindow>();
            window.titleContent = new GUIContent("Palette Variant Generator");
            window.minSize = new Vector2(DockedMinWidth, DockedMinHeight);
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
            selectionHighlightEnabled = EditorPrefs.GetBool(SelectionHighlightPrefsKey, true);
            LoadLanguageMode();
        }

        public void CreateGUI()
        {
            rootVisualElement.Clear();
            rootVisualElement.Add(BuildUiToolkitRoot());
            ApplyPreviewCanvasHeight();
            RefreshUiToolkitContent();
        }

        internal bool IsUiToolkitHostActive()
        {
            return rootVisualElement != null
                && rootVisualElement.Q<VisualElement>(MainWindowRootName) != null
                && rootVisualElement.Q<IMGUIContainer>() == null;
        }

        internal bool ContainsProductionUiToolkitSections()
        {
            return rootVisualElement.Q<VisualElement>("source-section") != null
                && rootVisualElement.Q<VisualElement>("analyze-section") != null
                && rootVisualElement.Q<VisualElement>("group-section") != null
                && rootVisualElement.Q<VisualElement>("preview-section") != null
                && rootVisualElement.Q<VisualElement>("palette-section") != null
                && rootVisualElement.Q<VisualElement>("variation-section") != null
                && rootVisualElement.Q<VisualElement>("replacement-rule-section") != null
                && rootVisualElement.Q<VisualElement>("color-rule-section") != null
                && rootVisualElement.Q<ScrollView>(MainWindowScrollName) != null;
        }

        private VisualElement BuildUiToolkitRoot()
        {
            VisualElement root = new VisualElement { name = MainWindowRootName };
            root.style.flexGrow = 1f;
            root.style.flexDirection = FlexDirection.Column;
            root.style.paddingLeft = 8f;
            root.style.paddingRight = 8f;
            root.style.paddingTop = 6f;
            root.style.paddingBottom = 6f;

            root.Add(BuildUiToolkitToolbar());

            reportLabel = new Label { name = "report-label" };
            reportLabel.style.whiteSpace = WhiteSpace.Normal;
            reportLabel.style.marginTop = 4f;
            reportLabel.style.marginBottom = 4f;
            root.Add(reportLabel);

            ScrollView scroll = new ScrollView(ScrollViewMode.Vertical) { name = MainWindowScrollName };
            scroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            scroll.style.flexGrow = 1f;

            VisualElement content = new VisualElement { name = "main-content" };
            content.style.flexDirection = FlexDirection.Row;
            content.style.flexWrap = Wrap.Wrap;
            content.style.alignItems = Align.Stretch;

            VisualElement leftColumn = CreateUiColumn("settings-column", 300f, 1f);
            leftColumn.Add(CreateCollapsibleSection("source-foldout", T("sourceInfo", "Source Info"), showSourceSection, value => showSourceSection = value, BuildSourceSection));
            leftColumn.Add(CreateCollapsibleSection("analyze-foldout", T("analyzeSettings", "Analyze Settings"), showAnalyzeSection, value => showAnalyzeSection = value, BuildAnalyzeSection));
            leftColumn.Add(CreateCollapsibleSection("group-foldout", T("groupSettings", "Group Settings"), showGroupSection, value => showGroupSection = value, BuildGroupSection));
            leftColumn.Add(CreateCollapsibleSection("tool-foldout", T("toolSettings", "Tool Settings"), showToolSection, value => showToolSection = value, BuildToolSection));
            leftColumn.Add(CreateCollapsibleSection("export-foldout", T("exportSettings", "Export Settings"), showExportSection, value => showExportSection = value, BuildExportSection));
            leftColumn.Add(CreateCollapsibleSection("preset-foldout", T("presetAsset", "Preset Asset"), showPresetSection, value => showPresetSection = value, BuildPresetSection));

            VisualElement centerColumn = CreateUiColumn("preview-column", 420f, 2f);
            centerColumn.Add(BuildPreviewSection());
            centerColumn.Add(BuildPaletteSection());

            VisualElement rightColumn = CreateUiColumn("rules-column", 420f, 1f);
            rightColumn.Add(BuildLayerSection());
            rightColumn.Add(BuildVariationSection());
            rightColumn.Add(BuildReplacementRuleSection());
            rightColumn.Add(BuildColorRuleSection());

            content.Add(leftColumn);
            content.Add(centerColumn);
            content.Add(rightColumn);
            scroll.Add(content);
            root.Add(scroll);
            return root;
        }

        private Toolbar BuildUiToolkitToolbar()
        {
            Toolbar toolbar = new Toolbar { name = "main-toolbar" };
            ObjectField sourceField = new ObjectField(T("sourceImage", "Source Image"))
            {
                name = "source-image-field",
                objectType = typeof(Texture2D),
                allowSceneObjects = false,
                value = sourceImage
            };
            sourceField.style.minWidth = 240f;
            sourceField.style.flexGrow = 1f;
            sourceField.RegisterValueChangedCallback(evt =>
            {
                if (isRefreshingUiToolkit)
                {
                    return;
                }

                sourceImage = evt.newValue as Texture2D;
                RefreshUiToolkitContent();
            });
            toolbar.Add(sourceField);
            toolbar.Add(CreateToolbarButton(T("analyze", "Analyze"), () => AnalyzeSourceImage(), () => sourceImage != null));
            toolbar.Add(CreateToolbarButton(T("autoGroup", "Auto Group"), () => AutoGroupPalette(), () => session.paletteColors.Count > 0));
            toolbar.Add(CreateToolbarButton(T("preview", "Preview"), () => RequestPreviewRefresh("Preview update queued."), () => readableSourceImage != null && session.colorGroups.Count > 0));
            toolbar.Add(CreateToolbarButton(T("export", "Export"), () => OpenExportSettingsWindow(), () => true));
            toolbar.Add(CreateToolbarButton(T("exportAll", "Export All"), () => OpenExportSettingsWindow(), () => true));
            toolbar.Add(CreateToolbarButton(T("undo", "Undo"), () => UndoSessionEdit(), () => undoSnapshots.Count > 0));
            toolbar.Add(CreateToolbarButton(T("redo", "Redo"), () => RedoSessionEdit(), () => redoSnapshots.Count > 0));
            toolbar.Add(CreateToolbarButton(T("saveSession", "Save Session"), () => SaveSession(), () => true));
            toolbar.Add(CreateToolbarButton(T("loadSession", "Load Session"), () => LoadSession(), () => true));
            toolbar.Add(CreateToolbarButton(T("help", "Help"), () => OpenHelpWindow(), () => true));
            toolbar.Add(BuildLanguagePopup());
            toolbar.Add(BuildAutoPreviewToggle());
            return toolbar;
        }

        private Button CreateToolbarButton(string text, System.Action action, System.Func<bool> enabled)
        {
            Button button = new Button(() => RunUiToolkitAction(action)) { text = text };
            button.SetEnabled(enabled == null || enabled());
            button.style.minWidth = 72f;
            return button;
        }

        private void RecordSessionEdit(System.Action edit)
        {
            if (edit == null)
            {
                return;
            }

            EnsureLayerSessionState();
            undoSnapshots.Push(JsonUtility.ToJson(session));
            redoSnapshots.Clear();
            edit.Invoke();
            variationService.SyncActiveVariation(session);
            HandlePreviewSettingChanged();
        }

        private void EnsureLayerSessionState()
        {
            session.drawingToolSettings ??= new DrawingToolSettings();
            session.sourcePixelData ??= new LayerPixelData();
            session.layers ??= new List<RasterLayer>();
            if (!string.IsNullOrWhiteSpace(session.activeLayerId)
                && session.layers.Any(layer => layer != null && layer.id == session.activeLayerId))
            {
                return;
            }

            session.activeLayerId = session.layers.FirstOrDefault(layer => layer != null)?.id ?? string.Empty;
        }

        private void ApplyAnalysisCategoryPreset(AnalysisCategoryPreset preset)
        {
            RecordSessionEdit(() =>
            {
                switch (preset)
                {
                    case AnalysisCategoryPreset.Gem:
                        session.analyzeSettings.alphaThreshold = 4;
                        session.analyzeSettings.minimumPixelCount = 1;
                        session.analyzeSettings.quantizeStep = 2;
                        session.analyzeSettings.maxPaletteColors = 384;
                        session.groupSettings.targetGroupCount = 8;
                        session.groupSettings.distanceMode = ColorDistanceMode.Lab;
                        session.groupSettings.maxColorDistance = 96f;
                        session.groupSettings.preserveDarkOutline = false;
                        session.groupSettings.preserveAlpha = true;
                        session.noiseRemovalSettings.enabled = true;
                        session.noiseRemovalSettings.maxRegionPixels = 3;
                        session.noiseRemovalSettings.neighborDistanceThreshold = 24f;
                        session.edgeOutsideCleanupSettings.enabled = false;
                        break;
                    case AnalysisCategoryPreset.Plant:
                        session.analyzeSettings.alphaThreshold = 6;
                        session.analyzeSettings.minimumPixelCount = 1;
                        session.analyzeSettings.quantizeStep = 4;
                        session.analyzeSettings.maxPaletteColors = 320;
                        session.groupSettings.targetGroupCount = 7;
                        session.groupSettings.distanceMode = ColorDistanceMode.Hsv;
                        session.groupSettings.maxColorDistance = 120f;
                        session.groupSettings.preserveDarkOutline = false;
                        session.groupSettings.preserveAlpha = true;
                        session.noiseRemovalSettings.enabled = true;
                        session.noiseRemovalSettings.maxRegionPixels = 4;
                        session.noiseRemovalSettings.neighborDistanceThreshold = 36f;
                        session.edgeOutsideCleanupSettings.enabled = false;
                        break;
                    case AnalysisCategoryPreset.WhiteBackgroundJpg:
                        session.analyzeSettings.alphaThreshold = 0;
                        session.analyzeSettings.minimumPixelCount = 2;
                        session.analyzeSettings.quantizeStep = 8;
                        session.analyzeSettings.maxPaletteColors = 192;
                        session.groupSettings.targetGroupCount = 5;
                        session.groupSettings.distanceMode = ColorDistanceMode.Lab;
                        session.groupSettings.maxColorDistance = 72f;
                        session.groupSettings.preserveDarkOutline = false;
                        session.groupSettings.preserveAlpha = false;
                        session.noiseRemovalSettings.enabled = true;
                        session.noiseRemovalSettings.maxRegionPixels = 6;
                        session.noiseRemovalSettings.neighborDistanceThreshold = 48f;
                        session.edgeOutsideCleanupSettings.enabled = true;
                        session.edgeOutsideCleanupSettings.mode = EdgeOutsideCleanupMode.BoundaryTrim;
                        session.edgeOutsideCleanupSettings.trimDistancePixels = 1;
                        break;
                    case AnalysisCategoryPreset.LineArtIcon:
                        session.analyzeSettings.alphaThreshold = 8;
                        session.analyzeSettings.minimumPixelCount = 1;
                        session.analyzeSettings.quantizeStep = 1;
                        session.analyzeSettings.maxPaletteColors = 128;
                        session.groupSettings.targetGroupCount = 4;
                        session.groupSettings.distanceMode = ColorDistanceMode.Rgb;
                        session.groupSettings.maxColorDistance = 42f;
                        session.groupSettings.preserveDarkOutline = true;
                        session.groupSettings.preserveAlpha = true;
                        session.noiseRemovalSettings.enabled = false;
                        session.edgeOutsideCleanupSettings.enabled = false;
                        break;
                    case AnalysisCategoryPreset.TransparentPng:
                    default:
                        session.analyzeSettings.alphaThreshold = 8;
                        session.analyzeSettings.minimumPixelCount = 1;
                        session.analyzeSettings.quantizeStep = 4;
                        session.analyzeSettings.maxPaletteColors = 256;
                        session.groupSettings.targetGroupCount = 6;
                        session.groupSettings.distanceMode = ColorDistanceMode.Rgb;
                        session.groupSettings.maxColorDistance = 441f;
                        session.groupSettings.preserveDarkOutline = true;
                        session.groupSettings.preserveAlpha = true;
                        session.noiseRemovalSettings.enabled = false;
                        session.edgeOutsideCleanupSettings.enabled = false;
                        break;
                }
            });
            reportMessage = $"Applied preset: {preset}.";
            reportType = MessageType.Info;
        }

        private void UndoSessionEdit()
        {
            if (undoSnapshots.Count == 0)
            {
                return;
            }

            redoSnapshots.Push(JsonUtility.ToJson(session));
            RestoreSessionSnapshot(undoSnapshots.Pop());
            reportMessage = "Undo applied.";
            reportType = MessageType.Info;
        }

        private void RedoSessionEdit()
        {
            if (redoSnapshots.Count == 0)
            {
                return;
            }

            undoSnapshots.Push(JsonUtility.ToJson(session));
            RestoreSessionSnapshot(redoSnapshots.Pop());
            reportMessage = "Redo applied.";
            reportType = MessageType.Info;
        }

        private void RestoreSessionSnapshot(string snapshot)
        {
            if (string.IsNullOrWhiteSpace(snapshot))
            {
                return;
            }

            session = JsonUtility.FromJson<PaletteVariantSession>(snapshot) ?? new PaletteVariantSession();
            NormalizeSessionDefaults(session);
            ApplySessionSourcePixelsToReadableImageIfAvailable();
            selectedGroupId = session.colorGroups.Count > 0 ? session.colorGroups[0].id : string.Empty;
            selectedColorEntryId = string.Empty;
            DestroyAfterPreview();
            InvalidateSelectionOverlay();
            RequestPreviewRefresh("Preview update queued after history change.");
        }

        private static void NormalizeSessionDefaults(PaletteVariantSession targetSession)
        {
            targetSession.analyzeSettings ??= new AnalyzeSettings();
            targetSession.groupSettings ??= new GroupSettings();
            targetSession.edgeOutsideCleanupSettings ??= new EdgeOutsideCleanupSettings();
            targetSession.noiseRemovalSettings ??= new NoiseRemovalSettings();
            targetSession.drawingToolSettings ??= new DrawingToolSettings();
            targetSession.exportSettings ??= new ExportSettings();
            targetSession.sourcePixelData ??= new LayerPixelData();
            targetSession.paletteColors ??= new List<PaletteColorEntry>();
            targetSession.colorGroups ??= new List<ColorGroup>();
            targetSession.colorRules ??= new List<ColorReplacementRule>();
            targetSession.variations ??= new List<IconVariation>();
            targetSession.layers ??= new List<RasterLayer>();
        }

        private VisualElement BuildLanguagePopup()
        {
            EnumField field = new EnumField(languageMode) { name = "language-popup" };
            field.tooltip = T("language", "Language");
            field.RegisterValueChangedCallback(evt =>
            {
                if (isRefreshingUiToolkit)
                {
                    return;
                }

                languageMode = (PaletteVariantLanguageMode)evt.newValue;
                displayLanguage = ResolveDisplayLanguage(languageMode);
                EditorPrefs.SetInt(LanguageModePrefsKey, (int)languageMode);
                CreateGUI();
            });
            return field;
        }

        private Toggle BuildAutoPreviewToggle()
        {
            Toggle toggle = new Toggle(T("autoPreview", "Auto Preview")) { name = "auto-preview-toggle", value = autoPreviewEnabled };
            toggle.RegisterValueChangedCallback(evt =>
            {
                if (isRefreshingUiToolkit)
                {
                    return;
                }

                autoPreviewEnabled = evt.newValue;
                EditorPrefs.SetBool(AutoPreviewPrefsKey, autoPreviewEnabled);
            });
            return toggle;
        }

        private VisualElement BuildSourceSection()
        {
            VisualElement section = CreateUiSection("source-section", T("sourceInfo", "Source Info"));
            sourcePathLabel = CreateWrappingLabel("source-asset-path-label");
            sourceSizeLabel = CreateWrappingLabel("source-size-label");
            sourcePaletteLabel = CreateWrappingLabel("source-palette-label");
            section.Add(sourcePathLabel);
            section.Add(sourceSizeLabel);
            section.Add(sourcePaletteLabel);
            return section;
        }

        private VisualElement BuildAnalyzeSection()
        {
            VisualElement section = CreateUiSection("analyze-section", T("analyzeSettings", "Analyze Settings"));
            VisualElement presetField = CreateEnumField("analysis-preset-popup", T("analysisPreset", "Category Preset"), selectedAnalysisPreset, value =>
            {
                selectedAnalysisPreset = (AnalysisCategoryPreset)value;
                ApplyAnalysisCategoryPreset(selectedAnalysisPreset);
            });
            section.Add(AttachParameterHelp(presetField, ParameterHelpText("analysisPreset")));
            section.Add(AttachParameterHelp(CreateSliderInt("alpha-threshold-slider", T("alphaThreshold", "Alpha Threshold"), session.analyzeSettings.alphaThreshold, 0, 255, value => session.analyzeSettings.alphaThreshold = value), ParameterHelpText("alphaThreshold")));
            section.Add(AttachParameterHelp(CreateIntegerField("minimum-pixel-count-field", T("minimumPixelCount", "Minimum Pixel Count"), session.analyzeSettings.minimumPixelCount, value => session.analyzeSettings.minimumPixelCount = Mathf.Max(1, value)), ParameterHelpText("minimumPixelCount")));
            section.Add(AttachParameterHelp(CreateSliderInt("quantize-step-slider", T("quantizeStep", "Quantize Step"), session.analyzeSettings.quantizeStep, 1, 64, value => session.analyzeSettings.quantizeStep = value), ParameterHelpText("quantizeStep")));
            section.Add(AttachParameterHelp(CreateIntegerField("max-palette-colors-field", T("maxPaletteColors", "Max Palette Colors"), session.analyzeSettings.maxPaletteColors, value => session.analyzeSettings.maxPaletteColors = Mathf.Max(1, value)), ParameterHelpText("maxPaletteColors")));
            return section;
        }

        private VisualElement BuildGroupSection()
        {
            VisualElement section = CreateUiSection("group-section", T("groupSettings", "Group Settings"));
            section.Add(AttachParameterHelp(CreateSliderInt("target-group-count-slider", T("targetGroupCount", "Target Group Count"), session.groupSettings.targetGroupCount, 1, 64, value => RecordSessionEdit(() => session.groupSettings.targetGroupCount = value)), ParameterHelpText("targetGroupCount")));
            section.Add(AttachParameterHelp(CreateEnumField("distance-mode-popup", T("distanceMode", "Distance Mode"), session.groupSettings.distanceMode, value => RecordSessionEdit(() => session.groupSettings.distanceMode = (ColorDistanceMode)value)), ParameterHelpText("distanceMode")));
            section.Add(AttachParameterHelp(CreateSlider("near-color-threshold-slider", T("maxColorDistance", "Max Color Distance"), session.groupSettings.maxColorDistance, 0f, 441f, value => RecordSessionEdit(() => session.groupSettings.maxColorDistance = value)), ParameterHelpText("maxColorDistance")));
            section.Add(AttachParameterHelp(CreateToggle("preserve-dark-outline-toggle", T("preserveDarkOutline", "Preserve Dark Outline"), session.groupSettings.preserveDarkOutline, value => RecordSessionEdit(() => session.groupSettings.preserveDarkOutline = value)), ParameterHelpText("preserveDarkOutline")));
            section.Add(AttachParameterHelp(CreateToggle("preserve-alpha-toggle", T("preserveAlpha", "Preserve Alpha"), session.groupSettings.preserveAlpha, value => RecordSessionEdit(() => session.groupSettings.preserveAlpha = value)), ParameterHelpText("preserveAlpha")));

            section.Add(CreateUiSubHeader(T("edgeOutsideCleanup", "Edge Outside Cleanup")));
            session.edgeOutsideCleanupSettings ??= new EdgeOutsideCleanupSettings();
            section.Add(AttachParameterHelp(CreateToggle("edge-cleanup-toggle", T("edgeCleanupEnabled", "Enable Edge Cleanup"), session.edgeOutsideCleanupSettings.enabled, value => RecordSessionEdit(() => session.edgeOutsideCleanupSettings.enabled = value)), ParameterHelpText("edgeCleanupEnabled")));
            section.Add(AttachParameterHelp(CreateEnumField("edge-cleanup-mode-popup", T("edgeCleanupMode", "Cleanup Mode"), session.edgeOutsideCleanupSettings.mode, value => RecordSessionEdit(() => session.edgeOutsideCleanupSettings.mode = (EdgeOutsideCleanupMode)value)), ParameterHelpText("edgeCleanupMode")));
            section.Add(AttachParameterHelp(CreateSliderInt("edge-cleanup-distance-slider", T("edgeCleanupDistance", "Outside Distance"), session.edgeOutsideCleanupSettings.maxDistancePixels, 1, 12, value => RecordSessionEdit(() => session.edgeOutsideCleanupSettings.maxDistancePixels = value)), ParameterHelpText("edgeCleanupDistance")));
            section.Add(AttachParameterHelp(CreateSliderInt("edge-cleanup-region-slider", T("edgeCleanupMaxRegion", "Max Outside Region"), session.edgeOutsideCleanupSettings.maxRegionPixels, 1, 128, value => RecordSessionEdit(() => session.edgeOutsideCleanupSettings.maxRegionPixels = value)), ParameterHelpText("edgeCleanupMaxRegion")));
            section.Add(AttachParameterHelp(CreateSliderInt("edge-trim-distance-slider", T("edgeTrimDistance", "Trim Distance"), session.edgeOutsideCleanupSettings.trimDistancePixels, 1, 4, value => RecordSessionEdit(() => session.edgeOutsideCleanupSettings.trimDistancePixels = value)), ParameterHelpText("edgeTrimDistance")));

            section.Add(CreateUiSubHeader(T("noiseRemoval", "Noise Removal")));
            session.noiseRemovalSettings ??= new NoiseRemovalSettings();
            section.Add(AttachParameterHelp(CreateToggle("noise-removal-toggle", T("noiseRemovalEnabled", "Enable Noise Removal"), session.noiseRemovalSettings.enabled, value => session.noiseRemovalSettings.enabled = value), ParameterHelpText("noiseRemovalEnabled")));
            section.Add(AttachParameterHelp(CreateSliderInt("max-noise-region-slider", T("maxNoiseRegionPixels", "Max Noise Size"), session.noiseRemovalSettings.maxRegionPixels, 1, 64, value => session.noiseRemovalSettings.maxRegionPixels = value), ParameterHelpText("maxNoiseRegionPixels")));
            section.Add(AttachParameterHelp(CreateSlider("noise-neighbor-threshold-slider", T("noiseNeighborThreshold", "Neighbor Threshold"), session.noiseRemovalSettings.neighborDistanceThreshold, 0f, 441f, value => session.noiseRemovalSettings.neighborDistanceThreshold = value), ParameterHelpText("noiseNeighborThreshold")));
            section.Add(AttachParameterHelp(CreateToggle("same-group-only-toggle", T("sameGroupOnly", "Same Group Only"), session.noiseRemovalSettings.sameGroupOnly, value => session.noiseRemovalSettings.sameGroupOnly = value), ParameterHelpText("sameGroupOnly")));
            return section;
        }

        private VisualElement BuildExportSection()
        {
            VisualElement section = CreateUiSection("export-section", T("exportSettings", "Export Settings"));
            Button toggle = new Button(() => OpenExportSettingsWindow())
            {
                text = T("showExportOptions", "Show Export Options")
            };
            section.Add(toggle);

            VisualElement details = new VisualElement { name = "export-details" };
            details.style.display = DisplayStyle.None;
            VisualElement folderRow = new VisualElement { name = "export-folder-row" };
            folderRow.style.flexDirection = FlexDirection.Row;
            TextField outputField = CreateTextField("export-folder-field", T("outputFolder", "Output Folder"), session.exportSettings.outputFolder, value => session.exportSettings.outputFolder = value);
            outputField.style.flexGrow = 1f;
            folderRow.Add(outputField);
            folderRow.Add(new Button(() => RunUiToolkitAction(SelectOutputFolder)) { text = "..." });
            details.Add(folderRow);
            details.Add(CreateTextField("file-prefix-field", T("filePrefix", "File Prefix"), session.exportSettings.filePrefix, value => session.exportSettings.filePrefix = value));
            details.Add(CreateTextField("file-suffix-field", T("fileSuffix", "File Suffix"), session.exportSettings.fileSuffix, value =>
            {
                session.exportSettings.fileSuffix = value;
                IconVariation activeVariation = variationService.GetActiveVariation(session);
                if (activeVariation != null)
                {
                    activeVariation.fileSuffix = value;
                }
            }));
            details.Add(CreateEnumField("conflict-mode-popup", T("conflictMode", "Conflict Mode"), session.exportSettings.conflictMode, value => session.exportSettings.conflictMode = (ExportConflictMode)value));
            details.Add(CreateToggle("refresh-asset-database-toggle", T("refreshAssetDatabase", "Refresh AssetDatabase"), session.exportSettings.refreshAssetDatabase, value => session.exportSettings.refreshAssetDatabase = value));
            details.Add(BuildBatchSection());
            section.Add(details);
            return section;
        }

        private VisualElement BuildToolSection()
        {
            EnsureLayerSessionState();
            VisualElement section = CreateUiSection("tool-section", T("toolSettings", "Tool Settings"));
            paintTargetPopup = CreateEnumField("paint-target-popup", T("paintTarget", "Paint Target"), session.drawingToolSettings.paintTarget, value =>
            {
                session.drawingToolSettings.paintTarget = (PaintEditTarget)value;
                EnsurePreviewModeMatchesActiveTool();
                RefreshUiToolkitContent();
            });
            section.Add(paintTargetPopup);
            drawToolPopup = CreateEnumField("draw-tool-popup", T("tool", "Tool"), session.drawingToolSettings.activeTool, value =>
            {
                session.drawingToolSettings.activeTool = (DrawToolKind)value;
                EnsurePreviewModeMatchesActiveTool();
                RefreshUiToolkitContent();
            });
            section.Add(drawToolPopup);
            section.Add(CreateSliderInt("draw-brush-size-slider", T("brushSize", "Brush Size"), session.drawingToolSettings.brushSize, 1, 64, value =>
            {
                session.drawingToolSettings.brushSize = Mathf.Max(1, value | 1);
            }));
            section.Add(CreateSlider("draw-strength-slider", T("strength", "Strength"), session.drawingToolSettings.strength, 0.05f, 1f, value =>
            {
                session.drawingToolSettings.strength = value;
            }));
            section.Add(CreateSlider("draw-opacity-slider", T("paintOpacity", "Paint Opacity"), session.drawingToolSettings.paintOpacity, 0f, 1f, value =>
            {
                session.drawingToolSettings.paintOpacity = value;
            }));
            section.Add(CreateColorField("draw-color-field", T("paintColor", "Paint Color"), session.drawingToolSettings.paintColor, value =>
            {
                session.drawingToolSettings.paintColor = ToColor32(value);
            }));
            section.Add(CreateSliderInt("draw-noise-size-slider", T("noiseRegion", "Noise Region"), session.drawingToolSettings.noiseRegionPixels, 1, 32, value =>
            {
                session.drawingToolSettings.noiseRegionPixels = value;
            }));
            section.Add(CreateSlider("draw-noise-threshold-slider", T("noiseThreshold", "Noise Threshold"), session.drawingToolSettings.noiseThreshold, 0f, MaxRgbColorDistance, value =>
            {
                session.drawingToolSettings.noiseThreshold = value;
            }));
            section.Add(CreateSliderInt("draw-blur-radius-slider", T("blurRadius", "Blur Radius"), session.drawingToolSettings.blurRadius, 1, 8, value =>
            {
                session.drawingToolSettings.blurRadius = value;
            }));
            section.Add(CreateSliderInt("draw-smooth-iterations-slider", T("smoothIterations", "Smooth Iterations"), session.drawingToolSettings.smoothIterations, 1, 4, value =>
            {
                session.drawingToolSettings.smoothIterations = value;
            }));
            section.Add(CreateWrappingLabel(T("toolSettingsHint", "Use Preview Mode = Paint to edit the selected target directly on the preview.")));
            return section;
        }

        private VisualElement BuildLayerSection()
        {
            EnsureLayerSessionState();
            VisualElement section = CreateUiSection("layer-section", T("layers", "Layers"));
            VisualElement editRow = new VisualElement();
            editRow.style.flexDirection = FlexDirection.Row;
            editRow.style.flexWrap = Wrap.Wrap;
            editRow.style.marginTop = 4f;
            addPaintLayerButton = new Button(() => RunUiToolkitAction(AddPaintLayer)) { text = T("addPaintLayer", "Add Paint Layer") };
            addImageLayerButton = new Button(() => RunUiToolkitAction(AddImageLayer)) { text = T("addImageLayer", "Add Image Layer") };
            duplicateLayerButton = new Button(() => RunUiToolkitAction(DuplicateActiveLayer)) { text = T("duplicateLayer", "Duplicate Layer") };
            addPaintLayerButton.style.marginRight = 4f;
            addImageLayerButton.style.marginRight = 4f;
            duplicateLayerButton.style.marginRight = 4f;
            editRow.Add(addPaintLayerButton);
            editRow.Add(addImageLayerButton);
            editRow.Add(duplicateLayerButton);
            section.Add(editRow);

            VisualElement orderRow = new VisualElement();
            orderRow.style.flexDirection = FlexDirection.Row;
            orderRow.style.flexWrap = Wrap.Wrap;
            orderRow.style.marginTop = 4f;
            moveLayerUpButton = new Button(() => RunUiToolkitAction(MoveActiveLayerUp)) { text = T("moveUp", "Move Up") };
            moveLayerDownButton = new Button(() => RunUiToolkitAction(MoveActiveLayerDown)) { text = T("moveDown", "Move Down") };
            deleteLayerButton = new Button(() => RunUiToolkitAction(RemoveActiveLayer)) { text = T("deleteLayer", "Delete Layer") };
            moveLayerUpButton.style.marginRight = 4f;
            moveLayerDownButton.style.marginRight = 4f;
            deleteLayerButton.style.marginRight = 4f;
            orderRow.Add(moveLayerUpButton);
            orderRow.Add(moveLayerDownButton);
            orderRow.Add(deleteLayerButton);
            section.Add(orderRow);
            layerListElement = new ScrollView(ScrollViewMode.Vertical) { name = "layer-scroll-view" };
            layerListElement.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            layerListElement.style.height = LayerListHeight;
            section.Add(layerListElement);
            return section;
        }

        private VisualElement BuildBatchSection()
        {
            VisualElement section = CreateUiSection("batch-export-section", T("batchExport", "Batch Export"));
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            TextField folder = CreateTextField("batch-source-folder-field", T("sourceFolder", "Source Folder"), batchSourceFolder, value => batchSourceFolder = value);
            folder.style.flexGrow = 1f;
            row.Add(folder);
            row.Add(new Button(() => RunUiToolkitAction(SelectBatchSourceFolder)) { text = "..." });
            section.Add(row);
            Button exportButton = new Button(() => RunUiToolkitAction(ExportBatchSourceFolder)) { text = T("exportFolder", "Export Folder") };
            exportButton.SetEnabled(session.colorGroups.Count > 0 && session.variations.Count > 0);
            section.Add(exportButton);
            return section;
        }

        private VisualElement BuildPresetSection()
        {
            VisualElement section = CreateUiSection("preset-section", T("presetAsset", "Preset Asset"));
            ObjectField presetField = new ObjectField(T("presetAsset", "Preset Asset"))
            {
                name = "preset-object-field",
                objectType = typeof(PaletteVariantRulePresetAsset),
                allowSceneObjects = false,
                value = presetAsset
            };
            presetField.RegisterValueChangedCallback(evt => presetAsset = evt.newValue as PaletteVariantRulePresetAsset);
            section.Add(presetField);
            section.Add(CreateButtonRow(
                (T("createPresetAsset", "Create Preset Asset"), () => CreateRulePresetAsset(), session.colorGroups.Count > 0),
                (T("updatePresetAsset", "Update Preset Asset"), () => UpdateRulePresetAsset(), presetAsset != null && session.colorGroups.Count > 0),
                (T("loadPresetAsset", "Load Preset Asset"), () => LoadRulePresetAsset(), presetAsset != null)));
            section.Add(CreateButtonRow(
                (T("exportPreset", "Export Preset"), () => ExportRulePreset(), session.colorGroups.Count > 0),
                (T("importPreset", "Import Preset"), () => ImportRulePreset(), true)));
            return section;
        }

        private VisualElement BuildPreviewSection()
        {
            VisualElement section = CreateUiSection("preview-section", T("preview", "Preview"));
            VisualElement miniToolbar = new VisualElement { name = "preview-mini-toolbar" };
            miniToolbar.style.flexDirection = FlexDirection.Row;
            miniToolbar.style.flexWrap = Wrap.Wrap;
            miniToolbar.style.marginBottom = 4f;
            miniToolbar.Add(CreateStatusBadge(T("clickPick", "Click: Pick"), BadgeNeutralColor));
            miniToolbar.Add(CreateStatusBadge(LocalizeEnumValue(previewInteractionMode), previewInteractionMode == PreviewInteractionMode.BrushSelect ? BadgeWarningColor : BadgeActiveColor));
            miniToolbar.Add(CreateStatusBadge(previewZoom > MinPreviewZoom ? T("dragPan", "Drag: Pan") : T("dragPanDisabled", "Drag Pan Off"), previewZoom > MinPreviewZoom && previewInteractionMode != PreviewInteractionMode.BrushSelect ? BadgeActiveColor : BadgeNeutralColor));
            miniToolbar.Add(CreateStatusBadge(selectionHighlightEnabled ? T("selectionHighlight", "Selection Highlight") : T("selectionHighlightOff", "Selection Off"), selectionHighlightEnabled ? BadgeActiveColor : BadgeNeutralColor));
            miniToolbar.Add(CreateStatusBadge(effectHighlightEnabled ? T("effectHighlight", "Effect Highlight") : T("effectHighlightOff", "Effect Off"), effectHighlightEnabled ? BadgeWarningColor : BadgeNeutralColor));
            miniToolbar.Add(CreateStatusBadge($"{T("zoom", "Zoom")} {previewZoom:0.##}x", BadgeNeutralColor));
            miniToolbar.Add(CreateStatusBadge(LocalizeEnumValue(previewCompareMode), previewCompareMode == PreviewCompareMode.Split ? BadgeActiveColor : BadgeNeutralColor));
            section.Add(miniToolbar);
            previewInteractionModePopup = CreateEnumField("preview-interaction-mode-popup", T("previewMode", "Preview Mode"), previewInteractionMode, value =>
            {
                ResetPreviewInteractionState();
                previewInteractionMode = (PreviewInteractionMode)value;
                RefreshUiToolkitContent();
            });
            section.Add(previewInteractionModePopup);
            section.Add(CreateSliderInt("preview-brush-size-slider", T("brushSize", "Brush Size"), previewBrushSize, 1, 33, value =>
            {
                previewBrushSize = Mathf.Max(1, value | 1);
                RefreshUiToolkitContent();
            }));
            previewCompareModePopup = CreateEnumField("compare-mode-popup", T("compareMode", "Compare"), previewCompareMode, value =>
            {
                ResetPreviewInteractionState();
                previewCompareMode = (PreviewCompareMode)value;
                RefreshUiToolkitContent();
            });
            section.Add(previewCompareModePopup);
            section.Add(CreateSlider("preview-zoom-slider", T("zoom", "Zoom"), previewZoom, MinPreviewZoom, MaxPreviewZoom, value =>
            {
                previewZoom = Mathf.Clamp(value, MinPreviewZoom, MaxPreviewZoom);
                ClampPreviewPan();
                RefreshUiToolkitContent();
            }));
            section.Add(CreateSlider("preview-split-slider", T("split", "Split"), previewSplit, 0f, 1f, value =>
            {
                previewSplit = value;
                RefreshUiToolkitContent();
            }));
            section.Add(CreateToggle("selection-highlight-toggle", T("selectionHighlight", "Selection Highlight"), selectionHighlightEnabled, value =>
            {
                selectionHighlightEnabled = value;
                EditorPrefs.SetBool(SelectionHighlightPrefsKey, selectionHighlightEnabled);
                InvalidateSelectionOverlay();
                RefreshUiToolkitContent();
            }));
            section.Add(CreateToggle("effect-highlight-toggle", T("effectHighlight", "Effect Highlight"), effectHighlightEnabled, value =>
            {
                effectHighlightEnabled = value;
                DestroyUiToolkitHighlightTextures();
                RefreshUiToolkitContent();
            }));

            VisualElement row = new VisualElement { name = "preview-image-row" };
            row.style.flexDirection = FlexDirection.Row;
            row.style.flexWrap = Wrap.NoWrap;
            row.style.alignItems = Align.Stretch;
            beforePreviewImage = CreatePreviewImage("before-preview-image");
            afterPreviewImage = CreatePreviewImage("after-preview-image");
            row.Add(beforePreviewImage);
            row.Add(afterPreviewImage);
            section.Add(row);
            section.Add(CreatePreviewResizeHandle());
            selectedColorInfoContainer = CreateSelectedColorInfoPanel();
            section.Add(selectedColorInfoContainer);
            section.Add(new Button(() =>
            {
                previewZoom = 1f;
                previewSplit = 0.5f;
                previewPan = Vector2.zero;
                RefreshUiToolkitContent();
            }) { text = T("resetView", "Reset View") });
            section.Add(new Button(() => RunUiToolkitAction(CreateRulesFromBrushSelection))
            {
                text = T("createRulesFromBrush", "Create Brush Rules")
            });
            section.Add(new Button(() => RunUiToolkitAction(ClearBrushSelection))
            {
                text = T("clearBrushSelection", "Clear Brush")
            });
            return section;
        }

        private VisualElement BuildPaletteSection()
        {
            VisualElement section = CreateUiSection("palette-section", T("palette", "Palette"));
            paletteListElement = new ScrollView(ScrollViewMode.Vertical) { name = "palette-scroll-view" };
            paletteListElement.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            paletteListElement.style.height = PaletteListHeight;
            section.Add(paletteListElement);
            return section;
        }

        private VisualElement BuildVariationSection()
        {
            VisualElement section = CreateUiSection("variation-section", T("variations", "Variations"));
            section.Add(CreateButtonRow(
                (T("add", "Add"), () => { variationService.AddVariation(session); variationService.SyncActiveVariation(session); }, session.colorGroups.Count > 0),
                (T("duplicate", "Duplicate"), () => { variationService.DuplicateActiveVariation(session); variationService.SyncActiveVariation(session); }, session.variations.Count > 0),
                (T("remove", "Remove"), () => { variationService.RemoveActiveVariation(session); variationService.SyncActiveVariation(session); }, session.variations.Count > 1)));
            variationListElement = new ScrollView(ScrollViewMode.Vertical) { name = "variation-scroll-view" };
            variationListElement.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            variationListElement.style.height = VariationListHeight;
            section.Add(variationListElement);
            return section;
        }

        private VisualElement BuildReplacementRuleSection()
        {
            replacementRuleContainer = CreateUiSection("replacement-rule-section", T("replacementRules", "Replacement Rules"));
            return replacementRuleContainer;
        }

        private VisualElement BuildColorRuleSection()
        {
            colorRuleContainer = CreateUiSection("color-rule-section", T("colorRules", "Color Rules"));
            return colorRuleContainer;
        }

        private void RefreshUiToolkitContent()
        {
            if (rootVisualElement.Q<VisualElement>(MainWindowRootName) == null)
            {
                return;
            }

            isRefreshingUiToolkit = true;
            if (reportLabel != null)
            {
                reportLabel.text = reportMessage;
            }

            if (sourcePathLabel != null)
            {
                sourcePathLabel.text = $"{T("assetPath", "Asset Path")}: {(string.IsNullOrWhiteSpace(sourceAssetPath) ? "-" : sourceAssetPath)}";
            }

            if (sourceSizeLabel != null)
            {
                sourceSizeLabel.text = readableSourceImage == null
                    ? $"{T("size", "Size")}: -"
                    : $"{T("size", "Size")}: {readableSourceImage.width} x {readableSourceImage.height}";
            }

            if (sourcePaletteLabel != null)
            {
                sourcePaletteLabel.text = $"{T("paletteColors", "Palette Colors")}: {session.paletteColors.Count} / {T("groups", "Groups")}: {session.colorGroups.Count}";
            }

            SyncInteractivePopupValues();

            if (beforePreviewImage != null)
            {
                beforePreviewImage.style.height = previewCanvasHeight;
                if (!ShouldUseSplitPreviewDisplay())
                {
                    DestroySplitPreviewTexture();
                }

                beforePreviewImage.image = GetPrimaryPreviewDisplayTexture();
                beforePreviewImage.style.display = DisplayStyle.Flex;
            }

            if (afterPreviewImage != null)
            {
                afterPreviewImage.style.height = previewCanvasHeight;
                afterPreviewImage.image = GetDisplayPreviewTexture(afterPreview, HighlightPreviewSlot.After);
                afterPreviewImage.style.display = ShouldShowSecondaryPreviewPane()
                    ? DisplayStyle.None
                    : DisplayStyle.Flex;
            }

            RefreshSelectedColorInfoElement();
            RefreshPaletteListElement();
            RefreshLayerListElement();
            RefreshLayerButtonStates();
            RefreshVariationListElement();
            RefreshReplacementRuleElement();
            RefreshColorRuleElement();
            RefreshToolbarButtonStates();
            isRefreshingUiToolkit = false;
        }

        private void RefreshToolbarButtonStates()
        {
            Toolbar toolbar = rootVisualElement.Q<Toolbar>("main-toolbar");
            if (toolbar == null)
            {
                return;
            }

            foreach (Button button in toolbar.Children().OfType<Button>())
            {
                switch (button.text)
                {
                    case var text when text == T("analyze", "Analyze"):
                        button.SetEnabled(sourceImage != null);
                        break;
                    case var text when text == T("autoGroup", "Auto Group"):
                        button.SetEnabled(session.paletteColors.Count > 0);
                        break;
                    case var text when text == T("preview", "Preview"):
                        button.SetEnabled(readableSourceImage != null && session.colorGroups.Count > 0);
                        break;
                    case var text when text == T("export", "Export"):
                        button.SetEnabled(afterPreview != null);
                        break;
                    case var text when text == T("exportAll", "Export All"):
                        button.SetEnabled(readableSourceImage != null && session.variations.Count > 0);
                        break;
                    case var text when text == T("undo", "Undo"):
                        button.SetEnabled(undoSnapshots.Count > 0);
                        break;
                    case var text when text == T("redo", "Redo"):
                        button.SetEnabled(redoSnapshots.Count > 0);
                        break;
                }
            }
        }

        private void RefreshSelectedColorInfoElement()
        {
            if (selectedColorInfoContainer == null)
            {
                return;
            }

            selectedColorInfoContainer.Clear();
            selectedColorInfoContainer.Add(CreateUiSubHeader(T("selectedColorInfo", "Selected Color Info")));

            PaletteColorEntry entry = GetSelectedPaletteEntry();
            if (entry == null)
            {
                selectedColorInfoContainer.Add(CreateWrappingLabel(T("noSelectedColor", "No palette color selected.")));
                return;
            }

            foreach (KeyValuePair<string, string> row in BuildSelectedColorInfoRows(entry))
            {
                selectedColorInfoContainer.Add(CreateInfoRow(row.Key, row.Value));
            }
        }

        private List<KeyValuePair<string, string>> BuildSelectedColorInfoRows(PaletteColorEntry entry)
        {
            List<KeyValuePair<string, string>> rows = new List<KeyValuePair<string, string>>();
            if (entry == null)
            {
                return rows;
            }

            int totalPixels = Mathf.Max(1, session.paletteColors.Sum(candidate => candidate == null ? 0 : candidate.pixelCount));
            ColorGroup group = session.colorGroups.FirstOrDefault(candidate => candidate != null && candidate.id == entry.groupId);
            ColorReplacementRule rule = FindColorRule(entry);
            bool ruleEnabled = rule != null && rule.enabled;
            bool canUseColorRule = group != null
                && (group.replacementMode == ColorReplacementMode.PerColor || group.replacementMode == ColorReplacementMode.Hybrid);
            bool usesColorRule = ruleEnabled && canUseColorRule;
            Color32 replacementColor = usesColorRule
                ? rule.targetColor
                : group?.targetColor ?? entry.color;
            float blendRatio = usesColorRule
                ? rule.blendRatio
                : group?.blendRatio ?? 1f;
            string ruleState = ruleEnabled
                ? T("enabled", "Enabled")
                : T("disabled", "Disabled");
            if (ruleEnabled && !canUseColorRule)
            {
                ruleState = $"{ruleState} ({T("notAppliedByMode", "not applied in current mode")})";
            }

            rows.Add(new KeyValuePair<string, string>("HEX", FormatHexWithAlpha(entry.color)));
            rows.Add(new KeyValuePair<string, string>("RGBA", $"R {entry.color.r} / G {entry.color.g} / B {entry.color.b} / A {entry.color.a}"));
            rows.Add(new KeyValuePair<string, string>(T("group", "Group"), group == null ? "-" : group.displayName));
            rows.Add(new KeyValuePair<string, string>(T("pixelCount", "Pixel Count"), $"{entry.pixelCount} px / {(entry.pixelCount / (float)totalPixels):P1}"));
            rows.Add(new KeyValuePair<string, string>(T("replacementColor", "Replacement Color"), $"{FormatHexWithAlpha(replacementColor)} / {T("blendRatio", "Blend Ratio")} {blendRatio:0.##}"));
            rows.Add(new KeyValuePair<string, string>(T("colorRuleState", "Color Rule"), ruleState));
            rows.Add(new KeyValuePair<string, string>(T("transparentReplacement", "Transparent Replacement"), replacementColor.a == 0 ? T("yes", "Yes") : T("no", "No")));
            return rows;
        }

        private void RefreshPaletteListElement()
        {
            if (paletteListElement == null)
            {
                return;
            }

            paletteListElement.Clear();
            if (session.paletteColors.Count == 0)
            {
                paletteListElement.Add(CreateWrappingLabel(T("noPalette", "No palette colors. Analyze a source image first.")));
                return;
            }

            int totalPixels = Mathf.Max(1, session.paletteColors.Sum(entry => entry == null ? 0 : entry.pixelCount));
            foreach (PaletteColorEntry entry in session.paletteColors)
            {
                if (entry == null)
                {
                    continue;
                }

                bool isBrushSelected = brushedColorEntryIds.Contains(entry.id);
                VisualElement row = CreateSelectableRow(entry.id == selectedColorEntryId || isBrushSelected);
                row.Add(CreateSwatch(entry.color));
                Label infoLabel = CreateWrappingLabel($"{entry.hex}  {entry.pixelCount} px  {(entry.pixelCount / (float)totalPixels):P1}");
                infoLabel.style.flexGrow = 1f;
                infoLabel.style.minWidth = 120f;
                row.Add(infoLabel);
                row.Add(CreatePaletteRuleBadgeRow(entry));
                if (isBrushSelected)
                {
                    row.Add(CreateStatusBadge(T("brushSelected", "Brush"), BadgeWarningColor));
                }
                row.RegisterCallback<PointerDownEvent>(_ =>
                {
                    SelectPaletteEntry(entry, "Palette");
                });
                paletteListElement.Add(row);
            }
        }

        private void RefreshLayerListElement()
        {
            if (layerListElement == null)
            {
                return;
            }

            EnsureLayerSessionState();
            layerListElement.Clear();
            if (session.layers.Count == 0)
            {
                layerListElement.Add(CreateWrappingLabel("No layers. Add a paint or image layer."));
                return;
            }

            foreach (RasterLayer layer in session.layers.AsEnumerable().Reverse())
            {
                if (layer == null)
                {
                    continue;
                }

                VisualElement row = CreateSelectableRow(layer.id == session.activeLayerId);
                row.style.alignItems = Align.Center;
                Toggle visibleToggle = new Toggle { value = layer.visible };
                visibleToggle.RegisterValueChangedCallback(evt =>
                {
                    layer.visible = evt.newValue;
                    RebuildCompositePreview();
                    RefreshUiToolkitContent();
                });
                row.Add(visibleToggle);

                Toggle lockToggle = new Toggle { value = layer.locked };
                lockToggle.RegisterValueChangedCallback(evt => layer.locked = evt.newValue);
                row.Add(lockToggle);

                TextField nameField = new TextField { value = layer.displayName };
                nameField.style.minWidth = 110f;
                nameField.RegisterValueChangedCallback(evt => layer.displayName = evt.newValue);
                row.Add(nameField);

                Slider opacitySlider = new Slider(0f, 1f) { value = layer.opacity };
                opacitySlider.style.minWidth = 90f;
                opacitySlider.RegisterValueChangedCallback(evt =>
                {
                    layer.opacity = evt.newValue;
                    RebuildCompositePreview();
                    RefreshUiToolkitContent();
                });
                row.Add(opacitySlider);

                row.RegisterCallback<PointerDownEvent>(_ =>
                {
                    session.activeLayerId = layer.id;
                    RefreshUiToolkitContent();
                });

                layerListElement.Add(row);
            }
        }

        private void RefreshLayerButtonStates()
        {
            bool canAddLayer = GetCanvasWidth() > 0 && GetCanvasHeight() > 0;
            bool hasActiveLayer = GetActiveLayer() != null;
            if (addPaintLayerButton != null)
            {
                addPaintLayerButton.SetEnabled(canAddLayer);
            }

            if (addImageLayerButton != null)
            {
                addImageLayerButton.SetEnabled(canAddLayer);
            }

            if (duplicateLayerButton != null)
            {
                duplicateLayerButton.SetEnabled(hasActiveLayer);
            }

            if (moveLayerUpButton != null)
            {
                moveLayerUpButton.SetEnabled(CanMoveActiveLayer(1));
            }

            if (moveLayerDownButton != null)
            {
                moveLayerDownButton.SetEnabled(CanMoveActiveLayer(-1));
            }

            if (deleteLayerButton != null)
            {
                deleteLayerButton.SetEnabled(hasActiveLayer);
            }
        }

        private void RefreshGroupListElement()
        {
            if (groupListElement == null)
            {
                return;
            }

            groupListElement.Clear();
            if (session.colorGroups.Count == 0)
            {
                groupListElement.Add(CreateWrappingLabel(T("noGroups", "No color groups. Run Auto Group first.")));
                return;
            }

            foreach (ColorGroup group in session.colorGroups)
            {
                VisualElement row = CreateSelectableRow(group.id == selectedGroupId);
                row.Add(CreateSwatch(group.representativeColor));
                row.Add(CreateWrappingLabel($"{group.displayName}  {group.colorEntryIds.Count} colors  {group.pixelRatio:P1}"));
                row.RegisterCallback<PointerDownEvent>(_ =>
                {
                    selectedGroupId = group.id;
                    selectedColorEntryId = string.Empty;
                    RefreshUiToolkitContent();
                });
                groupListElement.Add(row);
            }
        }

        private void RefreshVariationListElement()
        {
            if (variationListElement == null)
            {
                return;
            }

            variationListElement.Clear();
            if (session.variations.Count == 0)
            {
                variationListElement.Add(CreateWrappingLabel(T("noVariations", "No variations. Run Auto Group first.")));
                return;
            }

            foreach (IconVariation variation in session.variations)
            {
                VisualElement row = CreateSelectableRow(variation.id == session.activeVariationId);
                row.style.alignItems = Align.Center;
                Toggle exportToggle = new Toggle { value = variation.exportEnabled };
                exportToggle.RegisterValueChangedCallback(evt => variation.exportEnabled = evt.newValue);
                row.Add(exportToggle);
                TextField nameField = new TextField { value = variation.displayName };
                nameField.style.minWidth = 130f;
                nameField.RegisterValueChangedCallback(evt => variation.displayName = evt.newValue);
                row.Add(nameField);
                TextField suffixField = new TextField { value = variation.fileSuffix };
                suffixField.style.minWidth = 90f;
                suffixField.RegisterValueChangedCallback(evt =>
                {
                    variation.fileSuffix = evt.newValue;
                    if (variation.id == session.activeVariationId)
                    {
                        session.exportSettings.fileSuffix = evt.newValue;
                    }
                });
                row.Add(suffixField);
                row.RegisterCallback<PointerDownEvent>(_ =>
                {
                    variationService.SyncActiveVariation(session);
                    variationService.ApplyVariation(session, variation.id);
                    session.exportSettings.fileSuffix = variation.fileSuffix;
                    RefreshUiToolkitContent();
                });
                variationListElement.Add(row);
            }
        }

        private void RefreshReplacementRuleElement()
        {
            if (replacementRuleContainer == null)
            {
                return;
            }

            replacementRuleContainer.Clear();
            replacementRuleContainer.Add(CreateUiSubHeader(T("replacementRules", "Replacement Rules")));
            groupListElement = new ScrollView(ScrollViewMode.Vertical) { name = "group-scroll-view" };
            groupListElement.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            groupListElement.style.maxHeight = 220f;
            replacementRuleContainer.Add(groupListElement);
            RefreshGroupListElement();

            ColorGroup group = session.colorGroups.FirstOrDefault(candidate => candidate != null && candidate.id == selectedGroupId)
                ?? session.colorGroups.FirstOrDefault();
            if (group == null)
            {
                replacementRuleContainer.Add(CreateWrappingLabel(T("noGroups", "No color groups. Run Auto Group first.")));
                ColorField placeholder = new ColorField(T("targetColor", "Target Color")) { name = "group-color-field", value = Color.clear };
                placeholder.SetEnabled(false);
                replacementRuleContainer.Add(placeholder);
                return;
            }

            selectedGroupId = group.id;
            replacementRuleContainer.Add(CreateWrappingLabel($"{group.displayName} / {group.colorEntryIds.Count} colors"));
            replacementRuleContainer.Add(CreateColorField("group-color-field", T("targetColor", "Target Color"), group.targetColor, value =>
            {
                RecordSessionEdit(() => group.targetColor = ToColor32(value));
            }));
            replacementRuleContainer.Add(CreateSlider("group-blend-slider", T("blendRatio", "Blend Ratio"), group.blendRatio, 0f, 1f, value =>
            {
                RecordSessionEdit(() => group.blendRatio = value);
            }));
            replacementRuleContainer.Add(CreateEnumField("replacement-mode-popup", T("mode", "Mode"), group.replacementMode, value =>
            {
                RecordSessionEdit(() => group.replacementMode = (ColorReplacementMode)value);
            }));
            replacementRuleContainer.Add(CreateToggle("group-locked-toggle", T("locked", "Locked"), group.lockedGroup, value => RecordSessionEdit(() => group.lockedGroup = value)));
        }

        private void RefreshColorRuleElement()
        {
            if (colorRuleContainer == null)
            {
                return;
            }

            colorRuleContainer.Clear();
            colorRuleContainer.Add(CreateUiSubHeader(T("colorRules", "Color Rules")));
            PaletteColorEntry entry = session.paletteColors.FirstOrDefault(candidate => candidate != null && candidate.id == selectedColorEntryId);
            if (entry == null)
            {
                colorRuleContainer.Add(CreateWrappingLabel(T("selectPaletteColor", "Select a palette color to edit a per-color rule.")));
                ColorField placeholder = new ColorField(T("targetColor", "Target Color")) { name = "color-rule-color-field", value = Color.clear };
                placeholder.SetEnabled(false);
                colorRuleContainer.Add(placeholder);
                return;
            }

            ColorReplacementRule rule = GetOrCreateColorRule(entry);
            colorRuleContainer.Add(CreateWrappingLabel($"{entry.hex} / {entry.pixelCount} px"));
            colorRuleContainer.Add(CreateToggle("color-rule-visible-toggle", T("visibleInExport", "Visible in Export"), IsColorEntryVisible(rule), value =>
            {
                RecordSessionEdit(() => SetColorEntryVisible(entry, rule, value));
            }));
            colorRuleContainer.Add(CreateColorField("color-rule-color-field", T("targetColor", "Target Color"), rule.targetColor, value =>
            {
                RecordSessionEdit(() =>
                {
                    rule.targetColor = ToColor32(value);
                    rule.enabled = true;
                    EnsureGroupSupportsColorRule(entry);
                });
            }));
            colorRuleContainer.Add(CreateSlider("color-rule-blend-slider", T("blendRatio", "Blend Ratio"), rule.blendRatio, 0f, 1f, value =>
            {
                RecordSessionEdit(() => rule.blendRatio = value);
            }));
            colorRuleContainer.Add(CreateToggle("color-rule-enabled-toggle", T("enabled", "Enabled"), rule.enabled, value =>
            {
                RecordSessionEdit(() =>
                {
                    rule.enabled = value;
                    if (value)
                    {
                        EnsureGroupSupportsColorRule(entry);
                    }
                });
            }));
        }

        private static bool IsColorEntryVisible(ColorReplacementRule rule)
        {
            return rule == null || !rule.enabled || rule.targetColor.a > 0 || rule.blendRatio < 1f;
        }

        private void SetColorEntryVisible(PaletteColorEntry entry, ColorReplacementRule rule, bool visible)
        {
            if (entry == null || rule == null)
            {
                return;
            }

            rule.enabled = true;
            rule.blendRatio = 1f;
            rule.targetColor = visible
                ? new Color32(entry.color.r, entry.color.g, entry.color.b, 255)
                : new Color32(entry.color.r, entry.color.g, entry.color.b, 0);
            EnsureGroupSupportsColorRule(entry);
        }

        private RasterLayer GetActiveLayer()
        {
            EnsureLayerSessionState();
            return session.layers.FirstOrDefault(layer => layer != null && layer.id == session.activeLayerId);
        }

        private int GetCanvasWidth()
        {
            return replacementPreview != null ? replacementPreview.width : readableSourceImage != null ? readableSourceImage.width : 0;
        }

        private int GetCanvasHeight()
        {
            return replacementPreview != null ? replacementPreview.height : readableSourceImage != null ? readableSourceImage.height : 0;
        }

        private bool CanMoveActiveLayer(int delta)
        {
            RasterLayer activeLayer = GetActiveLayer();
            if (activeLayer == null)
            {
                return false;
            }

            int index = session.layers.FindIndex(layer => layer != null && layer.id == activeLayer.id);
            int targetIndex = index + delta;
            return index >= 0 && targetIndex >= 0 && targetIndex < session.layers.Count;
        }

        private void AddPaintLayer()
        {
            int width = GetCanvasWidth();
            int height = GetCanvasHeight();
            if (width <= 0 || height <= 0)
            {
                reportMessage = "Analyze and preview a source image before adding layers.";
                reportType = MessageType.Warning;
                return;
            }

            RecordSessionEdit(() =>
            {
                string id = Guid.NewGuid().ToString("N");
                session.layers.Add(layerCompositingService.CreatePaintLayer(id, $"Paint Layer {session.layers.Count + 1}", width, height));
                session.activeLayerId = id;
                RebuildCompositePreview();
            });
            reportMessage = "Paint layer added.";
            reportType = MessageType.Info;
        }

        private void AddImageLayer()
        {
            int width = GetCanvasWidth();
            int height = GetCanvasHeight();
            if (width <= 0 || height <= 0)
            {
                reportMessage = "Analyze and preview a source image before adding layers.";
                reportType = MessageType.Warning;
                return;
            }

            string path = EditorUtility.OpenFilePanel("Add Image Layer", Application.dataPath, "png,jpg,jpeg");
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            byte[] bytes = System.IO.File.ReadAllBytes(path);
            Texture2D tempTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                if (!ImageConversion.LoadImage(tempTexture, bytes))
                {
                    reportMessage = $"Could not load image layer: {path}";
                    reportType = MessageType.Error;
                    return;
                }

                RecordSessionEdit(() =>
                {
                    string id = Guid.NewGuid().ToString("N");
                    session.layers.Add(layerCompositingService.CreateImageLayer(id, System.IO.Path.GetFileNameWithoutExtension(path), tempTexture, path, width, height));
                    session.activeLayerId = id;
                    RebuildCompositePreview();
                });
                reportMessage = $"Image layer added: {System.IO.Path.GetFileName(path)}";
                reportType = MessageType.Info;
            }
            finally
            {
                DestroyImmediate(tempTexture);
            }
        }

        private void DuplicateActiveLayer()
        {
            RasterLayer activeLayer = GetActiveLayer();
            if (activeLayer == null)
            {
                return;
            }

            RecordSessionEdit(() =>
            {
                RasterLayer copy = new RasterLayer
                {
                    id = Guid.NewGuid().ToString("N"),
                    displayName = $"{activeLayer.displayName} Copy",
                    kind = activeLayer.kind,
                    visible = activeLayer.visible,
                    locked = activeLayer.locked,
                    opacity = activeLayer.opacity,
                    blendMode = activeLayer.blendMode,
                    offsetX = activeLayer.offsetX,
                    offsetY = activeLayer.offsetY,
                    sourceAssetPath = activeLayer.sourceAssetPath,
                    pixelData = new LayerPixelData
                    {
                        width = activeLayer.pixelData?.width ?? 0,
                        height = activeLayer.pixelData?.height ?? 0,
                        rgbaBytesBase64 = activeLayer.pixelData?.rgbaBytesBase64 ?? string.Empty
                    }
                };
                int index = session.layers.FindIndex(layer => layer != null && layer.id == activeLayer.id);
                session.layers.Insert(index + 1, copy);
                session.activeLayerId = copy.id;
                RebuildCompositePreview();
            });
        }

        private void RemoveActiveLayer()
        {
            RasterLayer activeLayer = GetActiveLayer();
            if (activeLayer == null)
            {
                return;
            }

            RecordSessionEdit(() =>
            {
                session.layers.RemoveAll(layer => layer != null && layer.id == activeLayer.id);
                session.activeLayerId = session.layers.LastOrDefault(layer => layer != null)?.id ?? string.Empty;
                RebuildCompositePreview();
            });
        }

        private void MoveActiveLayerUp()
        {
            MoveActiveLayer(1);
        }

        private void MoveActiveLayerDown()
        {
            MoveActiveLayer(-1);
        }

        private void MoveActiveLayer(int delta)
        {
            RasterLayer activeLayer = GetActiveLayer();
            if (activeLayer == null)
            {
                return;
            }

            RecordSessionEdit(() =>
            {
                int index = session.layers.FindIndex(layer => layer != null && layer.id == activeLayer.id);
                int targetIndex = index + delta;
                if (index < 0 || targetIndex < 0 || targetIndex >= session.layers.Count)
                {
                    return;
                }

                session.layers.RemoveAt(index);
                session.layers.Insert(targetIndex, activeLayer);
                RebuildCompositePreview();
            });
        }

        private void RebuildCompositePreview()
        {
            if (replacementPreview == null)
            {
                return;
            }

            if (afterPreview == null
                || afterPreview.width != replacementPreview.width
                || afterPreview.height != replacementPreview.height)
            {
                DestroyAfterPreview();
                afterPreview = layerCompositingService.Compose(replacementPreview, session.layers);
            }
            else
            {
                layerCompositingService.UpdateCompositeTexture(afterPreview, replacementPreview, session.layers);
                InvalidateAfterPreviewPresentationCaches();
            }

            UpdatePreviewImagesImmediately();
            Repaint();
        }

        private void RunUiToolkitAction(System.Action action)
        {
            action?.Invoke();
            RefreshUiToolkitContent();
        }

        private static VisualElement CreateUiSection(string name, string title)
        {
            VisualElement section = new VisualElement { name = name };
            section.style.borderTopWidth = 1f;
            section.style.borderRightWidth = 1f;
            section.style.borderBottomWidth = 1f;
            section.style.borderLeftWidth = 1f;
            section.style.borderTopColor = SeparatorColor;
            section.style.borderRightColor = SeparatorColor;
            section.style.borderBottomColor = SeparatorColor;
            section.style.borderLeftColor = SeparatorColor;
            section.style.borderTopLeftRadius = 4f;
            section.style.borderTopRightRadius = 4f;
            section.style.borderBottomLeftRadius = 4f;
            section.style.borderBottomRightRadius = 4f;
            section.style.paddingLeft = 8f;
            section.style.paddingRight = 8f;
            section.style.paddingTop = 6f;
            section.style.paddingBottom = 6f;
            section.style.marginRight = 8f;
            section.style.marginBottom = 8f;
            section.Add(CreateUiSubHeader(title));
            return section;
        }

        private static Label CreateUiSubHeader(string title)
        {
            Label label = new Label(title);
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginBottom = 4f;
            return label;
        }

        private static VisualElement CreateUiColumn(string name, float minWidth, float flexGrow)
        {
            VisualElement column = new VisualElement { name = name };
            column.style.minWidth = minWidth;
            column.style.flexBasis = 0f;
            column.style.flexGrow = flexGrow;
            column.style.marginRight = 8f;
            return column;
        }

        private VisualElement CreateCollapsibleSection(string name, string title, bool expanded, System.Action<bool> onExpandedChanged, System.Func<VisualElement> buildContent)
        {
            Foldout foldout = new Foldout { name = name, text = title, value = expanded };
            foldout.style.marginBottom = 4f;
            foldout.RegisterValueChangedCallback(evt => onExpandedChanged(evt.newValue));
            foldout.Add(buildContent());
            return foldout;
        }

        private static Label CreateWrappingLabel(string text)
        {
            Label label = new Label(text);
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.marginBottom = 2f;
            return label;
        }

        private static Label CreateWrappingLabel()
        {
            return CreateWrappingLabel(string.Empty);
        }

        private VisualElement CreateSelectedColorInfoPanel()
        {
            VisualElement panel = new VisualElement { name = "selected-color-info-panel" };
            panel.style.borderTopWidth = 1f;
            panel.style.borderRightWidth = 1f;
            panel.style.borderBottomWidth = 1f;
            panel.style.borderLeftWidth = 1f;
            panel.style.borderTopColor = SeparatorColor;
            panel.style.borderRightColor = SeparatorColor;
            panel.style.borderBottomColor = SeparatorColor;
            panel.style.borderLeftColor = SeparatorColor;
            panel.style.borderTopLeftRadius = 4f;
            panel.style.borderTopRightRadius = 4f;
            panel.style.borderBottomLeftRadius = 4f;
            panel.style.borderBottomRightRadius = 4f;
            panel.style.paddingLeft = 6f;
            panel.style.paddingRight = 6f;
            panel.style.paddingTop = 5f;
            panel.style.paddingBottom = 5f;
            panel.style.marginTop = 6f;
            panel.style.marginBottom = 6f;
            panel.style.minWidth = 0f;
            return panel;
        }

        private VisualElement CreateInfoRow(string labelText, string valueText)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.flexWrap = Wrap.Wrap;
            row.style.minWidth = 0f;

            Label label = CreateWrappingLabel(labelText);
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.minWidth = 92f;
            label.style.marginRight = 6f;
            row.Add(label);

            Label value = CreateWrappingLabel(valueText);
            value.style.flexGrow = 1f;
            value.style.minWidth = 0f;
            row.Add(value);
            return row;
        }

        private Image CreatePreviewImage(string name)
        {
            Image image = new Image { name = name, scaleMode = ScaleMode.ScaleToFit };
            image.style.height = previewCanvasHeight;
            image.style.minWidth = 0f;
            image.style.flexBasis = 0f;
            image.style.flexGrow = 1f;
            image.style.marginRight = 6f;
            image.style.backgroundColor = new Color(0.12f, 0.12f, 0.12f);
            image.RegisterCallback<PointerDownEvent>(evt => BeginPreviewDrag(image, evt));
            image.RegisterCallback<PointerMoveEvent>(HandlePreviewDrag);
            image.RegisterCallback<PointerUpEvent>(evt => EndPreviewDrag(image, evt));
            image.RegisterCallback<PointerCancelEvent>(_ => CancelPreviewDrag(image));
            return image;
        }

        private VisualElement CreatePreviewResizeHandle()
        {
            VisualElement handle = new VisualElement { name = "preview-resize-handle" };
            handle.style.height = 8f;
            handle.style.marginTop = 2f;
            handle.style.marginBottom = 6f;
            handle.style.backgroundColor = new Color(0.24f, 0.24f, 0.24f, 1f);
            handle.style.borderTopLeftRadius = 3f;
            handle.style.borderTopRightRadius = 3f;
            handle.style.borderBottomLeftRadius = 3f;
            handle.style.borderBottomRightRadius = 3f;
            handle.RegisterCallback<PointerDownEvent>(BeginPreviewResize);
            handle.RegisterCallback<PointerMoveEvent>(HandlePreviewResize);
            handle.RegisterCallback<PointerUpEvent>(EndPreviewResize);
            handle.RegisterCallback<PointerCancelEvent>(_ => CancelPreviewResize());
            return handle;
        }

        private Texture2D GetSplitPreviewTexture()
        {
            if (readableSourceImage == null)
            {
                return null;
            }

            string cacheKey = string.Join(
                "|",
                GetTextureCacheId(readableSourceImage),
                afterPreview == null ? "before" : GetTextureCacheId(afterPreview),
                previewSplit.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture));
            if (splitPreviewTexture != null && splitPreviewCacheKey == cacheKey)
            {
                return splitPreviewTexture;
            }

            DestroySplitPreviewTexture();
            splitPreviewCacheKey = cacheKey;
            Texture2D rightTexture = afterPreview != null ? afterPreview : readableSourceImage;
            int width = readableSourceImage.width;
            int height = readableSourceImage.height;
            Color32[] leftPixels = readableSourceImage.GetPixels32();
            Color32[] rightPixels = rightTexture.GetPixels32();
            Color32[] outputPixels = new Color32[leftPixels.Length];
            int splitX = Mathf.Clamp(Mathf.RoundToInt(width * previewSplit), 0, width);

            for (int y = 0; y < height; y++)
            {
                int rowOffset = y * width;
                for (int x = 0; x < width; x++)
                {
                    int index = rowOffset + x;
                    outputPixels[index] = x < splitX ? leftPixels[index] : rightPixels[index];
                }
            }

            splitPreviewTexture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = "PaletteVariantGenerator_SplitPreview",
                filterMode = readableSourceImage.filterMode,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            splitPreviewTexture.SetPixels32(outputPixels);
            splitPreviewTexture.Apply(false, false);
            return splitPreviewTexture;
        }

        private bool ShouldUseSourceEditingPreviewOnly()
        {
            return previewInteractionMode == PreviewInteractionMode.Paint
                && session?.drawingToolSettings != null
                && session.drawingToolSettings.paintTarget == PaintEditTarget.SourceImage;
        }

        private bool ShouldUseSplitPreviewDisplay()
        {
            return previewCompareMode == PreviewCompareMode.Split && !ShouldUseSourceEditingPreviewOnly();
        }

        private Texture2D GetPrimaryPreviewDisplayTexture()
        {
            if (ShouldUseSourceEditingPreviewOnly())
            {
                return GetDisplayPreviewTexture(GetSourceEditingPreviewTexture(), HighlightPreviewSlot.Before);
            }

            return previewCompareMode == PreviewCompareMode.Difference
                ? GetDisplayPreviewTexture(GetDiffPreviewTexture(), HighlightPreviewSlot.Split)
                : previewCompareMode == PreviewCompareMode.Split
                ? GetDisplayPreviewTexture(GetSplitPreviewTexture(), HighlightPreviewSlot.Split)
                : GetDisplayPreviewTexture(readableSourceImage, HighlightPreviewSlot.Before);
        }

        private bool ShouldShowSecondaryPreviewPane()
        {
            return ShouldUseSourceEditingPreviewOnly()
                || previewCompareMode == PreviewCompareMode.Split
                || previewCompareMode == PreviewCompareMode.Difference
                || afterPreview == null;
        }

        private Texture2D GetDiffPreviewTexture()
        {
            if (readableSourceImage == null || afterPreview == null)
            {
                return readableSourceImage;
            }

            string cacheKey = $"{GetTextureCacheId(readableSourceImage)}|{GetTextureCacheId(afterPreview)}|diff";
            if (diffPreviewTexture != null && diffPreviewKey == cacheKey)
            {
                return diffPreviewTexture;
            }

            DestroyDiffPreviewTexture();
            diffPreviewKey = cacheKey;
            Color32[] beforePixels = readableSourceImage.GetPixels32();
            Color32[] afterPixels = afterPreview.GetPixels32();
            Color32[] outputPixels = new Color32[beforePixels.Length];
            for (int index = 0; index < beforePixels.Length; index++)
            {
                Color32 before = beforePixels[index];
                Color32 after = afterPixels[index];
                bool changed = before.r != after.r || before.g != after.g || before.b != after.b || before.a != after.a;
                outputPixels[index] = changed
                    ? BlendHighlight(after, after.a == 0 ? EdgeEffectHighlightColor : BadgeActiveColor)
                    : new Color32((byte)(before.r / 3), (byte)(before.g / 3), (byte)(before.b / 3), (byte)Mathf.Max(48, before.a / 3));
            }

            diffPreviewTexture = new Texture2D(readableSourceImage.width, readableSourceImage.height, TextureFormat.RGBA32, false)
            {
                name = "PaletteVariantGenerator_DiffPreview",
                filterMode = readableSourceImage.filterMode,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            diffPreviewTexture.SetPixels32(outputPixels);
            diffPreviewTexture.Apply(false, false);
            return diffPreviewTexture;
        }

        private Texture2D GetSourceEditingPreviewTexture()
        {
            if (readableSourceImage == null)
            {
                return null;
            }

            string cacheKey = GetTextureCacheId(readableSourceImage);
            if (sourceEditingPreviewTexture != null && sourceEditingPreviewKey == cacheKey)
            {
                return sourceEditingPreviewTexture;
            }

            DestroyTexture(ref sourceEditingPreviewTexture);
            sourceEditingPreviewKey = cacheKey;
            Color32[] sourcePixels = readableSourceImage.GetPixels32();
            Color32[] outputPixels = new Color32[sourcePixels.Length];
            int width = readableSourceImage.width;
            int height = readableSourceImage.height;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = (y * width) + x;
                    outputPixels[index] = BlendWithCheckerboard(sourcePixels[index], x, y);
                }
            }

            sourceEditingPreviewTexture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = "PaletteVariantGenerator_SourceEditingPreview",
                filterMode = readableSourceImage.filterMode,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            sourceEditingPreviewTexture.SetPixels32(outputPixels);
            sourceEditingPreviewTexture.Apply(false, false);
            return sourceEditingPreviewTexture;
        }

        private static Color32 BlendWithCheckerboard(Color32 foreground, int x, int y)
        {
            bool light = ((x / 4) + (y / 4)) % 2 == 0;
            Color background = light ? new Color(0.72f, 0.72f, 0.72f, 1f) : new Color(0.48f, 0.48f, 0.48f, 1f);
            float alpha = foreground.a / 255f;
            return new Color32(
                (byte)Mathf.RoundToInt(Mathf.Lerp(background.r * 255f, foreground.r, alpha)),
                (byte)Mathf.RoundToInt(Mathf.Lerp(background.g * 255f, foreground.g, alpha)),
                (byte)Mathf.RoundToInt(Mathf.Lerp(background.b * 255f, foreground.b, alpha)),
                255);
        }

        private void DestroySplitPreviewTexture()
        {
            if (splitPreviewTexture == null)
            {
                return;
            }

            DestroyImmediate(splitPreviewTexture);
            splitPreviewTexture = null;
            splitPreviewCacheKey = string.Empty;
        }

        private void DestroyDiffPreviewTexture()
        {
            if (diffPreviewTexture == null)
            {
                return;
            }

            DestroyImmediate(diffPreviewTexture);
            diffPreviewTexture = null;
            diffPreviewKey = string.Empty;
        }

        private Texture2D GetDisplayPreviewTexture(Texture2D baseTexture, HighlightPreviewSlot slot)
        {
            if (baseTexture == null || readableSourceImage == null)
            {
                return baseTexture;
            }

            HashSet<uint> selectedKeys = GetSelectedColorKeys();
            HashSet<int> noiseEffectIndices = GetNoiseEffectHighlightIndices();
            HashSet<int> edgeEffectIndices = GetEdgeEffectHighlightIndices();
            bool applySelectionHighlight = selectionHighlightEnabled && selectedKeys.Count > 0;
            bool applyEffectHighlight = effectHighlightEnabled
                && slot != HighlightPreviewSlot.Before
                && (noiseEffectIndices.Count > 0 || edgeEffectIndices.Count > 0);
            bool applyHighlight = applySelectionHighlight || applyEffectHighlight;
            bool applyZoom = !Mathf.Approximately(Mathf.Clamp(previewZoom, MinPreviewZoom, MaxPreviewZoom), MinPreviewZoom)
                || previewPan != Vector2.zero;
            if (!applyHighlight && !applyZoom)
            {
                return baseTexture;
            }

            string cacheKey = BuildDisplayPreviewCacheKey(baseTexture, slot, applyHighlight, applyZoom, selectedKeys, noiseEffectIndices, edgeEffectIndices);
            Texture2D cached = GetCachedDisplayPreviewTexture(slot, applyHighlight, cacheKey);
            if (cached != null)
            {
                return cached;
            }

            Color32[] basePixels = baseTexture.GetPixels32();
            Color32[] sourcePixels = readableSourceImage.GetPixels32();
            if (basePixels.Length != sourcePixels.Length)
            {
                return baseTexture;
            }

            Color32[] outputPixels = new Color32[basePixels.Length];
            int quantizeStep = Mathf.Clamp(session.analyzeSettings.quantizeStep, 1, 64);
            int alphaThreshold = Mathf.Clamp(session.analyzeSettings.alphaThreshold, 0, 255);
            Rect texCoords = GetPreviewTexCoords(previewZoom, previewPan);
            int width = baseTexture.width;
            int height = baseTexture.height;

            for (int y = 0; y < height; y++)
            {
                float v = texCoords.yMin + (((y + 0.5f) / height) * texCoords.height);
                int sourceY = Mathf.Clamp(Mathf.FloorToInt(v * height), 0, height - 1);
                for (int x = 0; x < width; x++)
                {
                    float u = texCoords.xMin + (((x + 0.5f) / width) * texCoords.width);
                    int sourceX = Mathf.Clamp(Mathf.FloorToInt(u * width), 0, width - 1);
                    int sourceIndex = (sourceY * width) + sourceX;
                    int outputIndex = (y * width) + x;
                    outputPixels[outputIndex] = basePixels[sourceIndex];

                    if (applySelectionHighlight)
                    {
                        Color32 sourcePixel = sourcePixels[sourceIndex];
                        if (sourcePixel.a > alphaThreshold)
                        {
                            uint key = ColorCodeUtility.ToRgbKey(colorQuantizationService.Quantize(sourcePixel, quantizeStep));
                            if (selectedKeys.Contains(key))
                            {
                                outputPixels[outputIndex] = BlendHighlight(outputPixels[outputIndex], OverlayColor);
                            }
                        }
                    }

                    if (!applyEffectHighlight)
                    {
                        continue;
                    }

                    if (edgeEffectIndices.Contains(sourceIndex))
                    {
                        outputPixels[outputIndex] = BlendHighlight(outputPixels[outputIndex], EdgeEffectHighlightColor);
                    }
                    else if (noiseEffectIndices.Contains(sourceIndex))
                    {
                        outputPixels[outputIndex] = BlendHighlight(outputPixels[outputIndex], NoiseEffectHighlightColor);
                    }
                }
            }

            Texture2D texture = new Texture2D(baseTexture.width, baseTexture.height, TextureFormat.RGBA32, false)
            {
                name = $"{baseTexture.name}_PreviewDisplay",
                filterMode = baseTexture.filterMode,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.SetPixels32(outputPixels);
            texture.Apply(false, true);
            SetDisplayPreviewTexture(slot, texture, applyHighlight, cacheKey);
            return texture;
        }

        private HashSet<int> GetNoiseEffectHighlightIndices()
        {
            return new HashSet<int>(lastNoiseEffectHighlightIndices ?? new List<int>());
        }

        private HashSet<int> GetEdgeEffectHighlightIndices()
        {
            return new HashSet<int>(lastEdgeEffectHighlightIndices ?? new List<int>());
        }

        private string BuildDisplayPreviewCacheKey(
            Texture2D baseTexture,
            HighlightPreviewSlot slot,
            bool highlighted,
            bool zoomed,
            HashSet<uint> selectedKeys,
            HashSet<int> noiseEffectIndices,
            HashSet<int> edgeEffectIndices)
        {
            return string.Join(
                "|",
                slot.ToString(),
                GetTextureCacheId(baseTexture),
                GetTextureCacheId(readableSourceImage),
                highlighted ? "highlight" : "plain",
                zoomed ? "zoom" : "nozoom",
                previewZoom.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture),
                previewPan.x.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture),
                previewPan.y.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture),
                previewSplit.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture),
                selectionHighlightEnabled.ToString(),
                selectedGroupId,
                selectedColorEntryId,
                effectHighlightEnabled.ToString(),
                session.analyzeSettings.alphaThreshold.ToString(),
                session.analyzeSettings.quantizeStep.ToString(),
                string.Join(",", noiseEffectIndices.OrderBy(index => index)),
                string.Join(",", edgeEffectIndices.OrderBy(index => index)),
                string.Join(",", selectedKeys.OrderBy(key => key)));
        }

        private Texture2D GetCachedDisplayPreviewTexture(HighlightPreviewSlot slot, bool highlighted, string cacheKey)
        {
            switch (slot)
            {
                case HighlightPreviewSlot.Before:
                    return highlighted
                        ? highlightedBeforePreviewKey == cacheKey ? highlightedBeforePreviewTexture : null
                        : zoomedBeforePreviewKey == cacheKey ? zoomedBeforePreviewTexture : null;
                case HighlightPreviewSlot.After:
                    return highlighted
                        ? highlightedAfterPreviewKey == cacheKey ? highlightedAfterPreviewTexture : null
                        : zoomedAfterPreviewKey == cacheKey ? zoomedAfterPreviewTexture : null;
                case HighlightPreviewSlot.Split:
                    return highlighted
                        ? highlightedSplitPreviewKey == cacheKey ? highlightedSplitPreviewTexture : null
                        : zoomedSplitPreviewKey == cacheKey ? zoomedSplitPreviewTexture : null;
                default:
                    return null;
            }
        }

        private void SetDisplayPreviewTexture(HighlightPreviewSlot slot, Texture2D texture, bool highlighted, string cacheKey)
        {
            switch (slot)
            {
                case HighlightPreviewSlot.Before:
                    if (highlighted)
                    {
                        DestroyTexture(ref highlightedBeforePreviewTexture);
                        highlightedBeforePreviewTexture = texture;
                        highlightedBeforePreviewKey = cacheKey;
                    }
                    else
                    {
                        DestroyTexture(ref zoomedBeforePreviewTexture);
                        zoomedBeforePreviewTexture = texture;
                        zoomedBeforePreviewKey = cacheKey;
                    }
                    break;
                case HighlightPreviewSlot.After:
                    if (highlighted)
                    {
                        DestroyTexture(ref highlightedAfterPreviewTexture);
                        highlightedAfterPreviewTexture = texture;
                        highlightedAfterPreviewKey = cacheKey;
                    }
                    else
                    {
                        DestroyTexture(ref zoomedAfterPreviewTexture);
                        zoomedAfterPreviewTexture = texture;
                        zoomedAfterPreviewKey = cacheKey;
                    }
                    break;
                case HighlightPreviewSlot.Split:
                    if (highlighted)
                    {
                        DestroyTexture(ref highlightedSplitPreviewTexture);
                        highlightedSplitPreviewTexture = texture;
                        highlightedSplitPreviewKey = cacheKey;
                    }
                    else
                    {
                        DestroyTexture(ref zoomedSplitPreviewTexture);
                        zoomedSplitPreviewTexture = texture;
                        zoomedSplitPreviewKey = cacheKey;
                    }
                    break;
            }
        }

        private static Color32 BlendHighlight(Color32 baseColor, Color overlay)
        {
            float ratio = Mathf.Clamp01(overlay.a);
            byte r = (byte)Mathf.RoundToInt(Mathf.Lerp(baseColor.r, overlay.r * 255f, ratio));
            byte g = (byte)Mathf.RoundToInt(Mathf.Lerp(baseColor.g, overlay.g * 255f, ratio));
            byte b = (byte)Mathf.RoundToInt(Mathf.Lerp(baseColor.b, overlay.b * 255f, ratio));
            byte a = (byte)Mathf.Max(baseColor.a, Mathf.RoundToInt(overlay.a * 255f));
            return new Color32(r, g, b, a);
        }

        private void DestroyUiToolkitHighlightTextures()
        {
            DestroyTexture(ref highlightedBeforePreviewTexture);
            DestroyTexture(ref highlightedAfterPreviewTexture);
            DestroyTexture(ref highlightedSplitPreviewTexture);
            DestroyTexture(ref zoomedBeforePreviewTexture);
            DestroyTexture(ref zoomedAfterPreviewTexture);
            DestroyTexture(ref zoomedSplitPreviewTexture);
            highlightedBeforePreviewKey = string.Empty;
            highlightedAfterPreviewKey = string.Empty;
            highlightedSplitPreviewKey = string.Empty;
            zoomedBeforePreviewKey = string.Empty;
            zoomedAfterPreviewKey = string.Empty;
            zoomedSplitPreviewKey = string.Empty;
            DestroyDiffPreviewTexture();
        }

        private static void DestroyTexture(ref Texture2D texture)
        {
            if (texture == null)
            {
                return;
            }

            DestroyImmediate(texture);
            texture = null;
        }

        private static string GetTextureCacheId(Texture2D texture)
        {
            return texture == null ? "none" : texture.GetEntityId().ToString();
        }

        private void BeginPreviewDrag(Image image, PointerDownEvent evt)
        {
            if (evt.button != 0)
            {
                return;
            }

            if (previewInteractionMode == PreviewInteractionMode.Paint)
            {
                if (!CanPaintSelectedTarget())
                {
                    evt.StopPropagation();
                    return;
                }

                BeginPaintStroke();
                previewPaintActive = true;
                previewDragPointerId = evt.pointerId;
                image.CapturePointer(evt.pointerId);
                ApplyActivePaintToolFromPreview(image, evt.localPosition);
                evt.StopPropagation();
                return;
            }

            if (previewInteractionMode == PreviewInteractionMode.BrushSelect)
            {
                previewBrushActive = true;
                previewDragPointerId = evt.pointerId;
                image.CapturePointer(evt.pointerId);
                AddBrushSelectionFromPreview(image, evt.localPosition);
                evt.StopPropagation();
                return;
            }

            previewDragActive = true;
            previewDragMoved = false;
            previewDragPointerId = evt.pointerId;
            previewDragStartPosition = evt.localPosition;
            previewDragStartPan = previewPan;
            image.CapturePointer(evt.pointerId);
            evt.StopPropagation();
        }

        private void HandlePreviewDrag(PointerMoveEvent evt)
        {
            if (previewPaintActive && evt.pointerId == previewDragPointerId && evt.currentTarget is Image paintImage)
            {
                ApplyActivePaintToolFromPreview(paintImage, evt.localPosition);
                evt.StopPropagation();
                return;
            }

            if (previewBrushActive && evt.pointerId == previewDragPointerId && evt.currentTarget is Image brushImage)
            {
                AddBrushSelectionFromPreview(brushImage, evt.localPosition);
                evt.StopPropagation();
                return;
            }

            if (!previewDragActive || evt.pointerId != previewDragPointerId || previewZoom <= MinPreviewZoom)
            {
                return;
            }

            if (!(evt.currentTarget is Image image))
            {
                return;
            }

            Vector2 currentPosition = new Vector2(evt.localPosition.x, evt.localPosition.y);
            Vector2 delta = currentPosition - previewDragStartPosition;
            if (delta.sqrMagnitude < 9f)
            {
                return;
            }

            previewDragMoved = true;
            ApplyPreviewPanDrag(delta, image.resolvedStyle.width, image.resolvedStyle.height);
            RefreshUiToolkitContent();
            evt.StopPropagation();
        }

        private void EndPreviewDrag(Image image, PointerUpEvent evt)
        {
            if (previewPaintActive && evt.pointerId == previewDragPointerId)
            {
                ApplyActivePaintToolFromPreview(image, evt.localPosition);
                CommitPaintStroke();
                previewPaintActive = false;
                previewDragPointerId = -1;
                image.ReleasePointer(evt.pointerId);
                RefreshUiToolkitContent();
                evt.StopPropagation();
                return;
            }

            if (previewBrushActive && evt.pointerId == previewDragPointerId)
            {
                AddBrushSelectionFromPreview(image, evt.localPosition);
                previewBrushActive = false;
                previewDragPointerId = -1;
                image.ReleasePointer(evt.pointerId);
                reportMessage = $"Brush selected {brushedColorEntryIds.Count} palette color(s).";
                reportType = MessageType.Info;
                RefreshUiToolkitContent();
                evt.StopPropagation();
                return;
            }

            if (!previewDragActive || evt.pointerId != previewDragPointerId)
            {
                return;
            }

            bool shouldPick = !previewDragMoved && previewInteractionMode == PreviewInteractionMode.Pick;
            CancelPreviewDrag(image);
            image.ReleasePointer(evt.pointerId);
            if (shouldPick)
            {
                PickPaletteColorFromPreview(image, evt.localPosition);
            }

            evt.StopPropagation();
        }

        private void CancelPreviewDrag(Image image = null)
        {
            if (previewPaintActive)
            {
                CommitPaintStroke();
            }

            if (image != null && previewDragPointerId >= 0 && image.HasPointerCapture(previewDragPointerId))
            {
                image.ReleasePointer(previewDragPointerId);
            }

            previewPaintActive = false;
            previewBrushActive = false;
            previewDragActive = false;
            previewDragMoved = false;
            previewDragPointerId = -1;
        }

        private void ResetPreviewInteractionState()
        {
            ReleasePreviewPointerCapture(beforePreviewImage);
            ReleasePreviewPointerCapture(afterPreviewImage);
            CancelPreviewDrag();
            CancelPreviewResize();
            previewBrushActive = false;
        }

        private void BeginPreviewResize(PointerDownEvent evt)
        {
            if (evt.button != 0 || !(evt.currentTarget is VisualElement handle))
            {
                return;
            }

            previewResizeActive = true;
            previewDragPointerId = evt.pointerId;
            previewResizeStartPosition = evt.position;
            previewResizeStartHeight = previewCanvasHeight;
            handle.CapturePointer(evt.pointerId);
            evt.StopPropagation();
        }

        private void HandlePreviewResize(PointerMoveEvent evt)
        {
            if (!previewResizeActive || evt.pointerId != previewDragPointerId)
            {
                return;
            }

            float nextHeight = Mathf.Clamp(previewResizeStartHeight + (evt.position.y - previewResizeStartPosition.y), MinPreviewCanvasHeight, MaxPreviewCanvasHeight);
            if (Mathf.Approximately(nextHeight, previewCanvasHeight))
            {
                return;
            }

            previewCanvasHeight = nextHeight;
            ApplyPreviewCanvasHeight();
            evt.StopPropagation();
        }

        private void EndPreviewResize(PointerUpEvent evt)
        {
            if (!previewResizeActive || evt.pointerId != previewDragPointerId || !(evt.currentTarget is VisualElement handle))
            {
                return;
            }

            previewResizeActive = false;
            previewDragPointerId = -1;
            if (handle.HasPointerCapture(evt.pointerId))
            {
                handle.ReleasePointer(evt.pointerId);
            }

            evt.StopPropagation();
        }

        private void CancelPreviewResize()
        {
            previewResizeActive = false;
            previewDragPointerId = -1;
        }

        private void ApplyPreviewCanvasHeight()
        {
            if (beforePreviewImage != null)
            {
                beforePreviewImage.style.height = previewCanvasHeight;
            }

            if (afterPreviewImage != null)
            {
                afterPreviewImage.style.height = previewCanvasHeight;
            }

            Repaint();
        }

        private void EnsureSourcePixelDataMatchesReadableSource()
        {
            if (readableSourceImage == null)
            {
                return;
            }

            EnsureReadableSourceImageEditableFormat();

            if (session.sourcePixelData != null
                && session.sourcePixelData.width == readableSourceImage.width
                && session.sourcePixelData.height == readableSourceImage.height
                && !string.IsNullOrWhiteSpace(session.sourcePixelData.rgbaBytesBase64))
            {
                return;
            }

            session.sourcePixelData = layerTextureSerializationService.Serialize(readableSourceImage.GetPixels32(), readableSourceImage.width, readableSourceImage.height);
        }

        private void EnsureReadableSourceImageEditableFormat()
        {
            if (readableSourceImage == null || readableSourceImage.format == TextureFormat.RGBA32)
            {
                return;
            }

            Texture2D original = readableSourceImage;
            Color32[] pixels = original.GetPixels32();
            Texture2D editableTexture = new Texture2D(original.width, original.height, TextureFormat.RGBA32, false)
            {
                name = $"{original.name}_EditableRgba",
                filterMode = original.filterMode,
                wrapMode = original.wrapMode,
                hideFlags = original.hideFlags
            };
            editableTexture.SetPixels32(pixels);
            editableTexture.Apply(false, false);
            readableSourceImage = editableTexture;

            if (!ReferenceEquals(original, sourceImage))
            {
                DestroyImmediate(original);
            }

            InvalidateSourcePreviewPresentationCaches();
        }

        private static void ApplyPixelsToTexture(Texture2D texture, Color32[] pixels)
        {
            if (texture == null || pixels == null || pixels.Length == 0)
            {
                return;
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
        }

        private void ApplySessionSourcePixelsToReadableImageIfAvailable()
        {
            if (activePaintSession != null && activePaintSession.Target == PaintEditTarget.SourceImage)
            {
                return;
            }

            if (readableSourceImage == null
                || session.sourcePixelData == null
                || session.sourcePixelData.width != readableSourceImage.width
                || session.sourcePixelData.height != readableSourceImage.height
                || string.IsNullOrWhiteSpace(session.sourcePixelData.rgbaBytesBase64))
            {
                return;
            }

            ApplyPixelsToTexture(readableSourceImage, layerTextureSerializationService.Deserialize(session.sourcePixelData));
        }

        private void InvalidateSourcePreviewPresentationCaches()
        {
            DestroyTexture(ref sourceEditingPreviewTexture);
            sourceEditingPreviewKey = string.Empty;
            DestroySplitPreviewTexture();
            DestroyDiffPreviewTexture();
            DestroyUiToolkitHighlightTextures();
        }

        private void ReleasePreviewPointerCapture(Image image)
        {
            if (image == null || previewDragPointerId < 0)
            {
                return;
            }

            if (image.HasPointerCapture(previewDragPointerId))
            {
                image.ReleasePointer(previewDragPointerId);
            }
        }

        private void EnsurePreviewModeMatchesActiveTool()
        {
            switch (session.drawingToolSettings.activeTool)
            {
                case DrawToolKind.Brush:
                case DrawToolKind.Eraser:
                case DrawToolKind.Blur:
                case DrawToolKind.Smooth:
                case DrawToolKind.NoiseRemoval:
                    previewInteractionMode = PreviewInteractionMode.Paint;
                    return;
            }
        }

        private void ApplyPreviewPanDrag(Vector2 delta, float previewWidth, float previewHeight)
        {
            if (previewZoom <= MinPreviewZoom || previewWidth <= 0f || previewHeight <= 0f)
            {
                previewPan = Vector2.zero;
                return;
            }

            previewPan = previewDragStartPan + new Vector2(
                -delta.x / previewWidth / previewZoom,
                delta.y / previewHeight / previewZoom);
            ClampPreviewPan();
        }

        private void PickPaletteColorFromPreview(Image image, Vector2 localPosition)
        {
            if (readableSourceImage == null || session.paletteColors.Count == 0)
            {
                return;
            }

            if (!TryGetPreviewPixelCoordinate(image, localPosition, readableSourceImage, previewZoom, previewPan, out int x, out int y))
            {
                return;
            }

            TrySelectPaletteColorAtSourcePixel(x, y, "Preview");
        }

        private void AddBrushSelectionFromPreview(Image image, Vector2 localPosition)
        {
            if (readableSourceImage == null || session.paletteColors.Count == 0)
            {
                return;
            }

            if (!TryGetPreviewPixelCoordinate(image, localPosition, readableSourceImage, previewZoom, previewPan, out int centerX, out int centerY))
            {
                return;
            }

            int radius = Mathf.Max(0, previewBrushSize / 2);
            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                for (int x = centerX - radius; x <= centerX + radius; x++)
                {
                    TryAddBrushPaletteColorAtSourcePixel(x, y);
                }
            }
        }

        private bool CanPaintSelectedTarget()
        {
            if (session.drawingToolSettings.paintTarget == PaintEditTarget.SourceImage)
            {
                if (readableSourceImage == null)
                {
                    reportMessage = "Analyze a source image before editing it directly.";
                    reportType = MessageType.Warning;
                    return false;
                }

                return true;
            }

            RasterLayer activeLayer = GetActiveLayer();
            if (activeLayer == null)
            {
                reportMessage = "Select or create an active layer before painting.";
                reportType = MessageType.Warning;
                return false;
            }

            if (activeLayer.locked)
            {
                reportMessage = "Unlock the active layer before painting.";
                reportType = MessageType.Warning;
                return false;
            }

            if (replacementPreview == null)
            {
                reportMessage = "Preview is required before painting.";
                reportType = MessageType.Warning;
                return false;
            }

            return true;
        }

        private void BeginPaintStroke()
        {
            if (activePaintSession != null)
            {
                return;
            }

            EnsureLayerSessionState();
            undoSnapshots.Push(JsonUtility.ToJson(session));
            redoSnapshots.Clear();
            EnsureSourcePixelDataMatchesReadableSource();
            activePaintSession = paintStrokeSessionService.Begin(session, readableSourceImage, GetActiveLayer());
        }

        private void CommitPaintStroke()
        {
            if (activePaintSession == null)
            {
                return;
            }

            PaintStrokeCommitResult commitResult = paintStrokeSessionService.Commit(activePaintSession, session, GetActiveLayer());
            if (commitResult.Changed)
            {
                if (commitResult.Target == PaintEditTarget.SourceImage)
                {
                    ApplyPixelsToTexture(readableSourceImage, commitResult.Pixels);
                    InvalidateSourcePreviewPresentationCaches();
                    if (session.colorGroups.Count > 0)
                    {
                        RefreshAfterPreview();
                    }
                    else
                    {
                        UpdatePreviewImagesImmediately();
                    }
                }
                else
                {
                    RebuildCompositePreview();
                }
            }

            activePaintSession = null;
        }

        private void ApplyActivePaintToolFromPreview(Image image, Vector2 localPosition)
        {
            if (!CanPaintSelectedTarget())
            {
                return;
            }

            if (!TryGetPreviewPixelCoordinate(image, localPosition, readableSourceImage, previewZoom, previewPan, out int centerX, out int centerY))
            {
                return;
            }

            if (activePaintSession == null || activePaintSession.Width <= 0 || activePaintSession.Height <= 0)
            {
                BeginPaintStroke();
                if (activePaintSession == null)
                {
                    return;
                }
            }

            ApplyActivePaintToolAtPixel(centerX, centerY);
        }

        private void ApplyActivePaintToolAtPixel(int centerX, int centerY)
        {
            paintStrokeSessionService.Apply(activePaintSession, session.drawingToolSettings, centerX, centerY);

            if (activePaintSession.Target == PaintEditTarget.SourceImage)
            {
                ApplyPixelsToTexture(readableSourceImage, activePaintSession.Pixels);
                InvalidateSourcePreviewPresentationCaches();
                UpdatePreviewImagesImmediately();
                return;
            }

            RasterLayer activeLayer = GetActiveLayer();
            if (activeLayer == null)
            {
                return;
            }

            if (afterPreview == null)
            {
                afterPreview = layerCompositingService.Compose(replacementPreview, session.layers);
            }
            else
            {
                layerCompositingService.UpdateCompositeTexture(afterPreview, replacementPreview, session.layers, activeLayer.id, activePaintSession.Pixels, activePaintSession.Width, activePaintSession.Height);
                InvalidateAfterPreviewPresentationCaches();
            }

            UpdatePreviewImagesImmediately();
            Repaint();
        }

        private bool TryAddBrushPaletteColorAtSourcePixel(int x, int y)
        {
            PaletteColorEntry entry = FindPaletteColorAtSourcePixel(x, y);
            if (entry == null)
            {
                return false;
            }

            brushedColorEntryIds.Add(entry.id);
            return true;
        }

        private PaletteColorEntry FindPaletteColorAtSourcePixel(int x, int y)
        {
            if (readableSourceImage == null || session.paletteColors.Count == 0)
            {
                return null;
            }

            if (x < 0 || y < 0 || x >= readableSourceImage.width || y >= readableSourceImage.height)
            {
                return null;
            }

            Color32 sourcePixel = readableSourceImage.GetPixel(x, y);
            int alphaThreshold = Mathf.Clamp(session.analyzeSettings.alphaThreshold, 0, 255);
            if (sourcePixel.a <= alphaThreshold)
            {
                return null;
            }

            int quantizeStep = Mathf.Clamp(session.analyzeSettings.quantizeStep, 1, 64);
            uint key = ColorCodeUtility.ToRgbKey(colorQuantizationService.Quantize(sourcePixel, quantizeStep));
            return session.paletteColors.FirstOrDefault(candidate =>
                candidate != null && ColorCodeUtility.ToRgbKey(candidate.color) == key);
        }

        private void CreateRulesFromBrushSelection()
        {
            if (brushedColorEntryIds.Count == 0)
            {
                reportMessage = "Brush selection is empty.";
                reportType = MessageType.Warning;
                return;
            }

            RecordSessionEdit(() =>
            {
                foreach (string entryId in brushedColorEntryIds.ToList())
                {
                    PaletteColorEntry entry = session.paletteColors.FirstOrDefault(candidate => candidate != null && candidate.id == entryId);
                    if (entry == null)
                    {
                        continue;
                    }

                    ColorReplacementRule rule = GetOrCreateColorRule(entry);
                    rule.enabled = true;
                    rule.targetColor = rule.targetColor.a == 0 ? entry.color : rule.targetColor;
                    EnsureGroupSupportsColorRule(entry);
                }
            });
            reportMessage = $"Created or updated {brushedColorEntryIds.Count} brush color rule(s).";
            reportType = MessageType.Info;
        }

        private void ClearBrushSelection()
        {
            brushedColorEntryIds.Clear();
            reportMessage = "Brush selection cleared.";
            reportType = MessageType.Info;
        }

        internal bool TrySelectPaletteColorAtSourcePixel(int x, int y, string source)
        {
            if (readableSourceImage == null || session.paletteColors.Count == 0)
            {
                return false;
            }

            if (x < 0 || y < 0 || x >= readableSourceImage.width || y >= readableSourceImage.height)
            {
                return false;
            }

            PaletteColorEntry entry = FindPaletteColorAtSourcePixel(x, y);
            if (entry == null)
            {
                reportMessage = $"Preview pixel {x}, {y} does not match an extracted palette color.";
                reportType = MessageType.Warning;
                RefreshUiToolkitContent();
                return false;
            }

            SelectPaletteEntry(entry, source);
            return true;
        }

        internal static bool TryGetPreviewPixelCoordinate(Image image, Vector2 localPosition, Texture2D texture, out int x, out int y)
        {
            return TryGetPreviewPixelCoordinate(image, localPosition, texture, MinPreviewZoom, Vector2.zero, out x, out y);
        }

        internal static bool TryGetPreviewPixelCoordinate(Image image, Vector2 localPosition, Texture2D texture, float zoom, Vector2 pan, out int x, out int y)
        {
            x = 0;
            y = 0;
            if (image == null || texture == null)
            {
                return false;
            }

            float elementWidth = image.resolvedStyle.width;
            float elementHeight = image.resolvedStyle.height;
            if (elementWidth <= 0f || elementHeight <= 0f)
            {
                Rect contentRect = image.contentRect;
                elementWidth = contentRect.width;
                elementHeight = contentRect.height;
            }

            if (elementWidth <= 0f || elementHeight <= 0f)
            {
                return false;
            }

            float scale = Mathf.Min(elementWidth / texture.width, elementHeight / texture.height);
            float drawnWidth = texture.width * scale;
            float drawnHeight = texture.height * scale;
            float offsetX = (elementWidth - drawnWidth) * 0.5f;
            float offsetY = (elementHeight - drawnHeight) * 0.5f;
            float px = (localPosition.x - offsetX) / scale;
            float py = (localPosition.y - offsetY) / scale;
            if (px < 0f || py < 0f || px >= texture.width || py >= texture.height)
            {
                return false;
            }

            Rect texCoords = GetPreviewTexCoords(zoom, pan);
            float normalizedX = px / texture.width;
            float normalizedY = 1f - (py / texture.height);
            float sourceU = texCoords.xMin + (normalizedX * texCoords.width);
            float sourceV = texCoords.yMin + (normalizedY * texCoords.height);
            x = Mathf.Clamp(Mathf.FloorToInt(sourceU * texture.width), 0, texture.width - 1);
            y = Mathf.Clamp(Mathf.FloorToInt(sourceV * texture.height), 0, texture.height - 1);
            return true;
        }

        private static VisualElement CreateSelectableRow(bool selected)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.flexWrap = Wrap.Wrap;
            row.style.alignItems = Align.Center;
            row.style.paddingTop = 3f;
            row.style.paddingBottom = 3f;
            row.style.paddingLeft = 4f;
            row.style.paddingRight = 4f;
            row.style.marginBottom = 2f;
            if (selected)
            {
                row.style.backgroundColor = new Color(0.18f, 0.32f, 0.48f, 0.45f);
            }

            return row;
        }

        private static VisualElement CreateSwatch(Color32 color)
        {
            VisualElement swatch = new VisualElement();
            swatch.style.width = 18f;
            swatch.style.height = 18f;
            swatch.style.marginRight = 6f;
            swatch.style.backgroundColor = ToColor(color);
            return swatch;
        }

        private VisualElement CreatePaletteRuleBadgeRow(PaletteColorEntry entry)
        {
            VisualElement badgeRow = new VisualElement();
            badgeRow.style.flexDirection = FlexDirection.Row;
            badgeRow.style.flexWrap = Wrap.Wrap;
            badgeRow.style.justifyContent = Justify.FlexEnd;
            badgeRow.style.flexShrink = 1f;
            badgeRow.style.minWidth = 0f;
            foreach (PaletteRuleBadge badge in BuildPaletteRuleBadges(session, entry))
            {
                badgeRow.Add(CreateStatusBadge(badge.Text, badge.Color));
            }

            return badgeRow;
        }

        private static List<PaletteRuleBadge> BuildPaletteRuleBadges(PaletteVariantSession paletteSession, PaletteColorEntry entry)
        {
            List<PaletteRuleBadge> badges = new List<PaletteRuleBadge>();
            if (paletteSession == null || entry == null)
            {
                return badges;
            }

            ColorGroup group = paletteSession.colorGroups?.FirstOrDefault(candidate => candidate != null && candidate.id == entry.groupId);
            ColorReplacementRule rule = paletteSession.colorRules?.FirstOrDefault(candidate =>
                candidate != null
                && candidate.scope == ColorReplacementScope.ColorEntry
                && candidate.colorEntryId == entry.id);
            bool hasRule = rule != null;
            bool ruleEnabled = rule != null && rule.enabled;
            bool colorRuleMode = group != null
                && (group.replacementMode == ColorReplacementMode.PerColor || group.replacementMode == ColorReplacementMode.Hybrid);

            badges.Add(new PaletteRuleBadge(hasRule ? "Rule" : "NoRule", hasRule ? BadgeNeutralColor : new Color(0.22f, 0.22f, 0.22f, 1f)));
            if (hasRule)
            {
                badges.Add(new PaletteRuleBadge(ruleEnabled ? "On" : "Off", ruleEnabled ? BadgeActiveColor : BadgeNeutralColor));
            }

            if (group != null)
            {
                badges.Add(new PaletteRuleBadge(group.replacementMode.ToString(), colorRuleMode ? BadgeActiveColor : BadgeNeutralColor));
            }

            if (hasRule && ruleEnabled)
            {
                badges.Add(new PaletteRuleBadge(colorRuleMode ? "Active" : "ModeOff", colorRuleMode ? BadgeActiveColor : BadgeWarningColor));
            }

            Color32 replacementColor = hasRule && ruleEnabled && colorRuleMode
                ? rule.targetColor
                : group?.targetColor ?? entry.color;
            if (replacementColor.a == 0)
            {
                badges.Add(new PaletteRuleBadge("Alpha", BadgeTransparentColor));
            }

            return badges;
        }

        internal static IReadOnlyList<string> BuildPaletteRuleBadgeTextsForValidation(PaletteVariantSession paletteSession, PaletteColorEntry entry)
        {
            return BuildPaletteRuleBadges(paletteSession, entry).Select(badge => badge.Text).ToList();
        }

        private static Label CreateStatusBadge(string text, Color color)
        {
            Label badge = new Label(text);
            badge.style.fontSize = 10f;
            badge.style.unityFontStyleAndWeight = FontStyle.Bold;
            badge.style.color = Color.white;
            badge.style.backgroundColor = color;
            badge.style.paddingLeft = 4f;
            badge.style.paddingRight = 4f;
            badge.style.paddingTop = 1f;
            badge.style.paddingBottom = 1f;
            badge.style.marginLeft = 3f;
            badge.style.marginTop = 1f;
            badge.style.marginBottom = 1f;
            badge.style.borderTopLeftRadius = 3f;
            badge.style.borderTopRightRadius = 3f;
            badge.style.borderBottomLeftRadius = 3f;
            badge.style.borderBottomRightRadius = 3f;
            return badge;
        }

        private VisualElement CreateButtonRow(params (string Label, System.Action Action, bool Enabled)[] buttons)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.flexWrap = Wrap.Wrap;
            row.style.marginTop = 4f;
            foreach ((string label, System.Action action, bool enabled) in buttons)
            {
                Button button = new Button(() => RunUiToolkitAction(action)) { text = label };
                button.SetEnabled(enabled);
                button.style.marginRight = 4f;
                row.Add(button);
            }

            return row;
        }

        private SliderInt CreateSliderInt(string name, string label, int value, int lowValue, int highValue, System.Action<int> onChanged)
        {
            SliderInt slider = new SliderInt(label, lowValue, highValue) { name = name, value = value, showInputField = true };
            slider.RegisterValueChangedCallback(evt => ApplyUiChange(() => onChanged(evt.newValue)));
            return slider;
        }

        private Slider CreateSlider(string name, string label, float value, float lowValue, float highValue, System.Action<float> onChanged)
        {
            Slider slider = new Slider(label, lowValue, highValue) { name = name, value = value, showInputField = true };
            slider.RegisterValueChangedCallback(evt => ApplyUiChange(() => onChanged(evt.newValue)));
            return slider;
        }

        private IntegerField CreateIntegerField(string name, string label, int value, System.Action<int> onChanged)
        {
            IntegerField field = new IntegerField(label) { name = name, value = value };
            field.RegisterValueChangedCallback(evt => ApplyUiChange(() => onChanged(evt.newValue)));
            return field;
        }

        private static TControl AttachParameterHelp<TControl>(TControl control, string helpText)
            where TControl : VisualElement
        {
            control.tooltip = helpText;
            return control;
        }

        internal string ParameterHelpText(string key)
        {
            return key switch
            {
                "analysisPreset" => T("analysisPresetHelp", "Changes several analysis defaults at once. Choose this first, then fine tune individual parameters."),
                "alphaThreshold" => T("alphaThresholdHelp", "Increase to treat more semi-transparent edge pixels as transparent, making extracted shapes cleaner but possibly thinner. Decrease to keep faint antialiasing and soft glows."),
                "minimumPixelCount" => T("minimumPixelCountHelp", "Increase to ignore tiny colors and reduce palette noise. Decrease to keep small accents, sparkles, and single-pixel details."),
                "quantizeStep" => T("quantizeStepHelp", "Increase to merge close color values into fewer swatches, producing smoother groups. Decrease for more exact colors and sharper gradients."),
                "maxPaletteColors" => T("maxPaletteColorsHelp", "Increase to keep more extracted colors. Decrease to cap the palette earlier and make the list easier to edit."),
                "targetGroupCount" => T("targetGroupCountHelp", "Increase for more separate replacement groups and finer control. Decrease to combine similar colors into broader changes."),
                "distanceMode" => T("distanceModeHelp", "RGB is direct and fast, HSV keeps hue relationships, and Lab tends to match human-perceived color similarity."),
                "maxColorDistance" => T("maxColorDistanceHelp", "Increase to merge colors that are farther apart, reducing group count but broadening replacements. Decrease to keep similar shades separate. Visually, replacements become broader or more localized."),
                "preserveDarkOutline" => T("preserveDarkOutlineHelp", "Enable to keep dark line work from being absorbed into bright groups. Disable when outlines should recolor with the surrounding material."),
                "preserveAlpha" => T("preserveAlphaHelp", "Enable to keep original opacity for normal color replacements. Disable when replacement alpha should directly affect exported transparency."),
                "edgeCleanupEnabled" => T("edgeCleanupEnabledHelp", "Enable to remove color spill outside the detected foreground. Visually this clears stray edge pixels around opaque-background assets."),
                "edgeCleanupMode" => T("edgeCleanupModeHelp", "Boundary Trim cuts a narrow border from the inferred foreground, while outside cleanup targets small regions beyond the edge."),
                "edgeCleanupDistance" => T("edgeCleanupDistanceHelp", "Increase to search farther outside the edge for removable spill. Too high can remove nearby intentional details."),
                "edgeCleanupMaxRegion" => T("edgeCleanupMaxRegionHelp", "Increase to remove larger outside islands. Decrease to only remove very small specks."),
                "edgeTrimDistance" => T("edgeTrimDistanceHelp", "Increase to trim more pixels from the boundary, making silhouettes tighter. Decrease to preserve more edge antialiasing."),
                "noiseRemovalEnabled" => T("noiseRemovalEnabledHelp", "Enable to fill small isolated color islands with nearby colors. This reduces speckles in the generated preview/export."),
                "maxNoiseRegionPixels" => T("maxNoiseRegionPixelsHelp", "Increase to treat larger islands as noise. Decrease to protect small intentional highlights."),
                "noiseNeighborThreshold" => T("noiseNeighborThresholdHelp", "Increase to allow less similar neighbor colors to fill noise. Decrease to require closer visual matches."),
                "sameGroupOnly" => T("sameGroupOnlyHelp", "Enable to fill noise only from the same group, preserving material boundaries. Disable for more aggressive cleanup."),
                _ => T("parameterHelpDefault", "Focus a parameter to see how increasing or decreasing it changes the output.")
            };
        }

        private TextField CreateTextField(string name, string label, string value, System.Action<string> onChanged)
        {
            TextField field = new TextField(label) { name = name, value = value };
            field.RegisterValueChangedCallback(evt => ApplyUiChange(() => onChanged(evt.newValue)));
            return field;
        }

        private PopupField<string> CreateEnumField(string name, string label, System.Enum value, System.Action<System.Enum> onChanged)
        {
            System.Enum[] values = System.Enum.GetValues(value.GetType()).Cast<System.Enum>().ToArray();
            List<string> labels = values.Select(LocalizeEnumValue).ToList();
            int selectedIndex = System.Array.FindIndex(values, candidate => System.Enum.Equals(candidate, value));
            selectedIndex = Mathf.Clamp(selectedIndex, 0, Mathf.Max(0, labels.Count - 1));
            PopupField<string> field = new PopupField<string>(label, labels, selectedIndex) { name = name };
            field.RegisterValueChangedCallback(evt =>
            {
                int nextIndex = labels.IndexOf(evt.newValue);
                if (nextIndex < 0 || nextIndex >= values.Length)
                {
                    return;
                }

                ApplyUiChange(() => onChanged(values[nextIndex]));
            });
            return field;
        }

        private void SyncInteractivePopupValues()
        {
            if (session?.drawingToolSettings != null)
            {
                SetEnumPopupValueWithoutNotify(paintTargetPopup, session.drawingToolSettings.paintTarget);
                SetEnumPopupValueWithoutNotify(drawToolPopup, session.drawingToolSettings.activeTool);
            }

            SetEnumPopupValueWithoutNotify(previewInteractionModePopup, previewInteractionMode);
            SetEnumPopupValueWithoutNotify(previewCompareModePopup, previewCompareMode);
        }

        private void SetEnumPopupValueWithoutNotify(PopupField<string> field, System.Enum value)
        {
            if (field == null || value == null)
            {
                return;
            }

            string label = LocalizeEnumValue(value);
            if (field.choices == null || !field.choices.Contains(label))
            {
                return;
            }

            if (field.value != label)
            {
                field.SetValueWithoutNotify(label);
            }
        }

        private string LocalizeEnumValue(System.Enum value)
        {
            if (displayLanguage != PaletteVariantDisplayLanguage.Japanese || value == null)
            {
                return value?.ToString() ?? string.Empty;
            }

            return value switch
            {
                AnalysisCategoryPreset.TransparentPng => "透過 PNG",
                AnalysisCategoryPreset.WhiteBackgroundJpg => "白背景 JPG",
                AnalysisCategoryPreset.LineArtIcon => "線画アイコン",
                AnalysisCategoryPreset.Gem => "宝石",
                AnalysisCategoryPreset.Plant => "植物",
                ColorDistanceMode.Rgb => "RGB",
                ColorDistanceMode.Hsv => "HSV",
                ColorDistanceMode.Lab => "Lab",
                EdgeOutsideCleanupMode.DetachedRegions => "分離領域",
                EdgeOutsideCleanupMode.BoundaryTrim => "境界トリム",
                ExportConflictMode.Overwrite => "上書き",
                ExportConflictMode.Skip => "スキップ",
                ExportConflictMode.Duplicate => "複製",
                DrawToolKind.Brush => "ブラシ",
                DrawToolKind.Eraser => "消しゴム",
                DrawToolKind.Blur => "ぼかし",
                DrawToolKind.Smooth => "スムース",
                DrawToolKind.NoiseRemoval => "ノイズ除去",
                PaintEditTarget.SourceImage => "読み込み画像",
                PaintEditTarget.ActiveLayer => "アクティブレイヤー",
                PreviewInteractionMode.Pick => "選択",
                PreviewInteractionMode.BrushSelect => "ブラシ選択",
                PreviewInteractionMode.Paint => "描画",
                PreviewCompareMode.SideBySide => "左右比較",
                PreviewCompareMode.Split => "分割比較",
                PreviewCompareMode.Difference => "差分",
                ColorReplacementMode.GroupUniform => "グループ一括",
                ColorReplacementMode.PerColor => "色別",
                ColorReplacementMode.Hybrid => "ハイブリッド",
                PaletteVariantLanguageMode.Auto => "自動",
                PaletteVariantLanguageMode.English => "English",
                PaletteVariantLanguageMode.Japanese => "日本語",
                _ => value.ToString()
            };
        }

        private Toggle CreateToggle(string name, string label, bool value, System.Action<bool> onChanged)
        {
            Toggle toggle = new Toggle(label) { name = name, value = value };
            toggle.RegisterValueChangedCallback(evt => ApplyUiChange(() => onChanged(evt.newValue)));
            return toggle;
        }

        private ColorField CreateColorField(string name, string label, Color32 value, System.Action<Color> onChanged)
        {
            ColorField field = new ColorField(label) { name = name, value = ToColor(value) };
            field.RegisterValueChangedCallback(evt => ApplyUiChange(() => onChanged(evt.newValue)));
            return field;
        }

        private void ApplyUiChange(System.Action action)
        {
            if (isRefreshingUiToolkit)
            {
                return;
            }

            action?.Invoke();
        }

        private static Color ToColor(Color32 value)
        {
            return new Color(value.r / 255f, value.g / 255f, value.b / 255f, value.a / 255f);
        }

        private static Color32 ToColor32(Color value)
        {
            return new Color32(
                (byte)Mathf.RoundToInt(Mathf.Clamp01(value.r) * 255f),
                (byte)Mathf.RoundToInt(Mathf.Clamp01(value.g) * 255f),
                (byte)Mathf.RoundToInt(Mathf.Clamp01(value.b) * 255f),
                (byte)Mathf.RoundToInt(Mathf.Clamp01(value.a) * 255f));
        }

        private void OnGUI()
        {
            if (IsUiToolkitHostActive())
            {
                return;
            }

            DrawImGuiContent();
        }

        private void DrawImGuiContent()
        {
            bool compactLayout = ShouldUseCompactLayout(position.width);
            DrawToolbar();
            EditorGUILayout.HelpBox(reportMessage, reportType);

            if (compactLayout)
            {
                compactLayoutScroll = EditorGUILayout.BeginScrollView(compactLayoutScroll);
                DrawLeftPane(true);
                DrawSectionSeparator();
                DrawCenterPane(true);
                DrawSectionSeparator();
                DrawRightPane(true);
                EditorGUILayout.EndScrollView();
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawLeftPane(false);
                DrawPaneSeparator();
                GUILayout.Space(PaneGap);
                DrawCenterPane(false);
                GUILayout.Space(PaneGap);
                DrawPaneSeparator();
                DrawRightPane(false);
            }
        }

        private void DrawToolbar()
        {
            if (ShouldUseCompactLayout(position.width))
            {
                DrawCompactToolbar();
                return;
            }

            DrawWideToolbar();
        }

        private void DrawWideToolbar()
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
                        RequestPreviewRefresh("Preview update queued.");
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

        private void DrawCompactToolbar()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.toolbar))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(T("sourceImage", "Source Image"), GUILayout.Width(86f));
                    sourceImage = (Texture2D)EditorGUILayout.ObjectField(sourceImage, typeof(Texture2D), false, GUILayout.MinWidth(180f));
                    DrawLanguagePopup();
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(sourceImage == null))
                    {
                        if (GUILayout.Button(T("analyze", "Analyze"), EditorStyles.toolbarButton))
                        {
                            AnalyzeSourceImage();
                        }
                    }

                    using (new EditorGUI.DisabledScope(session.paletteColors.Count == 0))
                    {
                        if (GUILayout.Button(T("autoGroup", "Auto Group"), EditorStyles.toolbarButton))
                        {
                            AutoGroupPalette();
                        }
                    }

                    using (new EditorGUI.DisabledScope(readableSourceImage == null || session.colorGroups.Count == 0))
                    {
                        if (GUILayout.Button(T("preview", "Preview"), EditorStyles.toolbarButton))
                        {
                            CancelScheduledAutoPreview();
                            RequestPreviewRefresh("Preview update queued.");
                        }
                    }

                    using (new EditorGUI.DisabledScope(afterPreview == null))
                    {
                        if (GUILayout.Button(T("export", "Export"), EditorStyles.toolbarButton))
                        {
                            ExportPreview();
                        }
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(session.colorGroups.Count == 0))
                    {
                        if (GUILayout.Button(T("exportPreset", "Export Preset"), EditorStyles.toolbarButton))
                        {
                            ExportRulePreset();
                        }

                        if (GUILayout.Button(T("importPreset", "Import Preset"), EditorStyles.toolbarButton))
                        {
                            ImportRulePreset();
                        }
                    }

                    using (new EditorGUI.DisabledScope(readableSourceImage == null || session.variations.Count == 0))
                    {
                        if (GUILayout.Button(T("exportAll", "Export All"), EditorStyles.toolbarButton))
                        {
                            ExportAllVariations();
                        }
                    }

                    if (GUILayout.Button(T("saveSession", "Save Session"), EditorStyles.toolbarButton))
                    {
                        SaveSession();
                    }

                    if (GUILayout.Button(T("loadSession", "Load Session"), EditorStyles.toolbarButton))
                    {
                        LoadSession();
                    }

                    if (GUILayout.Button(T("help", "Help"), EditorStyles.toolbarButton))
                    {
                        OpenHelpWindow();
                    }

                    bool nextAutoPreview = GUILayout.Toggle(autoPreviewEnabled, T("autoPreview", "Auto Preview"), EditorStyles.toolbarButton);
                    if (nextAutoPreview != autoPreviewEnabled)
                    {
                        autoPreviewEnabled = nextAutoPreview;
                        EditorPrefs.SetBool(AutoPreviewPrefsKey, autoPreviewEnabled);
                        if (!autoPreviewEnabled)
                        {
                            CancelScheduledAutoPreview();
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(sourceAssetPath))
                {
                    EditorGUILayout.LabelField(sourceAssetPath, EditorStyles.miniLabel);
                }
            }
        }

        private void DrawLeftPane(bool compactLayout)
        {
            using (new EditorGUILayout.VerticalScope(GetPaneLayoutOptions(LeftPaneWidth, compactLayout)))
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
                DrawSectionHeader(T("noiseRemoval", "Noise Removal"));
                session.noiseRemovalSettings ??= new NoiseRemovalSettings();
                using (var change = new EditorGUI.ChangeCheckScope())
                {
                    session.noiseRemovalSettings.enabled = EditorGUILayout.Toggle(T("noiseRemovalEnabled", "Enable Noise Removal"), session.noiseRemovalSettings.enabled);
                    using (new EditorGUI.DisabledScope(!session.noiseRemovalSettings.enabled))
                    {
                        session.noiseRemovalSettings.maxRegionPixels = Mathf.Max(1, EditorGUILayout.IntSlider(T("maxNoiseRegionPixels", "Max Noise Size"), session.noiseRemovalSettings.maxRegionPixels, 1, 64));
                        session.noiseRemovalSettings.neighborDistanceThreshold = EditorGUILayout.Slider(T("noiseNeighborThreshold", "Neighbor Threshold"), session.noiseRemovalSettings.neighborDistanceThreshold, 0f, 441f);
                        session.noiseRemovalSettings.sameGroupOnly = EditorGUILayout.Toggle(T("sameGroupOnly", "Same Group Only"), session.noiseRemovalSettings.sameGroupOnly);
                    }

                    EditorGUILayout.HelpBox(T("noiseRemovalHint", "Fills small color islands surrounded by the same group with a nearby surrounding color."), MessageType.None);
                    if (change.changed && afterPreview != null)
                    {
                        HandlePreviewSettingChanged();
                    }
                }

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
                DrawSectionHeader(T("batchExport", "Batch Export"));
                using (new EditorGUILayout.HorizontalScope())
                {
                    batchSourceFolder = EditorGUILayout.TextField(T("sourceFolder", "Source Folder"), batchSourceFolder);
                    if (GUILayout.Button("...", GUILayout.Width(28f)))
                    {
                        SelectBatchSourceFolder();
                    }
                }

                using (new EditorGUI.DisabledScope(session.colorGroups.Count == 0 || session.variations.Count == 0))
                {
                    if (GUILayout.Button(T("exportFolder", "Export Folder")))
                    {
                        ExportBatchSourceFolder();
                    }
                }

                if (batchResults.Count > 0)
                {
                    batchResultScroll = EditorGUILayout.BeginScrollView(batchResultScroll, GUILayout.Height(82f));
                    foreach (BatchSourceExportItem item in batchResults.Take(20))
                    {
                        EditorGUILayout.LabelField($"{item.Status}: {System.IO.Path.GetFileNameWithoutExtension(item.AssetPath)} {item.VariationName}", EditorStyles.miniLabel);
                    }

                    EditorGUILayout.EndScrollView();
                }

                DrawSectionSeparator();
                DrawSectionHeader(T("presetAsset", "Preset Asset"));
                presetAsset = (PaletteVariantRulePresetAsset)EditorGUILayout.ObjectField(T("presetAsset", "Preset Asset"), presetAsset, typeof(PaletteVariantRulePresetAsset), false);
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(session.colorGroups.Count == 0))
                    {
                        if (GUILayout.Button(T("createPresetAsset", "Create Asset")))
                        {
                            CreateRulePresetAsset();
                        }
                    }

                    using (new EditorGUI.DisabledScope(presetAsset == null || session.colorGroups.Count == 0))
                    {
                        if (GUILayout.Button(T("updatePresetAsset", "Update Asset")))
                        {
                            UpdateRulePresetAsset();
                        }
                    }
                }

                using (new EditorGUI.DisabledScope(presetAsset == null))
                {
                    if (GUILayout.Button(T("loadPresetAsset", "Load Asset")))
                    {
                        LoadRulePresetAsset();
                    }
                }

                DrawSectionSeparator();
                DrawSectionHeader(T("sourceInfo", "Source Info"));
                if (readableSourceImage != null)
                {
                    DrawSourceAssetPath();
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

        private void DrawCenterPane(bool compactLayout)
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.MinWidth(360f), GUILayout.ExpandWidth(true)))
            {
                DrawSectionHeader(T("preview", "Preview"));
                DrawPreviewControls(compactLayout);
                if (previewCompareMode == PreviewCompareMode.Split)
                {
                    DrawSplitPreviewPanel();
                }
                else if (compactLayout)
                {
                    DrawPreviewPanel(T("before", "Before"), readableSourceImage);
                    DrawPreviewPanel(T("after", "After"), afterPreview);
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

        private void DrawPreviewControls(bool compactLayout)
        {
            if (compactLayout)
            {
                previewCompareMode = (PreviewCompareMode)EditorGUILayout.EnumPopup(T("compareMode", "Compare"), previewCompareMode);
                EditorGUI.BeginChangeCheck();
                previewZoom = EditorGUILayout.Slider(T("zoom", "Zoom"), previewZoom, MinPreviewZoom, MaxPreviewZoom);
                if (EditorGUI.EndChangeCheck())
                {
                    previewZoom = Mathf.Clamp(previewZoom, MinPreviewZoom, MaxPreviewZoom);
                    ClampPreviewPan();
                }

                if (GUILayout.Button(T("resetView", "Reset View")))
                {
                    ResetPreviewView();
                }

                if (previewCompareMode == PreviewCompareMode.Split)
                {
                    previewSplit = EditorGUILayout.Slider(T("split", "Split"), previewSplit, 0.05f, 0.95f);
                }

                return;
            }

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

        private void DrawRightPane(bool compactLayout)
        {
            using (new EditorGUILayout.VerticalScope(GetPaneLayoutOptions(RightPaneWidth, compactLayout)))
            {
                rightScroll = EditorGUILayout.BeginScrollView(
                    rightScroll,
                    false,
                    true,
                    GUIStyle.none,
                    GUI.skin.verticalScrollbar,
                    GUI.skin.scrollView);
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

        private void DrawSourceAssetPath()
        {
            EditorGUILayout.LabelField(T("assetPath", "Asset Path"), EditorStyles.miniBoldLabel);
            GUIStyle wrappedPathStyle = new GUIStyle(EditorStyles.wordWrappedMiniLabel)
            {
                wordWrap = true
            };
            EditorGUILayout.SelectableLabel(sourceAssetPath, wrappedPathStyle, GUILayout.MinHeight(32f));
        }

        private void DrawPreviewPanel(string title, Texture2D texture)
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
            {
                EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
                Rect previewRect = GUILayoutUtility.GetRect(10f, 10000f, previewCanvasHeight, previewCanvasHeight, GUILayout.ExpandWidth(true));
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
            Rect previewRect = GUILayoutUtility.GetRect(10f, 10000f, previewCanvasHeight, previewCanvasHeight, GUILayout.ExpandWidth(true));
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
            session.sourcePixelData = layerTextureSerializationService.Serialize(loadedTexture.GetPixels32(), loadedTexture.width, loadedTexture.height);
            session.paletteColors = new List<PaletteColorEntry>(colorExtractionService.Extract(readableSourceImage, session.analyzeSettings));
            session.colorGroups.Clear();
            session.colorRules.Clear();
            session.variations.Clear();
            session.layers.Clear();
            session.activeVariationId = string.Empty;
            session.activeLayerId = string.Empty;
            selectedGroupId = string.Empty;
            selectedColorEntryId = string.Empty;
            InvalidateSelectionOverlay();
            DestroyAfterPreview();
            DestroyReplacementPreview();
            reportMessage = BuildAnalyzeReportMessage(assetPath, session.paletteColors.Count);
            reportType = MessageType.Info;
        }

        private string BuildAnalyzeReportMessage(string assetPath, int paletteColorCount)
        {
            if (TextureAssetLoader.IsJpegAssetPath(assetPath))
            {
                return displayLanguage == PaletteVariantDisplayLanguage.Japanese
                    ? $"JPEGを内部の透過編集用PNGバッファに変換しました。元画像は変更しません。パレット色数: {paletteColorCount}"
                    : $"Converted JPEG to an internal transparent PNG editing buffer. Original asset is unchanged. Palette colors: {paletteColorCount}";
            }

            return displayLanguage == PaletteVariantDisplayLanguage.Japanese
                ? $"{assetPath} から {paletteColorCount} 色を解析しました。"
                : $"Analyzed {paletteColorCount} palette colors from {assetPath}.";
        }

        private void AutoGroupPalette()
        {
            session.colorGroups = new List<ColorGroup>(colorGroupingService.CreateGroups(session.paletteColors, session.groupSettings));
            session.colorRules.Clear();
            session.variations.Clear();
            session.layers.Clear();
            session.activeVariationId = string.Empty;
            session.activeLayerId = string.Empty;
            variationService.EnsureActiveVariation(session);
            variationService.SyncActiveVariation(session);
            selectedGroupId = session.colorGroups.Count > 0 ? session.colorGroups[0].id : string.Empty;
            selectedColorEntryId = string.Empty;
            InvalidateSelectionOverlay();
            reportMessage = $"Created {session.colorGroups.Count} color groups.";
            reportType = session.colorGroups.Count == 0 ? MessageType.Warning : MessageType.Info;
            RequestPreviewRefresh("Preview update queued after grouping.");
        }

        private void RefreshAfterPreview()
        {
            if (readableSourceImage == null)
            {
                reportMessage = "Analyze a source image before preview.";
                reportType = MessageType.Warning;
                return;
            }

            ApplySessionSourcePixelsToReadableImageIfAvailable();

            if (session.colorGroups.Count == 0)
            {
                reportMessage = "Create color groups before preview.";
                reportType = MessageType.Warning;
                return;
            }

            DestroyReplacementPreview();
            replacementPreview = colorReplacementService.Apply(readableSourceImage, session);
            RebuildCompositePreview();
            EdgeOutsideCleanupResult edgeResult = colorReplacementService.LastEdgeOutsideCleanupResult;
            NoiseRemovalResult noiseResult = colorReplacementService.LastNoiseRemovalResult;
            lastEdgeEffectHighlightIndices = edgeResult.ClearedPixelIndices?.ToList() ?? new List<int>();
            lastNoiseEffectHighlightIndices = noiseResult.FilledPixelIndices?.ToList() ?? new List<int>();
            List<string> preprocessingReports = new List<string>();
            if (session.edgeOutsideCleanupSettings?.enabled == true)
            {
                preprocessingReports.Add($"Edge Cleanup cleared {edgeResult.ClearedRegionCount} region(s), {edgeResult.ClearedPixelCount} pixel(s)");
            }

            if (session.noiseRemovalSettings?.enabled == true)
            {
                preprocessingReports.Add($"Noise Removal filled {noiseResult.FilledRegionCount} region(s), {noiseResult.FilledPixelCount} pixel(s)");
            }

            reportMessage = preprocessingReports.Count > 0
                ? "After preview updated. " + string.Join(". ", preprocessingReports) + "."
                : "After preview updated.";
            reportType = MessageType.Info;
        }

        private void RequestPreviewRefresh(string queuedMessage)
        {
            if (readableSourceImage == null)
            {
                reportMessage = "Analyze a source image before preview.";
                reportType = MessageType.Warning;
                RefreshUiToolkitContent();
                return;
            }

            if (session.colorGroups.Count == 0)
            {
                reportMessage = "Create color groups before preview.";
                reportType = MessageType.Warning;
                RefreshUiToolkitContent();
                return;
            }

            previewRefreshRequestVersion++;
            scheduledPreviewRefreshVersion = previewRefreshRequestVersion;
            previewRefreshQueued = true;
            reportMessage = IsLargeAutoPreviewSource(readableSourceImage)
                ? $"{queuedMessage} Large image detected; the latest request will run shortly."
                : queuedMessage;
            reportType = MessageType.Info;
            EditorApplication.delayCall -= ProcessDelayedPreviewRefresh;
            EditorApplication.delayCall += ProcessDelayedPreviewRefresh;
            RefreshUiToolkitContent();
            Repaint();
        }

        private void ProcessDelayedPreviewRefresh()
        {
            if (!previewRefreshQueued)
            {
                return;
            }

            int requestedVersion = scheduledPreviewRefreshVersion;
            previewRefreshQueued = false;
            if (requestedVersion != previewRefreshRequestVersion)
            {
                return;
            }

            previewRefreshProcessing = true;
            reportMessage = "Preview updating...";
            reportType = MessageType.Info;
            RefreshUiToolkitContent();
            Repaint();

            try
            {
                RefreshAfterPreview();
            }
            finally
            {
                previewRefreshProcessing = false;
                RefreshUiToolkitContent();
                Repaint();
            }
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
            CancelQueuedPreviewRefresh();
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
            RequestPreviewRefresh("Auto Preview update queued.");
            Repaint();
        }

        private void CancelScheduledAutoPreview()
        {
            autoPreviewPending = false;
            EditorApplication.update -= ProcessScheduledAutoPreview;
        }

        private void CancelQueuedPreviewRefresh()
        {
            previewRefreshQueued = false;
            EditorApplication.delayCall -= ProcessDelayedPreviewRefresh;
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

        internal static bool ShouldUseCompactLayout(float windowWidth)
        {
            return windowWidth > 0f && windowWidth < CompactLayoutWidth;
        }

        internal static Vector2 GetDockedMinimumWindowSize()
        {
            return new Vector2(DockedMinWidth, DockedMinHeight);
        }

        private static GUILayoutOption[] GetPaneLayoutOptions(float fixedWidth, bool compactLayout)
        {
            return compactLayout
                ? new[] { GUILayout.ExpandWidth(true) }
                : new[] { GUILayout.Width(fixedWidth) };
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
            if (!ConfirmExportReadiness(false))
            {
                return;
            }

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
            if (!ConfirmExportReadiness(true))
            {
                return;
            }

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
                Texture2D compositePreview = layerCompositingService.Compose(preview, session.layers);
                ExportSettings exportSettings = CreateExportSettingsForVariation(variation);
                PngExportResult result = pngExportService.Export(compositePreview, exportSettings, GetProjectRoot());
                DestroyImmediate(compositePreview);
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
            session.layers ??= new List<RasterLayer>();
            session.drawingToolSettings ??= new DrawingToolSettings();
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
            DestroyReplacementPreview();
            string loadError = string.Empty;
            if (sourceImage != null && textureAssetLoader.TryLoadReadableTexture(sourceImage, out Texture2D loadedTexture, out _, out loadError))
            {
                readableSourceImage = loadedTexture;
                if (session.sourcePixelData != null
                    && session.sourcePixelData.width == loadedTexture.width
                    && session.sourcePixelData.height == loadedTexture.height
                    && !string.IsNullOrWhiteSpace(session.sourcePixelData.rgbaBytesBase64))
                {
                    ApplyPixelsToTexture(readableSourceImage, layerTextureSerializationService.Deserialize(session.sourcePixelData));
                }
                else
                {
                    session.sourcePixelData = layerTextureSerializationService.Serialize(loadedTexture.GetPixels32(), loadedTexture.width, loadedTexture.height);
                }

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

        private void CreateRulePresetAsset()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Rule Preset Asset",
                "PaletteVariantRulePreset",
                "asset",
                "Create a ScriptableObject rule preset asset.");
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            variationService.SyncActiveVariation(session);
            presetAsset = rulePresetAssetService.CreateAsset(path, session, "Rule Preset");
            reportMessage = $"Rule preset asset created: {AssetDatabase.GetAssetPath(presetAsset)}";
            reportType = MessageType.Info;
        }

        private void UpdateRulePresetAsset()
        {
            variationService.SyncActiveVariation(session);
            rulePresetAssetService.UpdateAsset(presetAsset, session, string.IsNullOrWhiteSpace(presetAsset.preset.displayName) ? "Rule Preset" : presetAsset.preset.displayName);
            reportMessage = $"Rule preset asset updated: {AssetDatabase.GetAssetPath(presetAsset)}";
            reportType = MessageType.Info;
        }

        private void LoadRulePresetAsset()
        {
            IReadOnlyList<string> warnings = rulePresetAssetService.ApplyToSession(presetAsset, session);
            variationService.SyncActiveVariation(session);
            if (afterPreview != null)
            {
                RefreshAfterPreview();
            }

            reportMessage = warnings.Count == 0
                ? $"Rule preset asset loaded: {AssetDatabase.GetAssetPath(presetAsset)}"
                : $"Rule preset asset loaded with warnings:\n{string.Join("\n", warnings)}";
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

        private void SelectBatchSourceFolder()
        {
            string currentFolder = ResolveOutputFolderForPanel(batchSourceFolder);
            string selectedFolder = EditorUtility.OpenFolderPanel("Batch Source Folder", currentFolder, string.Empty);
            if (string.IsNullOrWhiteSpace(selectedFolder))
            {
                return;
            }

            batchSourceFolder = ToProjectRelativePath(selectedFolder);
        }

        private void ExportBatchSourceFolder()
        {
            if (session.colorGroups.Count == 0)
            {
                reportMessage = "Create color groups before folder batch export.";
                reportType = MessageType.Warning;
                return;
            }

            if (session.variations.Count == 0)
            {
                reportMessage = "Create at least one variation before folder batch export.";
                reportType = MessageType.Warning;
                return;
            }

            variationService.SyncActiveVariation(session);
            BatchSourceExportSummary summary = batchSourceExportService.ExportFolder(batchSourceFolder, session, GetProjectRoot());
            batchResults = new List<BatchSourceExportItem>(summary.Items);
            reportMessage = $"Folder batch export completed. Exported: {summary.ExportedCount}, Skipped: {summary.SkippedCount}, Failed: {summary.FailedCount}.";
            reportType = summary.FailedCount > 0
                ? MessageType.Error
                : summary.SkippedCount > 0
                    ? MessageType.Warning
                    : MessageType.Info;
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

        private PaletteColorEntry GetSelectedPaletteEntry()
        {
            return session.paletteColors.FirstOrDefault(candidate => candidate != null && candidate.id == selectedColorEntryId);
        }

        private ColorReplacementRule FindColorRule(PaletteColorEntry entry)
        {
            if (entry == null)
            {
                return null;
            }

            session.colorRules ??= new List<ColorReplacementRule>();
            return session.colorRules.FirstOrDefault(candidate =>
                candidate != null
                && candidate.scope == ColorReplacementScope.ColorEntry
                && candidate.colorEntryId == entry.id);
        }

        private static string FormatHexWithAlpha(Color32 color)
        {
            return $"{ColorCodeUtility.ToHex(color)}{color.a:X2}";
        }

        private ColorReplacementRule GetOrCreateColorRule(PaletteColorEntry entry)
        {
            session.colorRules ??= new List<ColorReplacementRule>();
            ColorReplacementRule rule = FindColorRule(entry);

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

        private void SelectPaletteEntry(PaletteColorEntry entry, string source)
        {
            if (entry == null)
            {
                return;
            }

            selectedColorEntryId = entry.id;
            selectedGroupId = entry.groupId;
            GetOrCreateColorRule(entry);
            InvalidateSelectionOverlay();
            reportMessage = $"{source} selected {entry.hex}. Edit the color rule to replace only this palette color.";
            reportType = MessageType.Info;
            RefreshUiToolkitContent();
        }

        private void EnsureGroupSupportsColorRule(PaletteColorEntry entry)
        {
            if (entry == null || string.IsNullOrEmpty(entry.groupId))
            {
                return;
            }

            ColorGroup group = session.colorGroups.FirstOrDefault(candidate => candidate != null && candidate.id == entry.groupId);
            if (group == null || group.replacementMode != ColorReplacementMode.GroupUniform)
            {
                return;
            }

            group.replacementMode = ColorReplacementMode.Hybrid;
        }

        internal string SelectedColorEntryIdForValidation => selectedColorEntryId;

        internal bool IsSelectionHighlightEnabledForValidation => selectionHighlightEnabled;

        internal bool IsBeforePreviewUsingHighlightTextureForValidation =>
            highlightedBeforePreviewTexture != null && beforePreviewImage != null && beforePreviewImage.image == highlightedBeforePreviewTexture;

        internal bool IsBeforePreviewUsingZoomTextureForValidation =>
            zoomedBeforePreviewTexture != null && beforePreviewImage != null && beforePreviewImage.image == zoomedBeforePreviewTexture;

        internal bool IsAfterPreviewUsingEffectHighlightTextureForValidation =>
            effectHighlightEnabled
            && highlightedAfterPreviewTexture != null
            && afterPreviewImage != null
            && afterPreviewImage.image == highlightedAfterPreviewTexture;

        internal int EffectHighlightPixelCountForValidation =>
            (lastNoiseEffectHighlightIndices?.Count ?? 0) + (lastEdgeEffectHighlightIndices?.Count ?? 0);

        internal bool HasCollapsedSettingsFoldoutsForValidation =>
            rootVisualElement.Q<Foldout>("analyze-foldout") != null
            && rootVisualElement.Q<Foldout>("group-foldout") != null
            && rootVisualElement.Q<Foldout>("tool-foldout") != null
            && rootVisualElement.Q<Foldout>("export-foldout") != null
            && rootVisualElement.Q<Foldout>("preset-foldout") != null;

        internal bool HasPreviewMiniToolbarForValidation =>
            rootVisualElement.Q<VisualElement>("preview-mini-toolbar") != null;

        internal bool IsDifferencePreviewTextureActiveForValidation =>
            previewCompareMode == PreviewCompareMode.Difference
            && diffPreviewTexture != null
            && beforePreviewImage != null
            && beforePreviewImage.image == diffPreviewTexture;

        internal int UndoSnapshotCountForValidation => undoSnapshots.Count;

        internal int RedoSnapshotCountForValidation => redoSnapshots.Count;

        internal int BrushedColorCountForValidation => brushedColorEntryIds.Count;

        internal AnalysisCategoryPreset SelectedAnalysisPresetForValidation => selectedAnalysisPreset;

        internal bool IsPreviewRefreshQueuedForValidation => previewRefreshQueued;

        internal bool IsPreviewRefreshProcessingForValidation => previewRefreshProcessing;

        internal int PreviewRefreshRequestVersionForValidation => previewRefreshRequestVersion;

        internal bool HasAfterPreviewForValidation => afterPreview != null;

        internal string SelectedColorInfoTextForValidation
        {
            get
            {
                PaletteColorEntry entry = GetSelectedPaletteEntry();
                if (entry == null)
                {
                    return T("noSelectedColor", "No palette color selected.");
                }

                return string.Join(" | ", BuildSelectedColorInfoRows(entry).Select(row => $"{row.Key}: {row.Value}"));
            }
        }

        private bool ConfirmExportReadiness(bool allVariations)
        {
            List<string> warnings = BuildExportReadinessWarnings(allVariations);
            List<string> errors = BuildExportReadinessErrors(allVariations);
            if (errors.Count > 0)
            {
                reportMessage = string.Join("\n", errors);
                reportType = MessageType.Error;
                return false;
            }

            if (warnings.Count == 0)
            {
                return true;
            }

            bool continueExport = EditorUtility.DisplayDialog(
                "Export Check",
                string.Join("\n", warnings) + "\n\nContinue export?",
                "Continue",
                "Cancel");
            if (!continueExport)
            {
                reportMessage = "Export cancelled by pre-check.";
                reportType = MessageType.Warning;
            }

            return continueExport;
        }

        internal List<string> BuildExportReadinessWarningsForValidation(bool allVariations)
        {
            return BuildExportReadinessWarnings(allVariations);
        }

        private List<string> BuildExportReadinessErrors(bool allVariations)
        {
            List<string> errors = new List<string>();
            if (string.IsNullOrWhiteSpace(session.exportSettings.outputFolder))
            {
                errors.Add("Output folder is required.");
            }

            if (allVariations && !session.variations.Any(variation => variation != null && variation.exportEnabled))
            {
                errors.Add("No enabled variations are available for export.");
            }

            if (!allVariations && afterPreview == null)
            {
                errors.Add("Preview must be updated before export.");
            }

            return errors;
        }

        private List<string> BuildExportReadinessWarnings(bool allVariations)
        {
            List<string> warnings = new List<string>();
            if (previewRefreshQueued || autoPreviewPending)
            {
                warnings.Add("Preview update is still pending.");
            }

            string outputFolder = ResolveOutputFolderForPanel(session.exportSettings.outputFolder);
            if (!System.IO.Directory.Exists(outputFolder))
            {
                warnings.Add($"Output folder does not exist yet: {outputFolder}");
            }

            foreach (string fileName in BuildExpectedExportFileNames(allVariations))
            {
                string outputPath = System.IO.Path.Combine(outputFolder, fileName);
                if (System.IO.File.Exists(outputPath))
                {
                    warnings.Add($"Output file already exists: {outputPath}");
                }
            }

            if (session.colorRules.Any(rule => rule != null && rule.enabled && rule.targetColor.a == 0))
            {
                warnings.Add("Transparent color replacement is enabled.");
            }

            if (session.colorRules.Any(rule => rule != null && !rule.enabled))
            {
                warnings.Add("Disabled per-color rules exist.");
            }

            if (!session.exportSettings.refreshAssetDatabase)
            {
                warnings.Add("AssetDatabase refresh is disabled.");
            }

            return warnings;
        }

        private IEnumerable<string> BuildExpectedExportFileNames(bool allVariations)
        {
            if (!allVariations)
            {
                yield return BuildVariationOutputFileName(session.exportSettings, variationService.GetActiveVariation(session));
                yield break;
            }

            foreach (IconVariation variation in session.variations.Where(variation => variation != null && variation.exportEnabled))
            {
                yield return BuildVariationOutputFileName(session.exportSettings, variation);
            }
        }

        internal void RequestPreviewRefreshForValidation(string queuedMessage)
        {
            RequestPreviewRefresh(queuedMessage);
        }

        internal void ProcessDelayedPreviewRefreshForValidation()
        {
            ProcessDelayedPreviewRefresh();
        }

        internal void SetPreviewZoomForValidation(float zoom, Vector2 pan)
        {
            previewZoom = Mathf.Clamp(zoom, MinPreviewZoom, MaxPreviewZoom);
            previewPan = pan;
            ClampPreviewPan();
            RefreshUiToolkitContent();
        }

        internal Vector2 PreviewPanForValidation => previewPan;

        internal int DisplayPreviewTextureCountForValidation =>
            (highlightedBeforePreviewTexture == null ? 0 : 1)
            + (highlightedAfterPreviewTexture == null ? 0 : 1)
            + (highlightedSplitPreviewTexture == null ? 0 : 1)
            + (zoomedBeforePreviewTexture == null ? 0 : 1)
            + (zoomedAfterPreviewTexture == null ? 0 : 1)
            + (zoomedSplitPreviewTexture == null ? 0 : 1);

        internal void ApplyPreviewPanDragForValidation(Vector2 delta, float previewWidth, float previewHeight)
        {
            previewDragStartPan = previewPan;
            ApplyPreviewPanDrag(delta, previewWidth, previewHeight);
            RefreshUiToolkitContent();
        }

        internal void SetValidationSession(Texture2D texture, PaletteVariantSession validationSession)
        {
            CancelScheduledAutoPreview();
            CancelQueuedPreviewRefresh();
            DestroyAfterPreview();
            DestroyReadableSourceImage();
            readableSourceImage = texture;
            sourceImage = texture;
            session = validationSession ?? new PaletteVariantSession();
            NormalizeSessionDefaults(session);
            EnsureSourcePixelDataMatchesReadableSource();
            RefreshUiToolkitContent();
        }

        internal void SetSelectionHighlightForValidation(bool enabled)
        {
            selectionHighlightEnabled = enabled;
            InvalidateSelectionOverlay();
            RefreshUiToolkitContent();
        }

        internal void SetEffectHighlightForValidation(bool enabled)
        {
            effectHighlightEnabled = enabled;
            DestroyUiToolkitHighlightTextures();
            RefreshUiToolkitContent();
        }

        internal void SetPreviewCompareModeForValidation(PreviewCompareMode mode)
        {
            previewCompareMode = mode;
            RefreshUiToolkitContent();
        }

        internal void ApplyAnalysisCategoryPresetForValidation(AnalysisCategoryPreset preset)
        {
            selectedAnalysisPreset = preset;
            ApplyAnalysisCategoryPreset(preset);
            RefreshUiToolkitContent();
        }

        internal void SetPreviewInteractionModeForValidation(PreviewInteractionMode mode)
        {
            previewInteractionMode = mode;
            RefreshUiToolkitContent();
        }

        internal void SetPaintTargetForValidation(PaintEditTarget target)
        {
            session.drawingToolSettings.paintTarget = target;
            EnsurePreviewModeMatchesActiveTool();
            RefreshUiToolkitContent();
        }

        internal void SetDrawToolForValidation(DrawToolKind tool)
        {
            session.drawingToolSettings.activeTool = tool;
            EnsurePreviewModeMatchesActiveTool();
            RefreshUiToolkitContent();
        }

        internal string GetPaintTargetPopupValueForValidation()
        {
            return paintTargetPopup?.value ?? string.Empty;
        }

        internal string GetDrawToolPopupValueForValidation()
        {
            return drawToolPopup?.value ?? string.Empty;
        }

        internal bool IsToolPopupStateSyncedForValidation()
        {
            return paintTargetPopup != null
                && drawToolPopup != null
                && paintTargetPopup.value == LocalizeEnumValue(session.drawingToolSettings.paintTarget)
                && drawToolPopup.value == LocalizeEnumValue(session.drawingToolSettings.activeTool);
        }

        internal void SetBrushSettingsForValidation(int brushSize, float strength)
        {
            session.drawingToolSettings.brushSize = Mathf.Max(1, brushSize | 1);
            session.drawingToolSettings.strength = Mathf.Clamp01(strength);
        }

        internal void ApplyPaintAtSourcePixelForValidation(int x, int y)
        {
            BeginPaintStroke();
            if (activePaintSession == null)
            {
                return;
            }

            ApplyActivePaintToolAtPixel(x, y);
            CommitPaintStroke();
            RefreshUiToolkitContent();
        }

        internal Color32 GetSourcePixelForValidation(int x, int y)
        {
            if (readableSourceImage == null || x < 0 || y < 0 || x >= readableSourceImage.width || y >= readableSourceImage.height)
            {
                return default;
            }

            return readableSourceImage.GetPixels32()[(y * readableSourceImage.width) + x];
        }

        internal Color32 GetPrimaryPreviewPixelForValidation(int x, int y)
        {
            Texture2D texture = GetPrimaryPreviewDisplayTexture();
            if (texture == null || x < 0 || y < 0 || x >= texture.width || y >= texture.height)
            {
                return default;
            }

            return texture.GetPixels32()[(y * texture.width) + x];
        }

        internal bool AddBrushSelectionAtSourcePixelForValidation(int x, int y)
        {
            return TryAddBrushPaletteColorAtSourcePixel(x, y);
        }

        internal void CreateBrushRulesForValidation()
        {
            CreateRulesFromBrushSelection();
        }

        internal bool SetSelectedColorVisibleForValidation(bool visible)
        {
            PaletteColorEntry entry = session.paletteColors.FirstOrDefault(candidate => candidate != null && candidate.id == selectedColorEntryId);
            if (entry == null)
            {
                return false;
            }

            ColorReplacementRule rule = GetOrCreateColorRule(entry);
            RecordSessionEdit(() => SetColorEntryVisible(entry, rule, visible));
            return true;
        }

        internal void RefreshAfterPreviewForValidation()
        {
            RefreshAfterPreview();
            RefreshUiToolkitContent();
        }

        internal void UndoForValidation()
        {
            UndoSessionEdit();
        }

        internal void RedoForValidation()
        {
            RedoSessionEdit();
        }

        internal bool SetSelectedColorRuleTargetForValidation(Color32 targetColor)
        {
            PaletteColorEntry entry = session.paletteColors.FirstOrDefault(candidate => candidate != null && candidate.id == selectedColorEntryId);
            if (entry == null)
            {
                return false;
            }

            ColorReplacementRule rule = GetOrCreateColorRule(entry);
            RecordSessionEdit(() =>
            {
                rule.targetColor = targetColor;
                rule.enabled = true;
                EnsureGroupSupportsColorRule(entry);
            });
            return true;
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
            if (!selectionHighlightEnabled || Event.current.type != EventType.Repaint || readableSourceImage == null)
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

            if (!selectionHighlightEnabled)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(selectedColorEntryId) && string.IsNullOrEmpty(selectedGroupId))
            {
                return string.Empty;
            }

            return string.Join(
                "|",
                GetTextureCacheId(readableSourceImage),
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
                "parameterHelpDefault" => "パラメータにマウスを重ねると、値の増減による見た目の変化を表示します。",
                "analysisPresetHelp" => "解析設定の初期値をまとめて切り替えます。先にカテゴリを選び、その後に個別パラメータを微調整してください。",
                "alphaThresholdHelp" => "値を上げると半透明の縁が透明扱いになり、形はきれいになりますが細く見える場合があります。下げるとアンチエイリアスや淡い光を残しやすくなります。",
                "minimumPixelCountHelp" => "値を上げると小さな色を無視してパレットのノイズが減ります。下げると小さな差し色や1ピクセルのディテールを残します。",
                "quantizeStepHelp" => "値を上げると近い色がまとまり、色数が減って滑らかなグループになります。下げると元画像に近い細かな色やグラデーションを残します。",
                "maxPaletteColorsHelp" => "値を上げると抽出する色数が増えます。下げるとパレットが早めに打ち切られ、編集対象を絞れます。",
                "targetGroupCountHelp" => "値を上げると置換グループが細かく分かれ、個別調整しやすくなります。下げると似た色がまとまり、広い範囲が一括で変わります。",
                "distanceModeHelp" => "RGBは直接的で高速、HSVは色相の近さを重視、Labは人の見た目に近い色差で近い色を判定します。",
                "maxColorDistanceHelp" => "値を上げると離れた色も同じグループになり、置換範囲が広がります。下げると近い色だけがまとまり、見た目の変化が局所的になります。",
                "preserveDarkOutlineHelp" => "有効にすると黒や暗い輪郭線が明るいグループに吸収されにくくなります。無効にすると輪郭も周囲の素材色と一緒に置換されます。",
                "preserveAlphaHelp" => "有効にすると通常の色置換では元の不透明度を保ちます。無効にすると置換色のアルファ値が書き出し結果にそのまま反映されます。",
                "edgeCleanupEnabledHelp" => "有効にすると前景の外側に残った色にじみを削除します。見た目では、不透明背景の素材の縁にある不要な点が消えます。",
                "edgeCleanupModeHelp" => "Boundary Trimは推定した前景境界を細く削ります。外側クリーンアップは境界外の小さな領域を対象にします。",
                "edgeCleanupDistanceHelp" => "値を上げると境界の外側をより遠くまで探索して色にじみを消します。上げすぎると近くの意図したディテールも消える場合があります。",
                "edgeCleanupMaxRegionHelp" => "値を上げると大きめの外側領域も削除対象になります。下げると小さな点だけを削除します。",
                "edgeTrimDistanceHelp" => "値を上げると境界から削る幅が増え、シルエットが締まります。下げると縁のアンチエイリアスを残しやすくなります。",
                "noiseRemovalEnabledHelp" => "有効にすると小さく孤立した色領域を周囲の近い色で埋めます。見た目では、プレビューや書き出しの細かな斑点が減ります。",
                "maxNoiseRegionPixelsHelp" => "値を上げると大きめの島もノイズとして扱います。下げると小さなハイライトや意図した点を保護しやすくなります。",
                "noiseNeighborThresholdHelp" => "値を上げると少し離れた色でもノイズの埋め色に使います。下げると見た目が近い色だけで補正します。",
                "sameGroupOnlyHelp" => "有効にすると同じグループ内の色だけでノイズを埋め、素材の境界を保ちます。無効にするとより強くノイズを消します。",
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
                "selectionHighlight" => "選択色をハイライト",
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
                "edgeOutsideCleanup" => "エッジ外側クリーンアップ",
                "edgeCleanupEnabled" => "エッジ外側クリーンアップを有効化",
                "edgeCleanupDistance" => "外側距離",
                "edgeCleanupMaxRegion" => "最大外側領域",
                "noiseRemoval" => "ノイズ削除",
                "noiseRemovalEnabled" => "ノイズ削除を有効化",
                "maxNoiseRegionPixels" => "最大ノイズサイズ",
                "noiseNeighborThreshold" => "近傍判定しきい値",
                "sameGroupOnly" => "同一グループ内のみ",
                "noiseRemovalHint" => "同一グループ内に囲まれた小さな色の塊を、周囲の近傍色で埋めます。",
                "exportSettings" => "書き出し設定",
                "showExportOptions" => "書き出し設定を表示",
                "hideExportOptions" => "書き出し設定を隠す",
                "outputFolder" => "出力フォルダ",
                "filePrefix" => "ファイル接頭辞",
                "fileSuffix" => "ファイル接尾辞",
                "conflictMode" => "競合時の処理",
                "refreshAssetDatabase" => "AssetDatabase更新",
                "batchExport" => "フォルダ一括処理",
                "sourceFolder" => "ソースフォルダ",
                "exportFolder" => "フォルダを書き出し",
                "presetAsset" => "プリセットアセット",
                "createPresetAsset" => "アセット作成",
                "updatePresetAsset" => "アセット更新",
                "loadPresetAsset" => "アセット読込",
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
                "selectedColorInfo" => "選択色情報",
                "noSelectedColor" => "パレット色が選択されていません。",
                "group" => "グループ",
                "pixelCount" => "ピクセル数",
                "replacementColor" => "置換後色",
                "colorRuleState" => "色別ルール",
                "transparentReplacement" => "透明置換",
                "disabled" => "無効",
                "notAppliedByMode" => "現在のモードでは未適用",
                "yes" => "はい",
                "no" => "いいえ",
                "enabled" => "有効",
                "helpOverview" => "画像を解析し、近い色をグループ化して、置換色のプレビューとPNG書き出しを行います。",
                "helpAnalysis" => "透明度しきい値、最小ピクセル数、量子化ステップで抽出するパレット色を調整します。",
                "helpGrouping" => "目標グループ数と近傍色しきい値で、似た色をどこまで同じグループに含めるかを調整します。",
                "helpNoiseRemoval" => "同一グループ内に囲まれた小さな色領域を検出し、周囲の近傍色で補正します。元画像は変更しません。",
                "helpPalette" => "パレット行またはグループ行を選択すると、プレビュー上で該当色がハイライトされます。",
                "helpRules" => "Group Uniformはグループ単位、Per Colorは色別ルールのみ、Hybridは色別ルールを優先して不足分をグループ設定で補います。",
                "helpExport" => "Previewを更新してからExportすると、設定したフォルダにPNGを書き出します。",
                "toolSettings" => "ツール設定",
                "paintTarget" => "編集対象",
                "tool" => "ツール",
                "strength" => "強さ",
                "paintOpacity" => "描画不透明度",
                "paintColor" => "描画色",
                "noiseRegion" => "ノイズ領域",
                "noiseThreshold" => "ノイズ閾値",
                "blurRadius" => "ぼかし半径",
                "smoothIterations" => "スムース回数",
                "toolSettingsHint" => "Preview Mode を Paint にすると、選択中の対象をプレビュー上で直接編集できます。",
                "layers" => "レイヤー",
                "addPaintLayer" => "描画レイヤー追加",
                "addImageLayer" => "画像レイヤー追加",
                "duplicateLayer" => "レイヤー複製",
                "moveUp" => "上へ移動",
                "moveDown" => "下へ移動",
                "deleteLayer" => "レイヤー削除",
                "previewMode" => "プレビューモード",
                "clickPick" => "クリック: 選択",
                _ => english
            };
        }

        private void OnDisable()
        {
            CancelScheduledAutoPreview();
            CancelQueuedPreviewRefresh();
            DestroyReadableSourceImage();
            DestroyReplacementPreview();
            DestroyAfterPreview();
            DestroySplitPreviewTexture();
            DestroySelectionOverlayTexture();
            DestroyUiToolkitHighlightTextures();
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
            DestroyReplacementPreview();
            DestroySplitPreviewTexture();
            DestroyUiToolkitHighlightTextures();
            InvalidateSelectionOverlay();
        }

        private void DestroyReplacementPreview()
        {
            if (replacementPreview == null)
            {
                return;
            }

            DestroyImmediate(replacementPreview);
            replacementPreview = null;
        }

        private void DestroyAfterPreview()
        {
            InvalidateAfterPreviewPresentationCaches();
            if (afterPreview == null)
            {
                return;
            }

            DestroyImmediate(afterPreview);
            afterPreview = null;
        }

        private void InvalidateAfterPreviewPresentationCaches()
        {
            lastNoiseEffectHighlightIndices.Clear();
            lastEdgeEffectHighlightIndices.Clear();
            DestroySplitPreviewTexture();
            DestroyDiffPreviewTexture();
            DestroyUiToolkitHighlightTextures();
        }

        private void UpdatePreviewImagesImmediately()
        {
            if (beforePreviewImage != null)
            {
                if (!ShouldUseSplitPreviewDisplay())
                {
                    DestroySplitPreviewTexture();
                }

                beforePreviewImage.image = GetPrimaryPreviewDisplayTexture();
            }

            if (afterPreviewImage != null)
            {
                afterPreviewImage.image = GetDisplayPreviewTexture(afterPreview, HighlightPreviewSlot.After);
                afterPreviewImage.style.display = ShouldShowSecondaryPreviewPane()
                    ? DisplayStyle.None
                    : DisplayStyle.Flex;
            }
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

        internal ExportSettings CurrentExportSettings => session.exportSettings;

        internal string CurrentBatchSourceFolder
        {
            get => batchSourceFolder;
            set => batchSourceFolder = value ?? string.Empty;
        }

        internal IReadOnlyList<BatchSourceExportItem> CurrentBatchResults => batchResults;

        internal bool CanExportPreview => afterPreview != null;

        internal bool CanExportAllVariations => readableSourceImage != null && session.variations.Count > 0;

        internal bool CanExportBatchFolder => session.colorGroups.Count > 0 && session.variations.Count > 0;

        internal void OpenExportSettingsWindow()
        {
            PaletteVariantExportSettingsWindow window = GetWindow<PaletteVariantExportSettingsWindow>(T("exportSettings", "Export Settings"));
            window.SetOwner(this);
            window.Show();
            window.Focus();
        }

        internal void SelectOutputFolderFromSettingsWindow()
        {
            SelectOutputFolder();
            RefreshUiToolkitContent();
        }

        internal void SelectBatchSourceFolderFromSettingsWindow()
        {
            SelectBatchSourceFolder();
            RefreshUiToolkitContent();
        }

        internal void ExportPreviewFromSettingsWindow()
        {
            ExportPreview();
            RefreshUiToolkitContent();
        }

        internal void ExportAllVariationsFromSettingsWindow()
        {
            ExportAllVariations();
            RefreshUiToolkitContent();
        }

        internal void ExportBatchSourceFolderFromSettingsWindow()
        {
            ExportBatchSourceFolder();
            RefreshUiToolkitContent();
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
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (asset == null)
            {
                return;
            }

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        private readonly struct PaletteRuleBadge
        {
            public PaletteRuleBadge(string text, Color color)
            {
                Text = text;
                Color = color;
            }

            public string Text { get; }

            public Color Color { get; }
        }
    }

    internal enum PreviewCompareMode
    {
        SideBySide,
        Split,
        Difference
    }

    internal enum PreviewInteractionMode
    {
        Pick,
        Pan,
        BrushSelect,
        Paint
    }

    internal enum AnalysisCategoryPreset
    {
        TransparentPng,
        WhiteBackgroundJpg,
        LineArtIcon,
        Gem,
        Plant
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

    internal enum HighlightPreviewSlot
    {
        Before,
        After,
        Split
    }

    internal sealed class PaletteVariantExportSettingsWindow : EditorWindow
    {
        private PaletteVariantGeneratorWindow owner;
        private Vector2 scroll;

        public void SetOwner(PaletteVariantGeneratorWindow window)
        {
            owner = window;
            minSize = new Vector2(440f, 360f);
        }

        private void OnGUI()
        {
            if (owner == null)
            {
                EditorGUILayout.HelpBox("Open Palette Variant Generator first.", MessageType.Info);
                return;
            }

            ExportSettings settings = owner.CurrentExportSettings;
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField(owner.T("exportSettings", "Export Settings"), EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            settings.outputFolder = EditorGUILayout.TextField(owner.T("outputFolder", "Output Folder"), settings.outputFolder);
            if (GUILayout.Button("...", GUILayout.Width(32f)))
            {
                owner.SelectOutputFolderFromSettingsWindow();
            }

            EditorGUILayout.EndHorizontal();
            settings.filePrefix = EditorGUILayout.TextField(owner.T("filePrefix", "File Prefix"), settings.filePrefix);
            settings.fileSuffix = EditorGUILayout.TextField(owner.T("fileSuffix", "File Suffix"), settings.fileSuffix);
            settings.conflictMode = (ExportConflictMode)EditorGUILayout.EnumPopup(owner.T("conflictMode", "Conflict Mode"), settings.conflictMode);
            settings.refreshAssetDatabase = EditorGUILayout.Toggle(owner.T("refreshAssetDatabase", "Refresh AssetDatabase"), settings.refreshAssetDatabase);

            EditorGUILayout.Space(8f);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginDisabledGroup(!owner.CanExportPreview);
                if (GUILayout.Button(owner.T("export", "Export"), GUILayout.Height(28f)))
                {
                    owner.ExportPreviewFromSettingsWindow();
                    Repaint();
                }

                EditorGUI.EndDisabledGroup();
                EditorGUI.BeginDisabledGroup(!owner.CanExportAllVariations);
                if (GUILayout.Button(owner.T("exportAll", "Export All"), GUILayout.Height(28f)))
                {
                    owner.ExportAllVariationsFromSettingsWindow();
                    Repaint();
                }

                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField(owner.T("batchExport", "Batch Export"), EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            owner.CurrentBatchSourceFolder = EditorGUILayout.TextField(owner.T("sourceFolder", "Source Folder"), owner.CurrentBatchSourceFolder);
            if (GUILayout.Button("...", GUILayout.Width(32f)))
            {
                owner.SelectBatchSourceFolderFromSettingsWindow();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUI.BeginDisabledGroup(!owner.CanExportBatchFolder);
            if (GUILayout.Button(owner.T("exportFolder", "Export Folder"), GUILayout.Height(28f)))
            {
                owner.ExportBatchSourceFolderFromSettingsWindow();
                Repaint();
            }

            EditorGUI.EndDisabledGroup();

            foreach (BatchSourceExportItem item in owner.CurrentBatchResults.Take(20))
            {
                EditorGUILayout.LabelField($"{item.Status}: {item.AssetPath} / {item.VariationName}", EditorStyles.miniLabel);
            }

            EditorGUILayout.EndScrollView();
        }
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
            DrawHelpSection("Noise Removal", owner.T("helpNoiseRemoval", "Detects small color islands surrounded by the same group and fills them with nearby surrounding colors. The source image is not modified."));
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
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("パッケージ", PaletteVariantGeneratorWindow.PackageName);
            EditorGUILayout.LabelField("バージョン", PaletteVariantGeneratorWindow.PackageVersion);
            EditorGUILayout.LabelField("検証済み Unity", PaletteVariantGeneratorWindow.ValidatedUnityVersion);
            EditorGUILayout.LabelField("メニュー", "Tools > Palette Variant Generator > メイン画面");
            EditorGUILayout.LabelField("ライセンス", "MIT License");
            EditorGUILayout.Space(8f);
            EditorGUILayout.TextField("リポジトリ", PaletteVariantGeneratorWindow.RepositoryUrl);
            EditorGUILayout.TextField("リリース", PaletteVariantGeneratorWindow.ReleaseUrl);
        }

        private static void DrawLicense()
        {
            EditorGUILayout.LabelField("ライセンス", EditorStyles.boldLabel);
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("ライセンス種別", "MIT License");
            EditorGUILayout.LabelField("対象パッケージ", PaletteVariantGeneratorWindow.PackageName);
            EditorGUILayout.Space(8f);
            EditorGUILayout.HelpBox(
                "本 Unity エディタ拡張は MIT License で提供されます。利用、改変、再配布、商用利用が可能です。",
                MessageType.None);
            EditorGUILayout.HelpBox(
                "再配布時は、パッケージに含まれる LICENSE.md の著作権表示とライセンス本文を保持してください。",
                MessageType.Info);
            EditorGUILayout.TextField("LICENSE", $"Packages/{PaletteVariantGeneratorWindow.PackageName}/LICENSE.md");
        }

        private enum InfoMode
        {
            Version,
            License
        }
    }
}
