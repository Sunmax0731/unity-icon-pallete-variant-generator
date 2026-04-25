# マニュアル

## 概要

`Unity Icon Palette Variant Generator` は、元画像を変更せずにアイコン PNG から色違いバリエーションを作成する Unity Editor 拡張です。

## 導入方法

以下のいずれかの方法で導入します。

### Git URL

Unity Package Manager から、このリポジトリの Git URL を指定してパッケージを追加します。

### Release ZIP

GitHub Releases から `PaletteVariantGenerator_v1.0.0.zip` をダウンロードして展開し、パッケージフォルダを Unity プロジェクトに追加します。

## 起動方法

```text
Tools > Palette Variant Generator > 開く
```

## 基本操作

1. `Source Image` に PNG または Texture2D アセットを指定する。
2. `Analyze` を押す。
3. 必要に応じてパレット抽出設定を調整する。
4. `Auto Group` を押す。
5. グループ単位または色単位の置換ルールを編集する。
6. `Preview` を押して結果を確認する。
7. 必要に応じてバリエーションを追加または複製する。
8. active なバリエーションだけ出力する場合は `Export`、出力対象の全バリエーションを出力する場合は `Export All` を押す。
9. 設定を再利用したい場合は `Save Session` で JSON 保存する。

## プリセット操作

- `Export Preset`: 現在のグループ置換設定と色別ルールを JSON として保存します。
- `Import Preset`: 保存済みプリセットを現在のセッションへ適用します。
- 現在のセッションに存在しない group がプリセットに含まれる場合、その項目はスキップされ、report panel に warning が表示されます。

## 置換モード

- `GroupUniform`: グループ内の全色に、同じ置換色とブレンド率を適用します。
- `PerColor`: 有効な色別ルールだけを適用します。
- `Hybrid`: 有効な色別ルールを優先し、未設定色はグループ設定を使います。

## グループ編集

- Palette 一覧の group popup から、解析後の色を別グループへ移動できます。
- グループ行の `固定` を有効にすると、そのグループからの移動や、そのグループへの移動を防げます。
- 手動で移動した結果はセッション JSON に保存されます。

## バリエーション操作

- `Add`: 新しいバリエーションを追加します。
- `Duplicate`: 選択中のバリエーションを複製します。
- `Remove`: バリエーションが2件以上ある場合に、選択中のバリエーションを削除します。
- `Export` / `Skip`: `Export All` の対象に含めるかどうかを切り替えます。
- `File Suffix`: 出力ファイル名の接尾辞を指定します。
- `Output File`: 出力予定の PNG ファイル名を確認できます。

## プレビュー操作

- `Preview`: 手動で生成結果を更新します。
- `Auto Preview`: 色、ブレンド率、置換モードの変更後、短い debounce を挟んで自動更新します。
- 大きい画像では、編集中の再計算が増えすぎないよう Auto Preview の待機時間が長くなります。
- `Compare`: `SideBySide` では Before / After を左右に並べ、`Split` では1つのプレビュー内で比較します。
- `Zoom`: プレビューを拡大します。拡大中はプレビュー上をドラッグして表示位置を動かせます。
- `表示リセット`: ズーム、パン、split 位置を初期状態に戻します。

## サンプル

検証用サンプルは以下に含まれています。

```text
Packages/com.sunmax0731.icon-palette-variant-generator/Samples~/SampleIcons
```

## 注意事項

- 元画像は上書きしません。
- PNG は設定した出力先フォルダに書き出されます。
- `Alpha Threshold` 以下の透明ピクセルは、解析と置換の対象外です。
