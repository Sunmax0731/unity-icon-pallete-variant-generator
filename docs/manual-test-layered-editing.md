# レイヤー対応描画拡張 手動確認手順

## 1. 前提

- Unity `6000.4.0f1`
- リポジトリ: `unity-icon-pallete-variant-generator`
- 検証コマンド:

```powershell
powershell -ExecutionPolicy Bypass -File tools\validation\run-editmode-tests.ps1
```

## 2. 事前準備

1. Unity でプロジェクトを開く。
2. `Tools > Palette Variant Generator > メイン画面` を開く。
3. `Assets/9cA7K7cS_400x400.jpg` または `Samples~/SampleIcons/pixel_art_64.png` を `Source Image` に設定する。
4. `Analyze` を実行する。
5. `Auto Group` を実行する。
6. `Preview` を実行する。

## 3. テストケース

### TC-UI-01 画面構成

1. Toolbar に主要操作が並んでいることを確認する。
2. 左に Settings、中央に Preview、右に Inspector / Layer / Variation があることを確認する。
3. `Tool Settings` が折りたたみ可能であることを確認する。
4. 日本語モード時、主要な設定項目と選択肢が日本語で表示されることを確認する。

### TC-UI-02 Preview サイズ変更

1. Preview 画像の直下にあるリサイズハンドルをドラッグする。

期待結果:
- Preview の表示キャンバス高さが追従して変わる
- ドラッグ後も Preview 操作が継続できる

### TC-LYR-01 Paint Layer 追加

1. `Add Paint Layer` を押す。
2. Layers に新規レイヤーが追加されることを確認する。
3. 新規レイヤーが active になることを確認する。

### TC-DRW-00 読み込み画像へ直接編集

1. `Tool Settings` の `編集対象` を `読み込み画像` にする。
2. `Preview Mode` を `Paint` にする。
3. `Tool` を `Brush` にする。
4. Preview 上をドラッグする。

期待結果:
- レイヤーを追加しなくても描画できる
- 読み込み画像そのものに対して描画結果が反映される

### TC-DRW-01 Brush

1. `編集対象` を `アクティブレイヤー` にする。
2. `Preview Mode` を `Paint` にする。
3. Tool を `Brush` にする。
4. `Add Paint Layer` でレイヤーを追加して active にする。
5. Color を赤、不透明度を `0.5` 以上に設定する。
6. Preview 上をドラッグする。

期待結果:
- ドラッグ中も Preview が白くならず、ストロークがリアルタイムで見える
- active layer に描画が反映される
- Composite Preview に結果が反映される

### TC-DRW-02 Eraser

1. 同じレイヤーを active にしたまま Tool を `Eraser` にする。
2. 塗った箇所をドラッグする。

期待結果:
- ドラッグ中も Preview が白くならず、消去範囲がリアルタイムで見える
- 該当部分の alpha が減る
- 下のベース画像が見える

### TC-DRW-03 Blur

1. Tool を `Blur` にする。
2. 塗り境界付近をドラッグする。

期待結果:
- 境界が平滑化されてにじむ

### TC-DRW-04 Smooth

1. Tool を `Smooth` にする。
2. ざらついた境界をドラッグする。

期待結果:
- Blur より穏やかに境界がならされる

### TC-DRW-05 Noise Removal

1. 小さな孤立ドットがある箇所か、ブラシで意図的に `1px` ノイズを作る。
2. Tool を `NoiseRemoval` にする。
3. 高コントラストなノイズを消す場合は `Noise Threshold` を高めに設定する。
4. ノイズ周辺をドラッグする。

期待結果:
- 小さな孤立ピクセルが周辺色で埋まる
- 透明に囲まれたノイズは透明へ戻せる

### TC-LYR-02 Image Layer

1. `Add Image Layer` を押す。
2. 追加用の PNG を選択する。
3. opacity を `0.5` に下げる。

期待結果:
- レイヤー一覧に画像レイヤーが追加される
- Composite Preview で透過合成される

### TC-LYR-03 Visible / Lock

1. Layer の visible を OFF にする。
2. Preview から該当レイヤーの効果が消えることを確認する。
3. visible を ON に戻す。
4. lock を ON にして描画を試す。

期待結果:
- visible OFF で非表示になる
- lock 中は描画できず、警告または無効化状態になる

### TC-SES-01 Session Save / Load

1. 描画済みの状態で `Save Session` を押す。
2. `Load Session` を押し、保存した JSON を読み込む。

期待結果:
- Layer 構成、active layer、描画結果、Tool 設定が復元される
- 読み込み画像への直接編集内容も復元される

### TC-EXP-01 Export

1. 描画とレイヤーを含んだ状態で `Export` を実行する。
2. 出力 PNG を Unity または画像ビューアで確認する。

期待結果:
- Preview と同じ見た目で PNG が出力される
- 元画像ファイルは変更されていない
