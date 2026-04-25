# UI Toolkit 移行メモ

## 現在の判断

本番メインウィンドウは UI Toolkit root + ScrollView の構成へ移行した。

- Toolbar、Source、Analyze、Group、Edge Outside Cleanup、Noise Removal、Preview、Palette、Variations、Replacement Rules、Color Rules、Export、Preset、Batch Export の主要セクションは UI Toolkit の VisualElement で構成する。
- Export Settings と Folder Batch Export は常時表示せず、Export 操作時または `書き出し設定を表示` から開く。
- Analyze、Auto Group、Preview、Export、Export All、Session、Preset、Batch Export は既存サービス処理を UI Toolkit 側の操作から呼び出す。
- 旧 IMGUI 描画メソッドは互換・参照用に残すが、通常の本番 Window には `IMGUIContainer` を配置しない。
- UI Toolkit preview window は評価用コードとして残すが、通常メニューには表示しない。

## 検証

- `ISSUE23_UI_TOOLKIT_PREVIEW_VALIDATION=PASS`: 評価用 UI Toolkit preview window の主要セクション構成を検証する。
- `ISSUE23_UI_TOOLKIT_INTERACTION_VALIDATION=PASS`: ObjectField、ColorField、PopupField、ScrollView、Slider、preview pan / wheel zoom / split compare の入力受け口を検証する。
- `ISSUE23_MAIN_WINDOW_UI_TOOLKIT_HOST_VALIDATION=PASS`: 本番 main window が UI Toolkit root と ScrollView で開くことを検証する。
- `ISSUE25_PREVIEW_MENU_HIDDEN_VALIDATION=PASS`: 評価用 preview window が通常メニューに露出していないことを検証する。
- `ISSUE25_UI_TOOLKIT_PRODUCTION_VALIDATION=PASS`: 本番 main window が `IMGUIContainer` に依存せず、主要セクションと標準 UI Toolkit controls を持つことを検証する。

## 維持する操作

- `Tools > Palette Variant Generator > 開く` からの起動
- Source Image の選択と Analyze
- Auto Group と近傍色しきい値の調整
- エッジ外側クリーンアップとノイズ削除の有効化、しきい値、対象サイズの調整
- Preview、Auto Preview、Zoom、Split compare
- Palette と Group のスクロール表示、選択
- Variations の追加、複製、削除、Export 対象切り替え
- Group / PerColor / Hybrid の置換ルール
- Export、Export All、Folder Batch Export
- JSON Session、JSON Preset、ScriptableObject Preset Asset
- Help、Language、Version、License

## 手動 QA 観点

- wide / compact / docked の各幅で横スクロールが出にくいこと。
- Source 情報や長い asset path が折り返され、見切れないこと。
- Preview / Export / Export All の出力が IMGUI 版から退行していないこと。
- ColorField / ObjectField の入力が Unity 6000.4.0f1 上で安定していること。
- UI Toolkit preview window が通常メニューに出ていないこと。
