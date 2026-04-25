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

        private Label statusLabel;

        [MenuItem(MenuPath)]
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
            if (root == null)
            {
                return false;
            }

            foreach (string sectionName in RequiredSectionNames)
            {
                if (root.Q<VisualElement>(sectionName) == null)
                {
                    return false;
                }
            }

            return true;
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

        private static VisualElement BuildPreviewRoot(System.Action<string> setStatus)
        {
            VisualElement root = new VisualElement { name = "ui-toolkit-preview-root" };
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
            settingsColumn.Add(CreateSection("analyze-section", "解析設定", "アルファしきい値", "量子化ステップ", "最大パレット色数"));
            settingsColumn.Add(CreateSection("group-section", "グループ設定", "目標グループ数", "色距離モード", "近傍色しきい値"));
            settingsColumn.Add(CreateSection("export-section", "出力設定", "出力フォルダ", "ファイル接頭辞", "競合時の処理"));
            settingsColumn.Add(CreateSection("preset-section", "プリセットアセット", "プリセット ObjectField", "適用", "更新"));

            VisualElement previewColumn = CreateColumn("preview-column", 360f, 2f);
            previewColumn.Add(CreatePreviewSection());
            previewColumn.Add(CreatePaletteSection());

            VisualElement rulesColumn = CreateColumn("rules-column", 360f, 1f);
            rulesColumn.Add(CreateSection("variation-section", "バリエーション", "追加", "複製", "出力対象"));
            rulesColumn.Add(CreateSection("replacement-rule-section", "置換ルール", "グループ色", "ブレンド率", "置換モード"));
            rulesColumn.Add(CreateSection("color-rule-section", "色別ルール", "パレット色", "上書き色", "有効"));

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

        private static Button CreateToolbarButton(string text, System.Action<string> setStatus)
        {
            Button button = new Button(() =>
            {
                if (setStatus != null)
                {
                    setStatus(text + " は本番IMGUIウィンドウで操作してください。");
                }
            })
            {
                text = text
            };
            button.style.minWidth = 76f;
            return button;
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

        private static VisualElement CreateSourceSection()
        {
            VisualElement section = CreateSection("source-section", "ソース情報", "アセットパス", "サイズ", "読み込み状態");
            return section;
        }

        private static VisualElement CreatePreviewSection()
        {
            VisualElement section = CreateSection("preview-section", "プレビュー", "比較モード", "ズーム", "表示リセット");
            VisualElement row = new VisualElement { name = "preview-placeholder-row" };
            row.style.flexDirection = FlexDirection.Row;
            row.style.flexWrap = Wrap.Wrap;
            row.Add(CreatePreviewPlaceholder("before-preview-placeholder", "Before"));
            row.Add(CreatePreviewPlaceholder("after-preview-placeholder", "After"));
            section.Add(row);
            return section;
        }

        private static VisualElement CreatePaletteSection()
        {
            VisualElement section = CreateSection("palette-section", "パレット", "スクロール可能な色一覧", "選択中グループ", "ピクセル数");
            section.style.minHeight = 180f;
            return section;
        }

        private static VisualElement CreateSection(string name, string title, params string[] rows)
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

            foreach (string row in rows)
            {
                Label label = new Label(row) { name = name + "-" + row.ToLowerInvariant().Replace(" ", "-") };
                label.style.whiteSpace = WhiteSpace.Normal;
                label.style.marginTop = 2f;
                section.Add(label);
            }

            return section;
        }

        private static VisualElement CreatePreviewPlaceholder(string name, string labelText)
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

            Label label = new Label(labelText);
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            placeholder.Add(label);
            return placeholder;
        }
    }
}
