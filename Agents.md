# AGENTS.md - Unity Icon Palette Variant Generator

## 1. 目的

このファイルは、AI Agent / Codex が `Unity Icon Palette Variant Generator` の実装、検証、リリース準備を行うための指示書である。

本ツールは Unity Editor 上で画像の色を解析し、近傍色グルーピング、色置換、レイヤー合成、描画ツールを使って、単一アイコンから複数の色違い PNG を生成する Editor 拡張である。

## 2. 実装対象

- Unity Editor 拡張のみを対象にする。
- ランタイムゲーム機能は実装しない。
- 元画像ファイルを直接上書きしない。
- 読み込み画像への直接編集は、セッション内の RGBA バッファと書き出し PNG に反映する。

推奨メニュー:

```text
Tools > Palette Variant Generator > メイン画面
Tools > Palette Variant Generator > ライセンス
Tools > Palette Variant Generator > バージョン情報
```

## 3. 参照すべきドキュメント

実装前に以下を確認する。

1. `docs/requirements.md`
2. `docs/specification.md`
3. `docs/architecture.md`
4. `docs/development_plan.md`
5. `docs/color_variant_rule.schema.json`
6. `docs/requirements-layered-editing.md`
7. `docs/specification-layered-editing.md`
8. `docs/architecture-layered-editing.md`
9. `docs/paint-editing-refactor.md`
10. `docs/jpeg-source-transparency.md`
11. `Skill.md`
12. 作業工程に対応する `docs/skills/*.md`

UI 改修時は `D:\Claude\UnityEditor-Dev\workspace-guides\UnityEditorDesign.md` を参照し、Toolbar / Settings / Preview Workspace / Inspector / Report の責務分離を優先する。

## 4. 実装原則

- GitHub Issue を確認し、Issue 起点で作業する。
- Issue、コメント、完了報告は日本語を基本とする。
- 実装 Issue ごとに作業範囲を区切る。
- Unity `6000.4.0f1` で検証してから commit / close する。
- ユーザーが検証用に追加した `Assets/` 配下の画像や session/preset は、明示されない限りコミットしない。
- EditorWindow に全ロジックを詰め込まない。
- 画像解析、色抽出、グルーピング、置換、レイヤー合成、描画、出力、JSON 保存は Service として分離する。
- Model は `[Serializable]` を基本とし、Unity の `JsonUtility` で保存しやすい構造にする。
- Dictionary をそのまま保存形式に使わない。
- 一時生成した `Texture2D` の破棄漏れに注意する。
- Editor 専用コードは `Editor` フォルダ配下に配置する。

## 5. 現在の主要構成

- `ColorExtractionService`: Texture2D からパレット色を抽出する。
- `ColorGroupingService`: 近傍色をグルーピングする。
- `ColorReplacementService`: 置換ルールを適用して Preview / Export のベース画像を作る。
- `LayerCompositingService`: ベース画像と Paint / Image Layer を合成する。
- `RasterPaintService`: Brush / Eraser / Fill / Blur / Smooth / NoiseRemoval をピクセル配列へ適用する。
- `PaintStrokeSessionService`: ドラッグ中の編集バッファ、ストローク補間、commit を管理する。
- `TextureAssetLoader`: JPEG を含む読み込み画像を RGBA 編集バッファとして扱う。
- `ExportedTextureImportSettingsService`: `Assets/` 配下の出力 PNG に `Alpha Is Transparency` を設定する。
- `SessionJsonService`: Session JSON の保存 / 読み込みを担当する。

## 6. 重要仕様

### 6.1 透明ピクセル

透明ピクセルは初期設定では解析・変換対象外にする。`Alpha Threshold` 以下のピクセルは無視する。

### 6.2 色置換

基本式は以下。

```text
output = Lerp(original, target, ratio)
```

ratio は 0.0 から 1.0。

### 6.3 優先順位

1. `GroupUniform`: グループルールを適用する。
2. `PerColor`: 有効な個別カラーコードルールのみ適用する。
3. `Hybrid`: 有効な個別カラーコードルールを優先し、未設定色はグループルールへフォールバックする。
4. 該当ルールがない場合は変換しない。

### 6.4 描画対象

- `読み込み画像`: セッション内の `sourcePixelData` を編集する。JPEG も内部 RGBA バッファとして扱い、消しゴムで alpha を 0 にできる。
- `アクティブレイヤー`: Paint Layer / Image Layer の pixelData を編集する。ロック中は編集しない。

### 6.5 塗りつぶし

塗りつぶしは、クリックしたピクセルと同じ RGBA の上下左右連結領域だけを対象にする。斜め接続は対象外。透明ピクセル同士の連結領域も対象にする。

### 6.6 PNG 出力

- 元画像と同じ幅・高さで出力する。
- アルファを維持する。
- レイヤー合成と読み込み画像への直接編集を反映する。
- 既存ファイルがある場合は Conflict Mode に従う。
- 出力後に `AssetDatabase.Refresh` を行う。
- 出力先が `Assets/` 配下の場合、TextureImporter の `Alpha Is Transparency` を ON にする。

## 7. UI 方針

- 上部: Source / Analyze / Auto Group / Preview / Export / Session / Help / Language
- 左: ソース情報 / 解析設定 / グループ設定 / ツール設定 / 書き出し設定 / プリセット
- 中央: Preview / 選択色情報 / Palette
- 右: Layers / Variations / 置換ルール / 色別ルール
- 日本語モードでは主要 UI 文言を日本語にする。
- Tool Settings は折りたたみ可能にする。
- Preview はリサイズ可能にし、Zoom / Drag Pan / Split / SideBySide を維持する。

## 8. テスト対象

最低限、以下を検証する。

- `ColorQuantizationServiceTests`
- `ColorDistanceServiceTests`
- `ColorExtractionServiceTests`
- `ColorReplacementServiceTests`
- `LayerCompositingServiceTests`
- `RasterPaintServiceTests`
- `PaintStrokeSessionServiceTests`
- `TextureAssetLoaderTests`
- `ExportedTextureImportSettingsServiceTests`
- `SessionJsonServiceTests`

## 9. 標準検証

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

## 10. Release Artifact Policy

- GitHub Release には ZIP と `.unitypackage` 単体の両方を必ず添付する。
- `tools\release\build-release.ps1 -Version <version>` で `ReleaseBuilds/PaletteVariantGenerator_v<version>.zip` と `ReleaseBuilds/PaletteVariantGenerator_v<version>.unitypackage` を生成する。
- `tools\release\test-release-package.ps1 -Version <version>` で ZIP と `.unitypackage` の両方を検証する。
- 既存 Release に成果物を追加した場合も、`gh release view <tag> --json assets` で ZIP と `.unitypackage` の両方が表示されることを確認する。

## 11. リリース準備時の更新対象

- `Packages/com.sunmax0731.icon-palette-variant-generator/package.json`
- `README.md`
- `Packages/com.sunmax0731.icon-palette-variant-generator/README.md`
- `CHANGELOG.md`
- `docs/manual.md`
- `docs/validation-checklist.md`
- `docs/release-checklist.md`
- `docs/release-notes-vX.Y.Z.md`
- `docs/booth-copy.md`
- `Agents.md`
- `Skill.md`

メニュー、ライセンス、UI 文言、配布形式が変わる場合は、BOOTH / GitHub Release 向け説明文も同じ変更で更新する。
