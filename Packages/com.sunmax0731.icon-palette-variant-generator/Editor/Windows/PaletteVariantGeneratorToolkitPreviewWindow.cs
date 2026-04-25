using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sunmax0731.IconPaletteVariantGenerator.Editor.Windows
{
    /// <summary>
    /// UI Toolkit layout prototype used to validate the future main-window migration.
    /// </summary>
    public sealed class PaletteVariantGeneratorToolkitPreviewWindow : EditorWindow
    {
        internal const string MenuPath = "Tools/Palette Variant Generator/UI Toolkitプレビュー";
        internal const string WindowTitle = "Palette Variant Generator UI Toolkitプレビュー";

        internal static readonly string[] RequiredSectionNames =
        {
            "source-section",
            "analyze-section",
            "group-section",
            "export-section",
            "preset-section",
            "preview-section",
            "palette-section",
            "variation-section",
            "replacement-rule-section",
            "color-rule-section"
        };

        internal static readonly string[] RequiredControlNames =
        {
            "source-image-field",
            "alpha-threshold-slider",
            "quantize-step-slider",
            "max-palette-colors-field",
            "distance-mode-popup",
            "near-color-threshold-slider",
            "export-folder-field",
            "preset-object-field",
            "compare-mode-popup",
            "preview-zoom-slider",
            "preview-split-slider",
            "palette-scroll-view",
            "group-color-field",
            "color-rule-color-field"
        };

        private Label statusLabel;

        public static void Open()
        {
            PaletteVariantGeneratorToolkitPreviewWindow window = GetWindow<PaletteVariantGeneratorToolkitPreviewWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(760f, 540f);
            window.Show();
        }

        public void CreateGUI()
        {
            rootVisualElement.Clear();
            VisualElement previewRoot = BuildPreviewRoot(CreateStatusUpdater());
            rootVisualElement.Add(previewRoot);
            statusLabel = previewRoot.Q<Label>("preview-status");
        }

        internal static VisualElement BuildPreviewRoot()
        {
            return BuildPreviewRoot(null);
        }

        internal static bool ContainsRequiredSections(VisualElement root)
        {
            return ContainsRequiredElements(root, RequiredSectionNames);
        }

        internal static bool ContainsRequiredControls(VisualElement root)
        {
            return ContainsRequiredElements(root, RequiredControlNames);
        }

        internal static bool SupportsPreviewInteractions(VisualElement root)
        {
            return root != null
                && root.userData is PreviewInteractionState state
                && state.SupportsPointerPan
                && state.SupportsWheelZoom
                && state.SupportsSplitCompare
                && state.HidesHorizontalScroll;
        }

        private System.Action<string> CreateStatusUpdater()
        {
            return message =>
            {
                if (statusLabel != null)
                {
                    statusLabel.text = message;
                }
            };
        }

        private static bool ContainsRequiredElements(VisualElement root, IEnumerable<string> names)
        {
            if (root == null)
            {
                return false;
            }

            foreach (string elementName in names)
            {
                if (root.Q<VisualElement>(elementName) == null)
                {
                    return false;
                }
            }

            return true;
        }

        private static VisualElement BuildPreviewRoot(System.Action<string> setStatus)
        {
            VisualElement root = new VisualElement { name = "ui-toolkit-preview-root" };
            root.userData = new PreviewInteractionState
            {
                SupportsPointerPan = true,
                SupportsWheelZoom = true,
                SupportsSplitCompare = true,
                HidesHorizontalScroll = true
            };
            root.style.flexGrow = 1f;
            root.style.flexDirection = FlexDirection.Column;
            root.style.paddingLeft = 8f;
            root.style.paddingRight = 8f;
            root.style.paddingTop = 8f;
            root.style.paddingBottom = 8f;

            Label notice = new Label("UI Toolkit移行評価用のプレビューです。本番作業はIMGUI版のウィンドウを使用してください。");
            notice.name = "migration-preview-notice";
            notice.style.unityFontStyleAndWeight = FontStyle.Bold;
            notice.style.marginBottom = 6f;
            root.Add(notice);

            Toolbar toolbar = new Toolbar { name = "preview-toolbar" };
            ObjectField sourceField = new ObjectField("ソース画像")
            {
                name = "source-image-field",
                objectType = typeof(Texture2D),
                allowSceneObjects = false
            };
            sourceField.style.minWidth = 220f;
            sourceField.style.flexGrow = 1f;
            toolbar.Add(sourceField);
            toolbar.Add(CreateToolbarButton("Analyze", setStatus));
            toolbar.Add(CreateToolbarButton("Auto Group", setStatus));
            toolbar.Add(CreateToolbarButton("Preview", setStatus));
            toolbar.Add(CreateToolbarButton("Export", setStatus));
            toolbar.Add(CreateToolbarButton("Export All", setStatus));
            toolbar.Add(CreateToolbarButton("Help", setStatus));
            root.Add(toolbar);

            ScrollView scrollView = new ScrollView(ScrollViewMode.Vertical) { name = "preview-scroll" };
            scrollView.style.flexGrow = 1f;
            scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;

            VisualElement content = new VisualElement { name = "preview-content" };
            content.style.flexDirection = FlexDirection.Row;
            content.style.flexWrap = Wrap.Wrap;
            content.style.alignItems = Align.Stretch;
            content.style.marginTop = 8f;
            content.style.marginRight = 0f;

            VisualElement settingsColumn = CreateColumn("settings-column", 280f, 1f);
            settingsColumn.Add(CreateSourceSection());
            settingsColumn.Add(CreateAnalyzeSection());
            settingsColumn.Add(CreateGroupSection());
            settingsColumn.Add(CreateExportSection());
            settingsColumn.Add(CreatePresetSection());

            VisualElement previewColumn = CreateColumn("preview-column", 360f, 2f);
            previewColumn.Add(CreatePreviewSection(setStatus));
            previewColumn.Add(CreatePaletteSection());

            VisualElement rulesColumn = CreateColumn("rules-column", 360f, 1f);
            rulesColumn.Add(CreateVariationSection());
            rulesColumn.Add(CreateReplacementRuleSection());
            rulesColumn.Add(CreateColorRuleSection());

            content.Add(settingsColumn);
            content.Add(previewColumn);
            content.Add(rulesColumn);
            scrollView.Add(content);
            root.Add(scrollView);

            Label status = new Label("UI Toolkit移行確認用のレイアウトを表示しています。") { name = "preview-status" };
            status.style.marginTop = 6f;
            root.Add(status);

            return root;
        }

        private static VisualElement CreateAnalyzeSection()
        {
            VisualElement section = CreateSection("analyze-section", "解析設定");
            section.Add(CreateSliderInt("alpha-threshold-slider", "アルファしきい値", 8, 0, 255));
            section.Add(CreateSliderInt("quantize-step-slider", "量子化ステップ", 8, 1, 64));
            section.Add(new IntegerField("最大パレット色数") { name = "max-palette-colors-field", value = 64 });
            return section;
        }

        private static VisualElement CreateGroupSection()
        {
            VisualElement section = CreateSection("group-section", "グループ設定");
            section.Add(CreateSliderInt("target-group-count-slider", "目標グループ数", 8, 1, 64));
            section.Add(new PopupField<string>("色距離モード", new List<string> { "RGB", "HSV", "Lab" }, 2) { name = "distance-mode-popup" });
            section.Add(CreateSlider("near-color-threshold-slider", "近傍色しきい値", 64f, 0f, 441f));
            return section;
        }

        private static VisualElement CreateExportSection()
        {
            VisualElement section = CreateSection("export-section", "出力設定");
            section.Add(new TextField("出力フォルダ") { name = "export-folder-field", value = "GeneratedIcons" });
            section.Add(new TextField("ファイル接頭辞") { name = "file-prefix-field", value = "icon" });
            section.Add(new PopupField<string>("競合時の処理", new List<string> { "複製", "上書き", "スキップ" }, 0) { name = "conflict-mode-popup" });
            return section;
        }

        private static VisualElement CreatePresetSection()
        {
            VisualElement section = CreateSection("preset-section", "プリセットアセット");
            section.Add(new ObjectField("プリセット") { name = "preset-object-field", objectType = typeof(ScriptableObject), allowSceneObjects = false });
            section.Add(CreateButtonRow("適用", "更新", "保存"));
            return section;
        }

        private static VisualElement CreateSourceSection()
        {
            VisualElement section = CreateSection("source-section", "ソース情報");
            Label assetPath = new Label("アセットパス: 未選択") { name = "source-asset-path-label" };
            assetPath.style.whiteSpace = WhiteSpace.Normal;
            section.Add(assetPath);
            section.Add(CreateRowLabel("サイズ: 未解析"));
            section.Add(CreateRowLabel("読み込み状態: 未確認"));
            return section;
        }

        private static VisualElement CreatePreviewSection(System.Action<string> setStatus)
        {
            VisualElement section = CreateSection("preview-section", "プレビュー");
            section.Add(new PopupField<string>("比較モード", new List<string> { "横並び", "分割", "Afterのみ" }, 0) { name = "compare-mode-popup" });
            section.Add(CreateSlider("preview-zoom-slider", "ズーム", 1f, 1f, 8f));
            section.Add(CreateSlider("preview-split-slider", "分割位置", 0.5f, 0f, 1f));
            section.Add(CreateButtonRow("表示リセット"));

            VisualElement row = new VisualElement { name = "preview-placeholder-row" };
            row.style.flexDirection = FlexDirection.Row;
            row.style.flexWrap = Wrap.Wrap;
            row.Add(CreatePreviewPlaceholder("before-preview-placeholder", "Before", setStatus));
            row.Add(CreatePreviewPlaceholder("after-preview-placeholder", "After", setStatus));
            section.Add(row);
            return section;
        }

        private static VisualElement CreatePaletteSection()
        {
            VisualElement section = CreateSection("palette-section", "パレット");
            ScrollView paletteScroll = new ScrollView(ScrollViewMode.Vertical) { name = "palette-scroll-view" };
            paletteScroll.style.height = 150f;
            paletteScroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            for (int index = 0; index < 8; index++)
            {
                paletteScroll.Add(CreatePaletteRow(index));
            }

            section.Add(paletteScroll);
            return section;
        }

        private static VisualElement CreateVariationSection()
        {
            VisualElement section = CreateSection("variation-section", "バリエーション");
            section.Add(new Toggle("出力対象") { name = "variation-export-toggle", value = true });
            section.Add(new TextField("ファイル接尾辞") { name = "variation-suffix-field", value = "blue" });
            section.Add(CreateButtonRow("追加", "複製", "削除"));
            return section;
        }

        private static VisualElement CreateReplacementRuleSection()
        {
            VisualElement section = CreateSection("replacement-rule-section", "置換ルール");
            section.Add(new ColorField("グループ色") { name = "group-color-field", value = Color.cyan });
            section.Add(CreateSlider("group-blend-slider", "ブレンド率", 1f, 0f, 1f));
            section.Add(new PopupField<string>("置換モード", new List<string> { "Group Uniform", "Per Color", "Hybrid" }, 2) { name = "replacement-mode-popup" });
            return section;
        }

        private static VisualElement CreateColorRuleSection()
        {
            VisualElement section = CreateSection("color-rule-section", "色別ルール");
            section.Add(new ColorField("上書き色") { name = "color-rule-color-field", value = Color.magenta });
            section.Add(CreateSlider("color-rule-blend-slider", "ブレンド率", 1f, 0f, 1f));
            section.Add(new Toggle("有効") { name = "color-rule-enabled-toggle", value = true });
            return section;
        }

        private static VisualElement CreateSection(string name, string title)
        {
            VisualElement section = new VisualElement { name = name };
            section.style.borderTopWidth = 1f;
            section.style.borderRightWidth = 1f;
            section.style.borderBottomWidth = 1f;
            section.style.borderLeftWidth = 1f;
            section.style.borderTopColor = new Color(0.22f, 0.22f, 0.22f);
            section.style.borderRightColor = new Color(0.22f, 0.22f, 0.22f);
            section.style.borderBottomColor = new Color(0.22f, 0.22f, 0.22f);
            section.style.borderLeftColor = new Color(0.22f, 0.22f, 0.22f);
            section.style.borderTopLeftRadius = 4f;
            section.style.borderTopRightRadius = 4f;
            section.style.borderBottomLeftRadius = 4f;
            section.style.borderBottomRightRadius = 4f;
            section.style.paddingLeft = 8f;
            section.style.paddingRight = 8f;
            section.style.paddingTop = 6f;
            section.style.paddingBottom = 6f;
            section.style.marginBottom = 8f;

            Label titleLabel = new Label(title) { name = name + "-title" };
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.marginBottom = 4f;
            section.Add(titleLabel);

            return section;
        }

        private static Button CreateToolbarButton(string text, System.Action<string> setStatus)
        {
            Button button = new Button(() => setStatus?.Invoke(text + " は本番IMGUIウィンドウで操作してください。"))
            {
                text = text
            };
            button.style.minWidth = 76f;
            return button;
        }

        private static VisualElement CreateButtonRow(params string[] labels)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.flexWrap = Wrap.Wrap;
            row.style.marginTop = 4f;
            foreach (string label in labels)
            {
                Button button = new Button { text = label };
                button.style.marginRight = 4f;
                row.Add(button);
            }

            return row;
        }

        private static SliderInt CreateSliderInt(string name, string label, int value, int lowValue, int highValue)
        {
            SliderInt slider = new SliderInt(label, lowValue, highValue)
            {
                name = name,
                value = value,
                showInputField = true
            };
            return slider;
        }

        private static Slider CreateSlider(string name, string label, float value, float lowValue, float highValue)
        {
            Slider slider = new Slider(label, lowValue, highValue)
            {
                name = name,
                value = value,
                showInputField = true
            };
            return slider;
        }

        private static VisualElement CreateColumn(string name, float minWidth, float flexGrow)
        {
            VisualElement column = new VisualElement { name = name };
            column.style.minWidth = minWidth;
            column.style.flexGrow = flexGrow;
            column.style.flexBasis = 0f;
            column.style.marginRight = 8f;
            return column;
        }

        private static Label CreateRowLabel(string text)
        {
            Label label = new Label(text);
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.marginTop = 2f;
            return label;
        }

        private static VisualElement CreatePaletteRow(int index)
        {
            VisualElement row = new VisualElement { name = "palette-row-" + index };
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = 4f;

            VisualElement swatch = new VisualElement();
            swatch.style.width = 18f;
            swatch.style.height = 18f;
            swatch.style.marginRight = 6f;
            swatch.style.backgroundColor = Color.HSVToRGB(index / 8f, 0.7f, 0.9f);
            row.Add(swatch);
            row.Add(CreateRowLabel("#" + index.ToString("00") + "  グループ未設定  128 px"));
            return row;
        }

        private static VisualElement CreatePreviewPlaceholder(string name, string labelText, System.Action<string> setStatus)
        {
            VisualElement placeholder = new VisualElement { name = name };
            placeholder.style.height = 180f;
            placeholder.style.minWidth = 180f;
            placeholder.style.flexGrow = 1f;
            placeholder.style.marginRight = 6f;
            placeholder.style.marginTop = 4f;
            placeholder.style.backgroundColor = new Color(0.13f, 0.13f, 0.13f);
            placeholder.style.justifyContent = Justify.Center;
            placeholder.style.alignItems = Align.Center;
            placeholder.RegisterCallback<PointerDownEvent>(_ => setStatus?.Invoke(labelText + " プレビューのパン操作を受け付けました。"));
            placeholder.RegisterCallback<PointerMoveEvent>(_ => { });
            placeholder.RegisterCallback<WheelEvent>(_ => setStatus?.Invoke(labelText + " プレビューのズーム操作を受け付けました。"));

            Label label = new Label(labelText);
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            placeholder.Add(label);
            return placeholder;
        }

        internal sealed class PreviewInteractionState
        {
            public bool SupportsPointerPan { get; set; }
            public bool SupportsWheelZoom { get; set; }
            public bool SupportsSplitCompare { get; set; }
            public bool HidesHorizontalScroll { get; set; }
        }
    }
}
