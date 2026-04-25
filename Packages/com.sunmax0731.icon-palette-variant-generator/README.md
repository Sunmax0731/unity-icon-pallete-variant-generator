# Unity Icon Palette Variant Generator

アイコン画像のパレット色を抽出し、複数の色違い PNG を生成する Unity Editor 拡張です。

## 起動方法

```text
Tools > Palette Variant Generator > 開く
```

## 主な機能

- PNG / Texture2D アセットからのパレット抽出
- RGB / HSV / Lab 色距離による近傍色の自動グルーピングと距離しきい値の調整
- グループ単位、色単位、Hybrid の置換ルール
- Before / After プレビューと選択色 overlay
- Add / Duplicate / Remove による複数バリエーション管理
- 単体出力と一括出力
- フォルダ内 Texture2D への一括バリエーション出力
- セッション JSON の保存 / 読み込み
- Help、言語設定、Auto Preview

## サンプル

Package Manager から以下のサンプルを import できます。

```text
Samples~/SampleIcons
```

サンプルには、透明 PNG、アンチエイリアスあり PNG、ドット絵 PNG が含まれます。

## Unity バージョン

Unity `6000.4.0f1` で検証しています。
