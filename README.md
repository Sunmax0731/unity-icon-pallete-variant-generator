# Unity Icon Palette Variant Generator

Unity Editor 上でアイコン画像のパレットを解析し、近傍色グループ、色置換、レイヤー合成、描画ツールを使って色違い PNG を生成する Editor 拡張です。

## 対応環境

- Unity: `6000.4.0f1`
- Package: `com.sunmax0731.icon-palette-variant-generator`
- Version: `1.1.0`
- 配布形式: UPM package ZIP / `.unitypackage`
- License: MIT License

## 起動方法

```text
Tools > Palette Variant Generator > メイン画面
Tools > Palette Variant Generator > ライセンス
Tools > Palette Variant Generator > バージョン情報
```

## 主な機能

- PNG / JPG / Texture2D アセットからのパレット抽出
- RGB / HSV / Lab 距離による近傍色グルーピング
- `GroupUniform` / `PerColor` / `Hybrid` 置換ルール
- Before / After / Split / SideBySide / Difference Preview
- Preview 上の色ピック、複数色ブラシ選択、選択色ハイライト
- 読み込み画像への直接編集と、描画レイヤー / 画像レイヤーの合成
- ブラシ、消しゴム、塗りつぶし、ぼかし、スムース、ノイズ除去
- JPEG 読み込み画像の内部 RGBA 化と、消しゴムによる透過編集
- Preview キャンバスのリサイズ、Zoom / Drag Pan
- Export / Export All / Folder Batch Export
- `Assets/` 配下に出力した PNG の `Alpha Is Transparency` 自動 ON
- Session JSON 保存 / 読み込み、Rule Preset JSON / ScriptableObject Preset
- 日本語 / 英語 UI、Help、Auto Preview、Undo / Redo

## 基本操作

1. `Source Image` に画像を指定します。
2. `Analyze` でパレットを抽出します。
3. `Auto Group` で近傍色をグループ化します。
4. 右側の置換ルールで色とブレンド率を調整します。
5. `Preview` で結果を確認します。
6. 必要に応じて `ツール設定 > 編集対象` を選び、Preview 上でブラシや塗りつぶしを使います。
7. `Export` または `Export All` で PNG を出力します。
8. 再利用する設定は `Save Session` または Preset として保存します。

## 描画とレイヤー

- `編集対象 = 読み込み画像`: 元アセットを上書きせず、セッション内の RGBA バッファへ直接描画します。JPEG でも内部的に PNG 相当の透過バッファとして扱うため、消しゴムで alpha を 0 にできます。
- `編集対象 = アクティブレイヤー`: Paint Layer / Image Layer に対して非破壊で描画します。
- `塗りつぶし`: クリックしたピクセルと同じ RGBA の上下左右連結領域だけを一括で塗りつぶします。透明ピクセルの連結領域も対象です。
- `消しゴム`: 対象の alpha を減算します。書き出し時は PNG の透明部分として保持されます。
- `ぼかし` / `スムース` / `ノイズ除去`: 局所的な仕上げや小さなノイズ除去に使います。

## サンプル

Package Manager から以下のサンプルを import できます。

```text
Packages/com.sunmax0731.icon-palette-variant-generator/Samples~/SampleIcons
```

## 検証

```powershell
powershell -ExecutionPolicy Bypass -File tools\validation\run-editmode-tests.ps1
```

主要 marker:

```text
ISSUE1_SCAFFOLD_VALIDATION=PASS
ISSUE2_IMAGE_PALETTE_VALIDATION=PASS
ISSUE3_COLOR_GROUPING_VALIDATION=PASS
ISSUE4_REPLACEMENT_PREVIEW_VALIDATION=PASS
ISSUE5_PNG_EXPORT_VALIDATION=PASS
ISSUE6_SESSION_JSON_VALIDATION=PASS
ISSUE7_VARIATION_BATCH_EXPORT_VALIDATION=PASS
ISSUE8_SAMPLE_QA_VALIDATION=PASS
ISSUE24_RELEASE_AUTOMATION_VALIDATION=PASS
ISSUE48_DIRECT_SOURCE_ERASER_VALIDATION=PASS
ISSUE48_RGB_SOURCE_ERASER_VALIDATION=PASS
ISSUE48_JPG_SOURCE_ERASER_VALIDATION=PASS
ISSUE48_JPG_INTERNAL_PNG_CONVERSION_VALIDATION=PASS
ISSUE48_JPG_WINDOW_ERASER_VALIDATION=PASS
ISSUE48_EXPORT_ALPHA_TRANSPARENCY_VALIDATION=PASS
ISSUE48_FILL_TOOL_VALIDATION=PASS
ISSUE48_TOOL_POPUP_SYNC_VALIDATION=PASS
```

## ドキュメント

- Manual: `docs/manual.md`
- 手動テスト: `docs/manual-test-layered-editing.md`
- 塗りつぶしテスト: `docs/manual-test-fill-tool.md`
- Export alpha テスト: `docs/manual-test-export-alpha-transparency.md`
- リリースノート: `docs/release-notes-v1.1.0.md`
- BOOTH 商品ページ Markdown: `docs/booth-copy.md`

## リリースビルド

```powershell
powershell -ExecutionPolicy Bypass -File tools\release\build-release.ps1 -Version 1.1.0
powershell -ExecutionPolicy Bypass -File tools\release\test-release-package.ps1 -Version 1.1.0
```

出力:

```text
ReleaseBuilds/PaletteVariantGenerator_v1.1.0.zip
ReleaseBuilds/PaletteVariantGenerator_v1.1.0.unitypackage
```

GitHub Release には ZIP と `.unitypackage` の両方を添付します。

## 既知の制限

- SpriteAtlas の直接編集は対象外です。Texture2D / PNG / JPG アセットを対象にしてください。
- 元画像ファイルは直接上書きしません。読み込み画像への直接編集はセッション内バッファと書き出し PNG に反映されます。
- `Alpha Is Transparency` の自動設定は Unity Project の `Assets/` 配下に出力した PNG に限ります。
