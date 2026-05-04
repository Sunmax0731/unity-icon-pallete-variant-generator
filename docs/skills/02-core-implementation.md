# 02 - Core Implementation Skill

## 目的

画像解析、色置換、レイヤー合成、描画ツール、Export、Session 保存などの中核処理を、保守しやすい Service 単位で実装するための工程ガイドです。

## 配置ルール

- Runtime Models: `Packages/com.sunmax0731.icon-palette-variant-generator/Runtime/Models`
- Runtime Services: `Packages/com.sunmax0731.icon-palette-variant-generator/Runtime/Services`
- Runtime Utilities: `Packages/com.sunmax0731.icon-palette-variant-generator/Runtime/Utilities`
- Editor Services: `Packages/com.sunmax0731.icon-palette-variant-generator/Editor/Services`
- Editor Window: `Packages/com.sunmax0731.icon-palette-variant-generator/Editor/Windows`
- Validation: `Packages/com.sunmax0731.icon-palette-variant-generator/Editor/Validation`

## 実装原則

- EditorWindow に画像処理を直接実装しない。
- Service は EditMode test しやすい API にする。
- Model は `[Serializable]` と List 中心の構造にする。
- Dictionary や `UnityEngine.Object` 参照を JSON 保存形式に含めない。
- 元画像ファイルを直接上書きしない。
- 一時 `Texture2D` は破棄責務を明確にする。

## 中核仕様

### 画像解析

- `Alpha Threshold` 以下のピクセルは解析対象外にする。
- `Quantize Step` で近似色をまとめる。
- 出現数と出現率を保持する。

### 色置換

```text
output = Lerp(original, target, blendRatio)
```

- `GroupUniform`: グループ単位で置換する。
- `PerColor`: 有効な色別ルールのみ置換する。
- `Hybrid`: 色別ルールを優先し、未設定色はグループルールにフォールバックする。

### 描画

- `RasterPaintService` が Brush / Eraser / Fill / Blur / Smooth / NoiseRemoval を担当する。
- `PaintStrokeSessionService` が編集対象解決、ストローク補間、commit を担当する。
- Eraser は色で塗らず alpha を下げる。
- Fill は RGBA が一致する上下左右連結領域だけを対象にする。

### Export

- `PngExportService` は PNG ファイルの出力だけを担当する。
- `ExportedTextureImportSettingsService` は `Assets/` 配下の `Alpha Is Transparency` 設定だけを担当する。

## 検証ルール

- Service 追加時は focused EditMode test を追加する。
- 重要な回帰は `PaletteVariantGeneratorValidation` に marker を追加する。
- `tools/validation/run-editmode-tests.ps1` の marker 期待値も更新する。
