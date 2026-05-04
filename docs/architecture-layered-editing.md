# レイヤー対応描画拡張 設計

## 1. 方針

既存 `PaletteVariantGeneratorWindow` は機能集中が進んでいるため、今回の拡張では UI 側に直接画像処理を書き足さず、描画とレイヤー合成をサービスへ分離する。Window は操作の仲介に留める。

## 2. 追加コンポーネント

### Runtime/Models

- `DrawToolKind`
- `LayerBlendMode`
- `LayerKind`
- `DrawingToolSettings`
- `LayerPixelData`
- `RasterLayer`

### Runtime/Services

- `LayerTextureSerializationService`
- `LayerCompositingService`
- `RasterPaintService`
- `LayerSessionMigrationService`

## 3. 役割分担

### LayerTextureSerializationService

- `Color32[]` と Base64 文字列の相互変換
- Session JSON 保存時の変換責務

### LayerCompositingService

- ベース画像 + レイヤー一覧を合成して `Texture2D` を返す
- Preview / Export の共通経路にする

### RasterPaintService

- レイヤー上の局所編集を担当
- `Brush`
- `Eraser`
- `Blur`
- `Smooth`
- `NoiseRemoval`

### LayerSessionMigrationService

- 旧 `schemaVersion` セッションへレイヤー既定値を補う

## 4. Window への組み込み

### 4.1 Preview

- 既存 `RefreshAfterPreview()` で色変換後テクスチャを作る
- その結果を `LayerCompositingService.Compose()` に渡して最終 Preview を得る

### 4.2 Export

- `ExportPreview()` と `ExportAllVariations()` は、色変換後のテクスチャへ同じレイヤー合成を適用してから PNG 出力する

### 4.3 UI

- Preview interaction mode を描画ツール選択へ拡張する
- Layers セクションを追加し、アクティブレイヤーを明示する
- Tool Settings は左カラムに寄せ、描画の実行対象と現在色はプレビュー直上にも表示する

## 5. データフロー

1. Source image load
2. Palette extract
3. Auto group / manual rule edit
4. Base replacement preview build
5. Layer composite preview build
6. User edits active layer with raster tool
7. Composite preview rebuild
8. Session save or PNG export

## 6. Undo/Redo

- 既存の session snapshot 方式を継続利用する
- 描画開始前に session snapshot を積む
- ストローク中は連続 snapshot を積まない

## 7. リスクと対策

- `PaletteVariantGeneratorWindow.cs` がさらに肥大化する
  - 対策: 新規処理はサービス化し、Window 側は呼び出しのみに留める
- レイヤーの JSON が肥大化する
  - 対策: Base64 圧縮なしでまず正しさ優先、将来圧縮余地を残す
- 大画像での描画再生成コスト
  - 対策: 編集対象レイヤーのみ変更し、Composite 生成を単純な 1 パスに留める
