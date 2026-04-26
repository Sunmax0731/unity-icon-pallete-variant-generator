# Unity Icon Palette Variant Generator 開発計画

## 1. 開発ゴール

単一のアイテムアイコンから、Unity Editor 上で複数の色違い PNG を生成できるエディタ拡張を作成する。

最初のゴールは MVP として、以下を満たすこと。

- 画像選択
- 色抽出
- 自動グルーピング
- グループ単位の色変更
- プレビュー
- PNG 出力
- JSON 保存 / 読み込み

## 2. 実装フェーズ

## Phase 0: プロジェクト準備

### タスク

- フォルダ構成を作成
- EditorWindow の空画面を作成
- メニュー項目を追加
- 基本 Model クラスを作成
- asmdef を作成する場合は Editor 専用に分離

### 完了条件

- `Tools > Palette Variant Generator > メイン画面` から空ウィンドウが開く。
- コンパイルエラーがない。

## Phase 1: 画像読み込み・プレビュー

### タスク

- Texture2D ObjectField を追加
- AssetPath 表示
- 解析用 Texture 読み込み処理を実装
- Before Preview 表示
- 画像情報表示

### 完了条件

- Unity プロジェクト内 PNG を選ぶとプレビューが表示される。
- 画像サイズとパスが表示される。
- Read/Write 設定が無効でも解析用に読み込める。

## Phase 2: 色抽出・パレット表示

### タスク

- ColorExtractionService 実装
- Alpha Threshold 実装
- Quantize Step 実装
- PaletteColorEntry 作成
- Palette List 表示
- 出現数・出現率表示

### 完了条件

- 画像内の色が一覧表示される。
- 透明ピクセルが除外される。
- Quantize Step を変えると抽出色数が変化する。

## Phase 3: 近傍色グルーピング

### タスク

- GroupSettings 実装
- ColorDistanceService 実装
- ColorGroupingService 実装
- Target Group Count による自動グループ化
- Group List 表示
- 代表色表示

### 完了条件

- 任意のグループ数で色が自動分類される。
- グループごとの色数と比率が表示される。
- 再実行しても大きく不安定にならない。

## Phase 4: 色置換・プレビュー

### タスク

- ColorReplacementRule 実装
- グループ単位の Target Color / Blend Ratio UI 実装
- ColorReplacementService 実装
- After Preview 表示
- Preserve Alpha 実装

### 完了条件

- グループごとに色変更できる。
- 反映率 0% では元画像、100% では置換色に寄る。
- 透明部分が保持される。
- Preview ボタンで結果が更新される。

## Phase 5: PNG 出力

### タスク

- ExportSettings 実装
- Output Folder 指定 UI
- File Prefix / Suffix UI
- Conflict Mode 実装
- PngExportService 実装
- AssetDatabase.Refresh 実装

### 完了条件

- 変換後画像を PNG として出力できる。
- Unity Project ビューに出力画像が表示される。
- 上書き / スキップ / 複製名が動作する。

## Phase 6: セッション保存・読み込み

### タスク

- SessionJsonService 実装
- Save Session ボタン
- Load Session ボタン
- JSON schemaVersion 付与
- 復元時の画像パスチェック

### 完了条件

- 現在設定を JSON 保存できる。
- JSON 読み込みで設定が復元される。
- 同じ設定で同じ画像を再出力できる。

## Phase 7: バリエーション対応

### タスク

- IconVariation Model 実装
- Variation List UI
- Add / Duplicate / Remove Variation
- Variation ごとの置換ルール保持
- 複数出力対応

### 完了条件

- 1 つの元画像から複数の色違いを管理できる。
- バリエーションごとに PNG 出力できる。
- 複数バリエーションの一括出力ができる。

## Phase 8: 商品化前の仕上げ

### タスク

- UI 文言整理
- ヘルプ表示追加
- サンプル画像追加
- マニュアル作成
- 利用規約追加
- README 作成
- GitHub Release 用 ZIP / unitypackage 作成
- BOOTH 商品説明文作成

