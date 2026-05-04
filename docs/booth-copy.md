# Unity Icon Palette Variant Generator BOOTH 商品ページ

## 根拠

- Repository: `D:\Claude\UnityEditor-Dev\unity-icon-pallete-variant-generator`
- Version: `1.1.0`
- Confirmed date: 2026-05-05
- Unity: `6000.4.0f1`
- Source files: `README.md`, `CHANGELOG.md`, `docs/manual.md`, `docs/release-notes-v1.1.0.md`, `docs/validation-checklist.md`
- BOOTH autofill reference: `D:\Claude\UnityEditor-Dev\booth-product-page-autofill\popup.js`

## 商品名

Unity Icon Palette Variant Generator

## 概要

Unity Editor 上でアイコン画像のパレットを解析し、色違い PNG を作成するエディタ拡張です。近傍色の自動グルーピング、色置換、Before / After Preview、レイヤー合成、ブラシや塗りつぶしなどの描画ツールを使い、1 枚のアイコンから複数のカラーバリエーションを効率よく作れます。

## 詳細

### 主な特徴

- PNG / JPEG / Texture2D からパレット色を抽出できます。
- RGB / HSV / Lab の距離モードで近い色をグルーピングできます。
- グループ単位、色単位、Hybrid の置換ルールで細かく色を調整できます。
- Before / After、分割比較、左右比較、差分 Preview で結果を確認できます。
- Paint Layer / Image Layer を重ね、表示、ロック、不透明度、順序を管理できます。
- ブラシ、消しゴム、塗りつぶし、ぼかし、スムース、ノイズ除去を Preview 上で使えます。
- 読み込み画像へ直接編集できます。元画像ファイルは上書きしません。
- JPEG 読み込み画像も内部で RGBA 化し、消しゴムで透明化して PNG に出力できます。
- `Assets/` 配下へ出力した PNG は Unity の `Alpha Is Transparency` が自動で ON になります。
- Session JSON と Preset により、作業状態や置換ルールを再利用できます。

### 想定用途

- ゲーム内アイコン、素材、宝石、植物、料理、装備などの色違い作成。
- BOOTH やアセット配布用のカラーバリエーション画像作成。
- Unity Editor 内で軽い塗り足し、消去、透明化、ノイズ除去を済ませたい場合。
- 同じ形状のアイコンを複数色で量産したい場合。

### 使い方

1. Unity Package Manager または `.unitypackage` から導入します。
2. `Tools > Palette Variant Generator > メイン画面` を開きます。
3. `Source Image` に PNG / JPEG / Texture2D を指定します。
4. `Analyze` でパレットを抽出し、`Auto Group` で近い色をまとめます。
5. 置換ルール、レイヤー、描画ツールを使って見た目を調整します。
6. `Preview` で結果を確認します。
7. `Export` または `Export All` で PNG を出力します。

## 内容物

- UPM package ZIP
- `.unitypackage`
- サンプルアイコン
- README
- Manual
- Validation checklist
- Release notes
- MIT License

## 対応環境

- Unity `6000.4.0f1` で検証済み
- Windows 環境で検証済み
- Unity Editor 拡張として動作
- Runtime 用コンポーネントではありません

## アップデート履歴

### v1.1.0

- レイヤー機能を追加しました。
- 読み込み画像への直接編集を追加しました。
- ブラシ、消しゴム、塗りつぶし、ぼかし、スムース、ノイズ除去を追加しました。
- JPEG 読み込み画像を内部 RGBA バッファとして扱い、消しゴムで透明化して PNG 出力できるようにしました。
- Preview キャンバスのリサイズと Paint 中のリアルタイム反映を改善しました。
- ブラシストローク補間により、高速ドラッグ時の描画飛びを抑えました。
- 出力 PNG の `Alpha Is Transparency` を自動で ON にする処理を追加しました。

## 注意事項

- 元画像ファイルは直接上書きしません。編集結果は Session JSON と出力 PNG に反映されます。
- JPEG 自体は alpha を保持できないため、透明化した結果は PNG として出力してください。
- `Alpha Is Transparency` の自動設定は、Unity Project の `Assets/` 配下に出力した PNG が対象です。
- SpriteAtlas の直接編集は対象外です。
- 本ツールは Unity Editor 拡張です。ゲーム実行時の画像変換機能ではありません。

## タグ案

- Unity
- UnityEditor
- エディタ拡張
- アイコン
- カラーバリエーション
- パレット
- PNG
- ゲーム制作
- BOOTH素材
- ツール

## 商品画像構成案

1. メイン画面の全体スクリーンショット。左に設定、中央に Preview、右に Layers / Rules が見える状態。
2. Before / After で色違いアイコンを比較しているスクリーンショット。
3. ブラシ、消しゴム、塗りつぶしを使った編集例。
4. レイヤー一覧と Paint Layer / Image Layer の合成例。
5. Export 後の PNG と `Alpha Is Transparency` ON の Inspector 例。

## BOOTH入力設定

- タグ、価格、カテゴリ、年齢制限、バリエーション、作品ファイルは BOOTH 画面で手動設定してください。
- 商品画像は BOOTH Product Page Autofill の popup で画像ファイルを選択してください。
