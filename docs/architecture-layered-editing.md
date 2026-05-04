# レイヤー対応描画拡張 設計

## 1. 方針

`PaletteVariantGeneratorWindow` は UI と操作仲介に寄せ、画像処理、レイヤー合成、描画、出力後 import 設定は Service へ分離する。全面的な MVVM 化ではなく、既存 IMGUI / UI Toolkit hybrid を維持しながら、回帰しやすい描画経路をサービス化する。

## 2. 主要コンポーネント

### PaletteVariantGeneratorWindow

- UI 構築
- Preview pointer event の受付
- Session 状態の保持
- Service 呼び出し
- 表示用 Texture2D の寿命管理
- report message / button state の更新

### LayerCompositingService

- ベース画像とレイヤー一覧を合成する。
- Preview / Export の共通経路にする。
- 既存 Texture2D へ更新する API を持ち、描画中の不要な再生成を抑える。

### RasterPaintService

- 1 回のツール適用ロジックを担当する。
- Brush / Eraser / Fill / Blur / Smooth / NoiseRemoval を扱う。
- Brush / Eraser 用に座標間補間を提供する。

### PaintStrokeSessionService

- SourceImage / ActiveLayer のどちらを編集するかを解決する。
- ドラッグ中の mutable pixel buffer を保持する。
- Fill は 1 ストローク 1 回に制限する。
- commit 時に `sourcePixelData` または `activeLayer.pixelData` へ戻す。

### TextureAssetLoader

- Texture2D / PNG / JPG を編集可能な RGBA32 バッファとして読み込む。
- JPEG の内部 PNG 相当バッファ化を担当する。

### ExportedTextureImportSettingsService

- Export 後の PNG が `Assets/` 配下か判定する。
- TextureImporter を取得し、`alphaSource = FromInput`、`alphaIsTransparency = true` を設定する。

## 3. データフロー

1. Source image を指定する。
2. `TextureAssetLoader` が編集可能な読み込み画像バッファを用意する。
3. Analyze / Auto Group / Replacement でベース Preview を作る。
4. `LayerCompositingService` が source direct edit と layer composite を反映した最終 Preview を作る。
5. Preview 上の pointer event を `PaintStrokeSessionService` に渡す。
6. `RasterPaintService` が対象 pixel buffer を変更する。
7. 描画中は表示用 Texture2D を更新し、commit 時に Session へ戻す。
8. Export 時は同じ合成経路で PNG を作る。
9. `ExportedTextureImportSettingsService` が Unity import 設定を更新する。

## 4. Undo / Redo

- 既存の session snapshot 方式を継続する。
- 描画開始前に snapshot を積む。
- ストローク中は連続 snapshot を積まない。
- commit 後に Preview を更新する。

## 5. 性能対策

- 描画中は全体再解析を行わない。
- 表示用 Texture2D は可能な限り再利用する。
- Brush / Eraser は座標補間のみを行い、重い Preview 再生成を遅延させる。
- Fill は 1 click 1 flood fill とし、ドラッグ中に繰り返し実行しない。

## 6. リスクと対策

| リスク | 対策 |
|---|---|
| Window が肥大化する | 描画、合成、読み込み、import 設定を Service へ切り出す |
| SourceImage と ActiveLayer の分岐が増える | `PaintStrokeSessionService` で編集対象解決を一元化する |
| JPEG の alpha が失われる | 内部 RGBA バッファと PNG 出力を正とする |
| Export と Preview がずれる | `LayerCompositingService` を共通経路にする |
| 出力 PNG が Unity で透過扱いにならない | `ExportedTextureImportSettingsService` で import 設定を更新する |

## 7. 検証

- `RasterPaintServiceTests`: Brush / Eraser / Fill / Blur / NoiseRemoval
- `PaintStrokeSessionServiceTests`: SourceImage / ActiveLayer commit、stroke interpolation、Fill one-shot
- `PaletteVariantGeneratorWindowTests`: Preview 経由の直接編集、Fill、tool popup 同期
- `TextureAssetLoaderTests`: JPEG RGBA buffer
- `ExportedTextureImportSettingsServiceTests`: `Alpha Is Transparency`
- `PaletteVariantGeneratorValidation`: Issue #48 headless validation markers
