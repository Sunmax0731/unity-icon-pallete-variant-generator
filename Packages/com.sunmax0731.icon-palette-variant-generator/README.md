# Unity Icon Palette Variant Generator

Unity Editor 上でアイコン画像の色を解析し、複数の色違い PNG を生成する Editor 拡張です。

## 起動方法

```text
Tools > Palette Variant Generator > メイン画面
Tools > Palette Variant Generator > ライセンス
Tools > Palette Variant Generator > バージョン情報
```

## 主な機能

- PNG / JPG / Texture2D からのパレット抽出
- RGB / HSV / Lab 距離による近傍色グルーピング
- グループ単位、色単位、Hybrid の置換ルール
- Before / After / Split / SideBySide / Difference Preview
- レイヤー合成、描画レイヤー、画像レイヤー
- 読み込み画像への直接描画、JPEG の内部 RGBA 化
- ブラシ、消しゴム、塗りつぶし、ぼかし、スムース、ノイズ除去
- Export / Export All / Folder Batch Export
- `Assets/` 配下の出力 PNG に対する `Alpha Is Transparency` 自動設定
- Session JSON、Rule Preset JSON、ScriptableObject Preset
- 日本語 / 英語 UI、Help、Auto Preview、Undo / Redo

## 基本操作

1. `Source Image` に画像を指定します。
2. `Analyze` でパレットを抽出します。
3. `Auto Group` で近傍色をグループ化します。
4. 置換ルールを調整して `Preview` します。
5. 必要に応じて Preview 上で描画、消去、塗りつぶしを行います。
6. `Export` または `Export All` で PNG を出力します。

## 描画ツール

- `ブラシ`: 指定色と不透明度で対象を塗ります。
- `消しゴム`: 対象の alpha を減算し、透明部分を作ります。
- `塗りつぶし`: クリックしたピクセルと同じ RGBA の上下左右連結領域を一括で塗ります。
- `ぼかし` / `スムース`: 局所的な境界調整を行います。
- `ノイズ除去`: 小さな孤立領域を周辺色で補正します。

`編集対象` は `読み込み画像` と `アクティブレイヤー` から選べます。読み込み画像を編集しても元アセットは上書きされません。

## サンプル

```text
Samples~/SampleIcons
```

## 検証済み環境

Unity `6000.4.0f1`

## License

MIT License. 詳細は `LICENSE.md` を参照してください。
