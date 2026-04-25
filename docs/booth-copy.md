# BOOTH 商品説明文案

## タイトル案

Unity Icon Palette Variant Generator

## 短い説明

Unity Editor 上でアイコン画像の色を抽出し、複数の色違い PNG を元画像非破壊で生成するエディタ拡張です。

## 商品説明

`Unity Icon Palette Variant Generator` は、1枚の PNG / Texture2D からパレット色を抽出し、近い色をグループ化した上で、グループ単位または色単位の置換ルールを使って色違いアイコンを作成する Unity Editor 拡張です。

鉱石、薬草、料理、装備、宝箱、素材アイコンなど、同じ形状で色だけ異なるアイコンを量産したいときに使えます。

## 主な機能

- PNG / Texture2D からのパレット抽出
- 近傍色の自動グルーピング
- グループ単位、色単位、Hybrid の置換モード
- Before / After プレビュー
- 選択色のプレビュー overlay
- 複数バリエーション管理
- 複数 PNG の一括出力
- セッション JSON 保存 / 読み込み
- Help 表示、言語設定、Auto Preview
- 検証用サンプル PNG 付属

## 動作確認

- Unity `6000.4.0f1`
- Windows 環境で検証

## 注意事項

- 本ツールは Unity Editor 拡張です。Runtime 用の変換機能ではありません。
- 元画像を直接上書きしない設計ですが、出力先とファイル名設定は利用者側で確認してください。
- SpriteAtlas の直接編集は v1.0.1 では対象外です。

## 同梱物

- UPM package
- サンプル PNG
- マニュアル
- 利用条件
- README
- CHANGELOG