### 完了条件

- 初めて使う人でも手順が分かる。
- サンプルファイルで動作確認できる。
- 配布物に不足がない。

## 3. 優先度付きタスクリスト

## Must

- EditorWindow 起動
- 画像選択
- 色抽出
- 近傍色グルーピング
- グループ単位の色置換
- プレビュー
- PNG 出力
- JSON 保存 / 読み込み
- 元画像非破壊

## Should

- 個別カラーコード単位の置換
- 複数バリエーション管理
- 出力ファイル名ルール
- Conflict Mode
- グループ名編集
- 透明ピクセル除外設定
- Quantize Step

## Could

- 選択グループのハイライト
- ピクセルグリッド表示
- Batch Export
- Preset 適用
- Outline Protection
- UI Toolkit 化

## Won't for MVP

- レイヤー編集
- ブラシ編集
- AI 画像生成
- SpriteAtlas 直接編集
- Runtime 変換

## 4. テスト観点

## 4.1 色抽出

- 完全透明ピクセルを除外できるか
- 半透明ピクセルの扱いが Alpha Threshold に従うか
- 同色の出現数が正しく集計されるか
- Quantize Step の結果が期待通りか

## 4.2 グルーピング

- Target Group Count と同じ数のグループができるか
- 近い色が同じグループに入るか
- 出現数の多い色が代表色に反映されるか
- 空グループができた場合に警告できるか

## 4.3 置換

- ratio = 0 で元色維持
- ratio = 1 で置換色
- ratio = 0.5 で中間色
- 個別カラー設定がグループ設定より優先されるか
- アルファが維持されるか

## 4.4 出力

- PNG が正しい場所に出るか
- 既存ファイルとの競合処理が正しいか
- AssetDatabase.Refresh 後に Project に表示されるか
- 出力画像のサイズが元画像と同じか

## 4.5 セッション

- 保存 JSON が壊れていないか
- 読み込みで設定が復元されるか
- 元画像が移動・削除された場合に警告されるか
- schemaVersion が異なる場合に警告されるか

## 5. 実装上のリスクと対策

| リスク | 内容 | 対策 |
|---|---|---|
| 色数が多すぎる | アンチエイリアス画像で色が膨大になる | Quantize Step と最大色数を用意 |
| グルーピング結果が直感と違う | RGB 距離では人間の見た目とズレる | 将来的に Lab 距離を追加 |
| 輪郭色まで変わる | 黒線や影が変換対象になる | Outline Protection / 手動ロックを追加 |
| UI が複雑になる | パレット、グループ、バリエーションの情報量が多い | MVP では最小 UI、段階的にタブ化 |
| 元画像を壊す | 上書き処理の誤用 | 元画像への直接保存は禁止 |
| JSON 復元に失敗 | Unity JsonUtility の制約 | List 中心の保存形式にする |

## 6. Codex / AI Agent に依頼しやすい実装順

1. Model クラスと enum を作成
2. TextureAssetLoader を作成
3. ColorExtractionService を作成
4. ColorQuantizationService を作成
5. ColorDistanceService を作成
6. ColorGroupingService を作成
7. ColorReplacementService を作成
8. PngExportService を作成
9. SessionJsonService を作成
10. EditorWindow と Presenter を接続
11. Unit Test を追加
12. サンプル画像で手動確認

## 7. 初回リリース成果物

```text
PaletteVariantGenerator_v1.0.2.zip
  /UnityPackage or /Packages
  /Samples
  /Manual
  /Terms
  README.md
  CHANGELOG.md
```

### Manual に含める内容

- インストール手順
- 起動方法
- 基本操作
- パレット生成の考え方
- グループ数の目安
- 色置換ルールの使い方
- PNG 出力方法
- セッション保存・読み込み
- FAQ

### Terms に含める内容

- ツール本体の利用条件
- 生成画像の利用範囲
- 再配布可否
- 免責事項
- サポート範囲

