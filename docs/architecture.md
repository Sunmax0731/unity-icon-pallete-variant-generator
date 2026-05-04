# Unity Icon Palette Variant Generator 設計書

## 設計方針

EditorWindow は View / Controller として扱い、画像処理、描画処理、合成、保存、Export は Service に分離する。MVP / MVVM を厳密に適用するより、Unity Editor 拡張として保守しやすい責務分割を優先する。

## 主要構成

```text
Packages/com.sunmax0731.icon-palette-variant-generator/
  Runtime/
    Models/
    Services/
    Utilities/
  Editor/
    Windows/
    Services/
    Validation/
    Tests/
```

## Model

- `PaletteVariantSession`: 現在のセッション状態。
- `PaletteColorEntry`: 抽出色、出現数、グループ、色別ルール。
- `ColorGroup`: 代表色とグループ単位の置換設定。
- `IconVariation`: バリエーション名、出力名、ルール状態。
- `PaletteLayer`: 描画レイヤー / 画像レイヤーの状態。
- `DrawToolSettings`: ツール種別、編集対象、サイズ、強さ、不透明度、各種半径。

保存対象は `[Serializable]` を基本にし、`JsonUtility` で扱える List 中心の構造にする。

## Service

| Service | 責務 |
|---|---|
| `TextureAssetLoader` | Texture2D / PNG / JPEG を読み込み、編集可能な RGBA バッファを作る。 |
| `ColorExtractionService` | ピクセルからパレットを抽出する。 |
| `ColorQuantizationService` | 近似色をまとめる。 |
| `ColorDistanceService` | RGB / HSV / Lab の距離を計算する。 |
| `ColorGroupingService` | パレット色をグループ化する。 |
| `ColorReplacementService` | 置換ルールを適用する。 |
| `RasterPaintService` | Brush / Eraser / Fill / Blur / Smooth / NoiseRemoval のピクセル処理を行う。 |
| `PaintStrokeSessionService` | Preview 入力をストロークに変換し、対象バッファへ commit する。 |
| `LayerCompositingService` | 読み込み画像、置換結果、レイヤーを合成する。 |
| `PngExportService` | PNG を出力する。 |
| `ExportedTextureImportSettingsService` | `Assets/` 配下の出力 PNG に `Alpha Is Transparency` を設定する。 |
| `SessionJsonService` | セッション JSON を保存 / 読み込みする。 |

## 描画フロー

```text
Preview mouse event
  -> Window が座標を画像ピクセルへ変換
  -> PaintStrokeSessionService が対象を解決
  -> RasterPaintService がピクセル配列を更新
  -> LayerCompositingService が Preview 用 Texture を再構築
  -> Window が Repaint
```

Fill はクリック点から flood fill を行い、1 ストロークにつき 1 回だけ適用する。Brush / Eraser は前回点から現在点まで補間して飛びを防ぐ。

## Export フロー

```text
Source edit buffer
  -> ColorReplacementService
  -> LayerCompositingService
  -> PngExportService
  -> AssetDatabase.Refresh
  -> ExportedTextureImportSettingsService
```

元画像アセットは読み取り専用として扱い、直接保存しない。

## UI 責務

`PaletteVariantGeneratorWindow` は次に限定する。

- UI の描画。
- 入力イベントの受け取り。
- Service の呼び出し。
- `Texture2D` の表示用ライフサイクル管理。
- ステータスメッセージ表示。

重い画像処理、flood fill、ノイズ除去、Export、JSON 保存は Window 内に実装しない。

## テスト方針

- Service 単位の EditMode テストを優先する。
- Window 経由の回帰は Validation marker で補完する。
- リリース前は `tools\validation\run-editmode-tests.ps1` を必ず実行する。
- ReleaseBuilds の ZIP と `.unitypackage` は `test-release-package.ps1` で検証する。

主な検証対象:

- 色抽出 / 量子化 / 距離計算 / グルーピング。
- 色置換と alpha 維持。
- Brush / Eraser / Fill / Blur / Smooth / NoiseRemoval。
- 読み込み画像とアクティブレイヤーの編集。
- JPEG 内部 RGBA 化。
- Export alpha と `Alpha Is Transparency`。
- Session Save / Load。
