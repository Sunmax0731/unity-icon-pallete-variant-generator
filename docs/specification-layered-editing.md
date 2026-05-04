# レイヤー対応描画拡張 仕様

## 1. UI 構成

### Toolbar

- `Source`
- `Analyze`
- `Auto Group`
- `Preview`
- `Export`
- `Session`
- `Help`
- `Language`

補助操作:

- `Add Paint Layer`
- `Add Image Layer`
- `Duplicate Layer`
- `Delete Layer`

### Settings Column

- Source Info
- Analyze Settings
- Group Settings
- Export Settings
- Tool Settings

### Preview Workspace

- Before / After / Composite の比較表示
- ツールバー
  - Tool
  - Size
  - Strength
  - Opacity
  - Color
- 描画対象は Composite Preview 上のアクティブレイヤー

### Inspector Column

- Layers
- Active Layer Details
- Replacement Rules
- Color Rules

### Report

- エラー
- 警告
- 直近の操作結果

## 2. データモデル

### 2.1 LayerBlendMode

- `Normal`

### 2.2 LayerKind

- `Paint`
- `Image`

### 2.3 DrawToolKind

- `Brush`
- `Eraser`
- `Blur`
- `Smooth`
- `NoiseRemoval`

### 2.4 LayerPixelData

- `width`
- `height`
- `rgbaBytesBase64`

### 2.5 RasterLayer

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

### 2.6 DrawingToolSettings

- `activeTool`
- `brushSize`
- `strength`
- `paintColor`
- `paintOpacity`
- `noiseRegionPixels`
- `noiseThreshold`
- `smoothIterations`
- `blurRadius`

## 3. 合成仕様

### 3.1 ベース画像

- 色変換後の Preview 画像をレイヤー合成の土台とする
- レイヤー合成は Export にも同じ順序で適用する

### 3.2 合成順

1. 色解析・グループ化・置換ルールでベース結果を作る
2. `visible = true` のレイヤーを下から順に合成する
3. 各レイヤーは `opacity` を乗算して `Normal` 合成する

## 4. ツール仕様

### 4.1 Brush

- 円形ブラシ
- ドラッグ中の座標列を補間しながら塗る
- `paintColor` と `paintOpacity` を使う

### 4.2 Eraser

- 円形ブラシ
- アクティブレイヤーの alpha を減算する
- RGB は維持、alpha のみ減衰

### 4.3 Blur

- ブラシ範囲内に簡易 box blur を適用する
- 半径は `blurRadius`

### 4.4 Smooth

- ブラシ範囲内に対して近傍平均との差分を弱く寄せる
- ぼかしより強度を抑える

### 4.5 Noise Removal

- ブラシ範囲を抽出して、小領域に対して既存 `NoiseRemovalService` を適用する
- 出力はアクティブレイヤーへ戻す

## 5. セッション互換

- `schemaVersion` を `1.1.0` に更新する
- 旧 JSON 読み込み時は以下を補完する
  - `layers = []`
  - `drawingToolSettings = default`
  - `activeLayerId = ""`

## 6. テスト対象

- Layer compositing
- Brush / Eraser pixel write
- Blur / Smooth mutation
- Layer session serialization
- Legacy session migration
