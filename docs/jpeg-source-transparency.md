# JPEG 読み込み画像の透過編集仕様

## 方針

JPEG はファイル形式として alpha を保持できません。そのため、JPEG を読み込んだ場合も元ファイルは変更せず、ツール内部で RGBA32 の編集用バッファを作成します。

## 処理

1. `Source Image` に JPEG を指定する。
2. `Analyze` 時に `TextureAssetLoader` が RGBA32 バッファを作成する。
3. `編集対象 = 読み込み画像` の描画ツールは、この内部バッファを編集する。
4. `消しゴム` は内部バッファの alpha を減算する。
5. `Export` は PNG として出力し、透明化結果を保持する。

## ユーザーに見える挙動

- JPEG でも Preview 上では透明化した箇所が見える。
- 元の JPEG アセットは上書きされない。
- Session Save / Load で透明化結果を復元できる。
- Export PNG には alpha が保持される。
- `Assets/` 配下に出力した PNG は `Alpha Is Transparency` が ON になる。

## 手動確認

1. JPEG を `Source Image` に指定する。
2. `Analyze` と `Auto Group` を実行する。
3. `ツール設定 > 編集対象` を `読み込み画像` にする。
4. `ツール設定 > ツール` を `消しゴム` にする。
5. `プレビューモード` を `描画` にする。
6. Preview 上をドラッグし、該当箇所が透明になることを確認する。
7. `Export` し、出力 PNG の alpha が保持されることを確認する。

## 自動検証 marker

```text
ISSUE48_RGB_SOURCE_ERASER_VALIDATION=PASS
ISSUE48_JPG_SOURCE_ERASER_VALIDATION=PASS
ISSUE48_JPG_INTERNAL_PNG_CONVERSION_VALIDATION=PASS
ISSUE48_JPG_WINDOW_ERASER_VALIDATION=PASS
ISSUE48_EXPORT_ALPHA_TRANSPARENCY_VALIDATION=PASS
```
