# 描画編集リファクタリング方針

## 1. 背景

`PaletteVariantGeneratorWindow` には、以下の責務が混在していた。

- Tool Settings / Preview / Layers の UI 構築
- 入力イベント処理
- 描画対象の選択
- ストローク中バッファの保持
- `sourcePixelData` / `layer.pixelData` への commit
- Preview 再構築

この構成では、`読み込み画像` と `アクティブレイヤー` の編集経路が Window 内の分岐で増殖し、Brush / Eraser の組み合わせで回帰が起こりやすかった。

## 2. 新しい責務分割

### Window

- UI の構築
- Preview pointer event の受付
- 画面表示用 `Texture2D` の寿命管理
- report message / button state の更新

### PaintStrokeSessionService

- 描画対象に応じたストローク用ピクセルバッファの開始
- Brush / Eraser / Blur / Smooth / NoiseRemoval の適用
- 補間付きストロークの継続
- `sourcePixelData` または `layer.pixelData` への commit

### RasterPaintService

- 1 回の tool 適用ロジック
- ストローク補間
- ノイズ除去 / 平滑化 / ぼかし

### LayerCompositingService

- ベース Preview とレイヤーの合成

## 3. 採用パターン

全面的な MVVM 化ではなく、既存 Window を view/controller として残しつつ、描画状態だけを専用 service へ抽出する形を採用した。

理由:

- 既存 UI Toolkit Window を全面書き換えせずに責務を減らせる
- 不具合が集中している描画編集経路を優先して単純化できる
- EditMode テストを service 単位で追加しやすい

## 4. 追加したテスト観点

- `PaintStrokeSessionServiceTests`
  - `読み込み画像` に対して Brush 後 Eraser で alpha が 0 になる
  - `アクティブレイヤー` に対して Brush 後 Eraser で alpha が 0 になる
- `PaletteVariantGeneratorWindowTests`
  - Preview 上の source direct edit で、描画後に消しゴムを当てると source pixel と preview 表示の両方が変化し、`RefreshAfterPreview()` 後も戻らない
- `PaletteVariantGeneratorValidation`
  - batch 実行で direct source brush -> eraser の回帰を検出する

## 5. 今後の分離候補

- Preview 表示切り替えを `PreviewPresentationService` に抽出
- report message / localized label 生成を `WindowTextService` に抽出
- Undo/Redo snapshot 制御を `SessionHistoryService` に抽出
