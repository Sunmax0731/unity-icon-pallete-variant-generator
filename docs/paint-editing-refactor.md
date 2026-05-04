# 描画編集リファクタリング方針

## 背景

描画機能の追加により、EditorWindow に次の責務が集中しやすくなりました。

- Preview の UI と入力イベント。
- 編集対象の判定。
- Brush / Eraser / Fill などのピクセル処理。
- 読み込み画像バッファとレイヤーバッファへの commit。
- Preview 用 Texture の再構築。

この状態では、読み込み画像とアクティブレイヤーの分岐、Brush と Eraser の分岐、比較表示と Paint 表示の分岐が複雑になり、回帰が起こりやすくなります。

## 採用した分割

### PaletteVariantGeneratorWindow

- UI 表示。
- Preview のマウスイベント受け取り。
- 画像座標への変換。
- Service 呼び出し。
- ステータスメッセージ表示。

### PaintStrokeSessionService

- 編集対象の解決。
- ストローク開始 / 更新 / 終了。
- 高速ドラッグ時の補間点生成。
- Fill の one-shot 制御。
- `sourcePixelData` または `layer.pixelData` への反映。

### RasterPaintService

- Brush。
- Eraser。
- Fill。
- Blur。
- Smooth。
- NoiseRemoval。

ピクセル配列を受け取り、ツールごとの処理だけを行います。UI や Texture2D の生成には関与しません。

### LayerCompositingService

- 読み込み画像編集バッファ、色置換結果、レイヤーを合成する。
- Preview と Export の見た目を一致させる。

## 設計上のルール

- 元画像アセットを直接変更しない。
- JPEG は編集前に内部 RGBA バッファへ正規化する。
- Eraser は Brush 色で塗らず、対象ピクセルの alpha を変更する。
- Fill はクリックした RGBA の上下左右連結領域だけを処理する。
- Paint モード中は比較表示よりリアルタイム編集表示を優先する。
- Preview 用 Texture の再構築は必要な範囲に抑える。

## 検証対象

- 読み込み画像に Brush が効く。
- 読み込み画像に Eraser が効き、alpha が下がる。
- JPEG 読み込み画像でも Eraser の結果が PNG Export に反映される。
- 描画レイヤーに Brush / Eraser が効く。
- Fill が読み込み画像と描画レイヤーの両方に効く。
- Preview Mode や比較モードを切り替えても Paint 入力が失われない。

## 関連 marker

```text
ISSUE48_DIRECT_SOURCE_ERASER_VALIDATION=PASS
ISSUE48_RGB_SOURCE_ERASER_VALIDATION=PASS
ISSUE48_JPG_SOURCE_ERASER_VALIDATION=PASS
ISSUE48_JPG_INTERNAL_PNG_CONVERSION_VALIDATION=PASS
ISSUE48_JPG_WINDOW_ERASER_VALIDATION=PASS
ISSUE48_FILL_TOOL_VALIDATION=PASS
ISSUE48_TOOL_POPUP_SYNC_VALIDATION=PASS
ISSUE48_EXPORT_ALPHA_TRANSPARENCY_VALIDATION=PASS
```
