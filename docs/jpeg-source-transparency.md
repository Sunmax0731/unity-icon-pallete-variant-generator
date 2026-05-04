# JPEG読み込み画像の透過編集仕様

## 方針

- JPEGはファイル形式として透明度を保持できないため、元JPEGアセットは直接変更しない。
- Analyze時に、ツール内部ではJPEGをRGBA32の透過編集用バッファへ変換する。
- 消しゴムツールは、この内部バッファのAlphaを0にする。
- Export時はPNGとして書き出すため、消しゴムで作った透明部分を保持できる。

## 手動確認

1. `Source Image` にJPEG画像を指定して `Analyze` を実行する。
2. ステータスに「JPEGを内部の透過編集用PNGバッファに変換しました」と表示されることを確認する。
3. `Tool Settings > 編集対象` を `読み込み画像` にする。
4. `Tool Settings > ツール` を `消しゴム` にする。
5. `Preview Mode` を `描画` にする。
6. Preview上でJPEG由来の画像をドラッグし、該当箇所がチェッカーボード表示になることを確認する。
7. `Preview` を実行してから `Export` し、出力PNGで透明部分が保持されていることを確認する。

## 自動検証

- `ISSUE48_RGB_SOURCE_ERASER_VALIDATION=PASS`
- `ISSUE48_JPG_SOURCE_ERASER_VALIDATION=PASS`
- `ISSUE48_JPG_INTERNAL_PNG_CONVERSION_VALIDATION=PASS`
- `ISSUE48_JPG_WINDOW_ERASER_VALIDATION=PASS`
- `ISSUE48_EXPORT_ALPHA_TRANSPARENCY_VALIDATION=PASS`
