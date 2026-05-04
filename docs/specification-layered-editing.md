# レイヤー対応描画拡張 仕様

## 1. UI 構成

### Toolbar

- Source Image
- Analyze
- Auto Group
- Preview
- Export
- Export All
- Undo / Redo
- Save Session / Load Session
- Help
- Language
- Auto Preview

### Settings Column

- ソース情報
- 解析設定
- グループ設定
- ツール設定
- 書き出し設定
- プリセットアセット

### Preview Workspace

- Preview Mode
- Brush Size shortcut
- Compare mode
- Zoom
- Split position
- Selection Highlight
- Effect Highlight
- Resizable preview canvas
- Selected color information
- Palette list

### Inspector Column

- Layers
- Variations
- Group replacement rules
- Per-color replacement rules

## 2. データモデル

### DrawToolKind

- `Brush`
- `Eraser`
- `Blur`
- `Smooth`
- `NoiseRemoval`
- `Fill`

### PaintEditTarget

- `SourceImage`: セッション内の読み込み画像 RGBA バッファを編集する。
- `ActiveLayer`: アクティブな Paint / Image Layer を編集する。

### RasterLayer

- `id`
- `displayName`
- `kind`
- `visible`
- `locked`
- `opacity`
- `blendMode`
- `offsetX`
- `offsetY`
- `sourceAssetPath`
- `pixelData`

### DrawingToolSettings

- `paintTarget`
- `activeTool`
- `brushSize`
- `strength`
- `paintColor`
- `paintOpacity`
- `noiseRegionPixels`
- `noiseThreshold`
- `smoothIterations`
- `blurRadius`

### PaletteVariantSession 追加項目

- `sourcePixelData`: 読み込み画像への直接編集結果。
- `layers`: レイヤー一覧。
- `activeLayerId`: アクティブレイヤー。
- `drawingToolSettings`: 描画ツール設定。

## 3. 合成仕様

1. 元画像または `sourcePixelData` をベースにする。
2. 色置換ルールを適用してベース Preview を作る。
3. `visible = true` のレイヤーを下から順に Normal 合成する。
4. 各レイヤーは `opacity` を alpha に乗算する。
5. Preview と Export は同じ合成経路を使う。

## 4. 描画仕様

### Brush

- 円形ブラシで `paintColor` と `paintOpacity` を適用する。
- `strength` で既存色との混合量を調整する。
- ドラッグ中は前回座標と現在座標を補間して連続適用する。

### Eraser

- 円形ブラシ範囲の alpha を減算する。
- RGB は保持し、alpha のみ減衰させる。
- SourceImage / ActiveLayer の両方に適用できる。

### Fill

- 開始ピクセルの RGBA と完全一致する上下左右連結領域を flood fill する。
- Fill は 1 ストロークにつき 1 回だけ適用する。
- 塗り色は `paintColor` と `paintOpacity` を使う。

### Blur

- ブラシ範囲内に box blur を適用する。
- 半径は `blurRadius` を使う。

### Smooth

- ブラシ範囲内で近傍平均へ寄せる。
- 反復数は `smoothIterations` を使う。

### NoiseRemoval

- ブラシ範囲内の小さな孤立領域を周囲色で補正する。
- `noiseRegionPixels` と `noiseThreshold` を使う。

## 5. JPEG 透過編集

- JPEG は読み込み時に RGBA32 の編集バッファへ正規化する。
- 消しゴムで alpha 0 を作れる。
- 元 JPEG ファイルは上書きしない。
- Session JSON と Export PNG では alpha を保持する。

## 6. Export 仕様

- PNG は元画像と同じ幅・高さで出力する。
- 色置換、source direct edit、layer composite を反映する。
- 出力先が `Assets/` 配下の場合、`ExportedTextureImportSettingsService` が TextureImporter を更新する。
- `alphaSource = FromInput`、`alphaIsTransparency = true` にする。

## 7. Session 互換

- 旧 JSON に `sourcePixelData` がない場合、読み込み画像から作成する。
- 旧 JSON に `layers` がない場合、空配列を補う。
- 旧 JSON に `drawingToolSettings` がない場合、既定値を補う。

## 8. テスト対象

- Layer compositing
- Brush / Eraser / Fill pixel write
- Blur / Smooth / NoiseRemoval mutation
- SourceImage direct edit
- JPEG internal RGBA buffer
- Export alpha import settings
- Layer session serialization
- Tool popup synchronization
