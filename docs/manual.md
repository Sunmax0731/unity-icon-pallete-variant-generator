# 利用マニュアル

## 概要

`Unity Icon Palette Variant Generator` は、元画像を直接上書きせずに、アイコン画像の色違い PNG を作成する Unity Editor 拡張です。色解析、近傍色グルーピング、置換ルール、レイヤー合成、描画ツール、Session 保存を 1 つの EditorWindow で扱えます。

## 導入方法

### Git URL

Unity Package Manager から、このリポジトリの Git URL を指定してパッケージを追加します。

### Release ZIP

GitHub Releases から `PaletteVariantGenerator_v1.1.0.zip` をダウンロードして展開し、`Packages/com.sunmax0731.icon-palette-variant-generator` を Unity プロジェクトへ追加します。

### UnityPackage

`PaletteVariantGenerator_v1.1.0.unitypackage` を Unity Editor へ import します。

## 起動方法

```text
Tools > Palette Variant Generator > メイン画面
```

補助画面:

```text
Tools > Palette Variant Generator > ライセンス
Tools > Palette Variant Generator > バージョン情報
```

## 基本操作

1. `Source Image` に PNG / JPG / Texture2D を指定します。
2. `Analyze` でパレット色を抽出します。
3. 必要に応じて `Category Preset`、透明度しきい値、最小ピクセル数、量子化ステップを調整します。
4. `Auto Group` で近い色をグループ化します。
5. 右側の `置換ルール` または `色別ルール` で置換色とブレンド率を調整します。
6. `Preview` で結果を確認します。
7. 必要に応じて `ツール設定` から描画対象とツールを選び、Preview 上で編集します。
8. `Export` または `Export All` で PNG を出力します。
9. 作業状態を再利用する場合は `Save Session` を実行します。

## UI 構成

- 上部 Toolbar: Source / Analyze / Auto Group / Preview / Export / Session / Help / Language
- 左カラム: ソース情報、解析設定、グループ設定、ツール設定、書き出し設定、プリセット
- 中央: Preview、選択色情報、Palette
- 右カラム: Layers、Variations、置換ルール、色別ルール

日本語モードでは主要な操作ラベルは日本語で表示されます。Tool Settings は折りたたみ可能です。

## Preview 操作

- `比較`: 左右比較、分割比較、差分などの表示を切り替えます。
- `ズーム`: Preview を拡大します。
- `Drag Pan`: ズーム時に Preview 上をドラッグして表示位置を移動できます。
- `表示リセット`: ズーム、パン、分割位置を初期状態に戻します。
- Preview 下のリサイズハンドルをドラッグすると表示キャンバスの高さを変更できます。
- Preview 上のクリックで元画像座標のパレット色を選択できます。
- `選択色をハイライト`: 選択中の色またはグループを Preview 上で強調します。

## レイヤー

- `描画レイヤー追加`: 透明な Paint Layer を追加します。
- `画像レイヤー追加`: 画像ファイルを Image Layer として追加します。
- `レイヤー複製`: 選択中レイヤーを複製します。
- `上へ移動` / `下へ移動`: 合成順を変更します。
- `レイヤー削除`: 選択中レイヤーを削除します。
- Visible を OFF にすると Preview / Export から除外されます。
- Lock を ON にすると描画編集できなくなります。
- Opacity でレイヤーの不透明度を調整します。

## 描画対象

`ツール設定 > 編集対象` で次のいずれかを選びます。

- `読み込み画像`: 元画像ファイルを上書きせず、セッション内の RGBA バッファを直接編集します。
- `アクティブレイヤー`: 選択中の Paint Layer / Image Layer を編集します。

JPEG はファイル形式として透明度を保持できませんが、本ツールでは読み込み時に内部 RGBA バッファへ変換します。消しゴムで透明化した結果は Session JSON と PNG 出力に保持されます。

## 描画ツール

- `ブラシ`: 指定した描画色、描画不透明度、ブラシサイズ、強さで塗ります。
- `消しゴム`: 対象の alpha を減算します。読み込み画像にも使用できます。
- `塗りつぶし`: クリックしたピクセルと同じ RGBA の上下左右連結領域だけを一括で塗ります。透明領域も対象にできます。
- `ぼかし`: ブラシ範囲を平均化して境界をぼかします。
- `スムース`: 近傍色へ寄せ、ぼかしより穏やかに境界をならします。
- `ノイズ除去`: 小さな孤立色領域を周囲色で補正します。

ブラシと消しゴムはストローク補間されるため、素早くマウスを動かしても線が飛びにくくなっています。

## 塗りつぶし仕様

- 判定は RGBA の完全一致です。
- 上下左右でつながっている同色領域だけを対象にします。
- 斜め接続のみのピクセルは対象外です。
- 透明ピクセル同士の連結領域も塗りつぶせます。
- 1 回のクリックで確定するツールのため、ドラッグ中に連続して別領域へ広がることはありません。

## ノイズ除去

- `ノイズ領域`: 局所補正の対象範囲を指定します。
- `ノイズ閾値`: 周辺色として許容する色差を指定します。
- 小さな孤立領域を周囲の近傍色で補正します。
- 元画像アセットは変更しません。

## 書き出し

- `Export`: アクティブなバリエーションを PNG として出力します。
- `Export All`: 書き出し対象の全バリエーションを出力します。
- Folder Batch Export: フォルダ内の複数 Texture2D に同じ設定を適用して出力します。

出力 PNG には以下が反映されます。

- 色置換結果
- 読み込み画像への直接編集
- Paint / Image Layer の合成
- 透明化した alpha

出力先が Unity Project の `Assets/` 配下の場合、TextureImporter の `Alpha Is Transparency` が自動で ON になります。`Assets/` 外へ出力した場合も PNG の alpha は保持されますが、Unity の import 設定は変更できません。

## セッションとプリセット

- `Save Session`: 画像パス、解析設定、置換ルール、バリエーション、レイヤー、描画ツール設定、読み込み画像への直接編集バッファを JSON に保存します。
- `Load Session`: 保存済み JSON を読み込みます。
- `Export Preset` / `Import Preset`: 置換ルールを JSON として共有します。
- `Preset Asset`: ScriptableObject `.asset` としてルールを共有します。

## 手動確認

- レイヤー / 描画: `docs/manual-test-layered-editing.md`
- 塗りつぶし: `docs/manual-test-fill-tool.md`
- Export alpha: `docs/manual-test-export-alpha-transparency.md`

## 注意事項

- 元画像ファイルは直接上書きしません。
- JPEG の透明化結果は PNG として出力してください。
- `Alpha Threshold` 以下の透明ピクセルは解析・置換対象外です。
- SpriteAtlas の直接編集は対象外です。
- 本ツールは Unity Editor 拡張であり、Runtime 変換機能ではありません。
