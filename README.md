# Unity Icon Palette Variant Generator

アイコン画像からパレット色を抽出し、近傍色グループと置換ルールを使って色違い PNG を生成する Unity Editor 拡張です。

## Unity バージョン

- Unity 6000.4.0f1

## パッケージ

- Package: `com.sunmax0731.icon-palette-variant-generator`
- Version: `1.0.2`
- 配布形式: UPM package ZIP

## 現在の対応範囲

現在の実装では、MVP として以下の編集ワークフローに対応しています。

- `Packages/com.sunmax0731.icon-palette-variant-generator` 配下の UPM パッケージ
- 画像読み込みとパレット色抽出
- RGB / HSV / Lab 色距離による近傍色の自動グルーピングと、距離しきい値の調整
- パレット色の手動グループ移動と locked group 保護
- グループ単位、色単位、Hybrid の置換ルール
- Before / After プレビューと選択色 overlay
- スクロール可能なパレット一覧
- PNG 出力
- セッション JSON の保存 / 読み込み
- 置換ルールプリセット JSON の export / import と ScriptableObject プリセットアセット
- 複数バリエーション管理と一括出力
- フォルダ内の複数 Texture2D に対する一括バリエーション出力
- Help、言語設定、Auto Preview

## 起動方法

Unity Editor のメニューから起動します。

```text
Tools > Palette Variant Generator > 開く
```

## 検証

```powershell
powershell -ExecutionPolicy Bypass -File tools\validation\run-editmode-tests.ps1
```

期待される marker:

```text
ISSUE1_SCAFFOLD_VALIDATION=PASS
ISSUE2_IMAGE_PALETTE_VALIDATION=PASS
ISSUE3_COLOR_GROUPING_VALIDATION=PASS
ISSUE4_REPLACEMENT_PREVIEW_VALIDATION=PASS
ISSUE5_PNG_EXPORT_VALIDATION=PASS
ISSUE6_SESSION_JSON_VALIDATION=PASS
ISSUE7_VARIATION_BATCH_EXPORT_VALIDATION=PASS
ISSUE8_SAMPLE_QA_VALIDATION=PASS
ISSUE10_UI_POLISH_VALIDATION=PASS
ISSUE12_VARIATION_UX_VALIDATION=PASS
ISSUE13_AUTO_PREVIEW_DEBOUNCE_VALIDATION=PASS
ISSUE17_PREVIEW_NAVIGATION_VALIDATION=PASS
ISSUE18_RULE_PRESET_VALIDATION=PASS
ISSUE19_MANUAL_GROUP_EDITING_VALIDATION=PASS
ISSUE20_COLOR_DISTANCE_MODE_VALIDATION=PASS
ISSUE21_FOLDER_BATCH_EXPORT_VALIDATION=PASS
ISSUE22_SCRIPTABLE_OBJECT_PRESET_VALIDATION=PASS
ISSUE23_DOCKED_LAYOUT_VALIDATION=PASS
ISSUE23_UI_TOOLKIT_PREVIEW_VALIDATION=PASS
ISSUE23_UI_TOOLKIT_INTERACTION_VALIDATION=PASS
ISSUE23_MAIN_WINDOW_UI_TOOLKIT_HOST_VALIDATION=PASS
ISSUE25_PREVIEW_MENU_HIDDEN_VALIDATION=PASS
ISSUE25_UI_TOOLKIT_PRODUCTION_VALIDATION=PASS
ISSUE26_NOISE_REMOVAL_VALIDATION=PASS
ISSUE27_EDGE_OUTSIDE_CLEANUP_VALIDATION=PASS
ISSUE28_EXPORT_UI_DISCLOSURE_VALIDATION=PASS
ISSUE38_PALETTE_RULE_STATUS_VALIDATION=PASS
ISSUE39_EFFECT_HIGHLIGHT_VALIDATION=PASS
ISSUE41_COLLAPSIBLE_SETTINGS_VALIDATION=PASS
ISSUE42_PREVIEW_MINI_TOOLBAR_VALIDATION=PASS
ISSUE43_UNDO_REDO_VALIDATION=PASS
ISSUE44_DIFFERENCE_PREVIEW_VALIDATION=PASS
ISSUE45_EXPORT_PRECHECK_VALIDATION=PASS
ISSUE46_BOUNDARY_TRIM_VALIDATION=PASS
ISSUE24_RELEASE_AUTOMATION_VALIDATION=PASS
```

## ドキュメント

計画、仕様、利用方法、配布用文案は `docs/` 配下にあります。

- マニュアル: `docs/manual.md`
- 利用条件: `docs/terms.md`
- リリースノート: `docs/release-notes-v1.0.2.md`
- BOOTH 商品説明文案: `docs/booth-copy.md`

## リリースビルド

```powershell
powershell -ExecutionPolicy Bypass -File tools\release\build-release.ps1 -Version 1.0.2
powershell -ExecutionPolicy Bypass -File tools\release\test-release-package.ps1 -Version 1.0.2
```

出力先:

```text
ReleaseBuilds/PaletteVariantGenerator_v1.0.2.zip
```

GitHub Actions の `Release Package` workflow でも tracked files から同じ ZIP を生成します。

## サンプル

検証用サンプルアイコンは以下にあります。

```text
Packages/com.sunmax0731.icon-palette-variant-generator/Samples~/SampleIcons
```

## 既知の制限

- 初回リリースの UI は IMGUI ベースです。
- 色距離は RGB / HSV / Lab から選択できます。RGB は高速で安定、HSV は色相差を扱いやすく、Lab は見た目に近い近傍色判定に向いています。
- SpriteAtlas の直接編集は対象外です。
- フォルダ単位の一括処理は Texture2D アセットを対象にしています。SpriteAtlas の直接編集は対象外です。

## Agent 向けドキュメント

- `Agents.md`
- `Skill.md`
