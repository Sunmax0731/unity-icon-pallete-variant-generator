# レイヤー対応描画拡張 手動確認手順

## 1. 前提

- Unity `6000.4.0f1`
- リポジトリ: `unity-icon-pallete-variant-generator`
- 実行コマンド:

```powershell
powershell -ExecutionPolicy Bypass -File tools\validation\run-editmode-tests.ps1
```

## 2. 事前準備

1. Unity でプロジェクトを開く。
2. `Tools > Palette Variant Generator > メイン画面` を開く。
3. `Assets/9cA7K7cS_400x400.jpg` か `Samples~/SampleIcons/pixel_art_64.png` を Source に指定する。
4. `Analyze` を実行する。
5. `Auto Group` を実行する。
6. `Preview` を実行する。

## 3. テストケース

### TC-UI-01 画面構成

1. Toolbar に主要操作が並んでいることを確認する。
2. 左に Settings、中央に Preview、右に Inspector/Layer 情報、下部に Report があることを確認する。
3. 未選択状態で空状態メッセージまたは誘導文が表示されることを確認する。

### TC-LYR-01 Paint Layer 追加

1. `Add Paint Layer` を押す。
2. Layers に新規レイヤーが追加されることを確認する。
3. 新規レイヤーが active になることを確認する。

### TC-DRW-01 Brush

1. Tool を `Brush` にする。
2. Color を赤、不透明度を 0.5 以上に設定する。
3. Preview 上をドラッグする。

期待結果:
- active layer に赤い塗りが乗る
- Composite Preview に即時反映される

### TC-DRW-02 Eraser

1. 同じレイヤーを active にしたまま Tool を `Eraser` にする。
2. 塗った箇所をドラッグする。

期待結果:
- 該当部分の alpha が減る
- 下のベース画像が見える

### TC-DRW-03 Blur

1. Tool を `Blur` にする。
2. 塗り境界付近をドラッグする。

期待結果:
- 境界が平均化されてにじむ

### TC-DRW-04 Smooth

1. Tool を `Smooth` にする。
2. ざらついた箇所をドラッグする。

期待結果:
- Blur より輪郭を残しつつ色差が和らぐ

### TC-DRW-05 Noise Removal

1. 小さな孤立ドットがある画像か、ブラシで人工的に 1px ノイズを作る。
2. Tool を `NoiseRemoval` にする。
3. ノイズ付近をドラッグする。

期待結果:
- 小さな孤立ピクセルが近傍色で埋まる

### TC-LYR-02 Image Layer

1. `Add Image Layer` を押す。
2. 任意の PNG を指定する。
3. opacity を 0.5 に下げる。

期待結果:
- レイヤー一覧に画像レイヤーが増える
- Composite Preview で半透明に重なる

### TC-LYR-03 Visible / Lock

1. active layer の visible を OFF にする。
2. 再度 ON にする。
3. lock を ON にする。
4. Brush を使って描こうとする。

期待結果:
- visible OFF 中はプレビューに反映されない
- lock ON 中は編集されず、警告が出る

### TC-SES-01 Session Save / Load

1. レイヤーを 2 枚以上追加し、各レイヤーへ編集を加える。
2. `Save Session` で JSON を保存する。
3. Window を閉じて再度開く。
4. `Load Session` で保存した JSON を開く。

期待結果:
- レイヤー数、順序、表示状態、描画結果が復元される
- active layer と tool settings が復元される

### TC-EXP-01 Export

1. Paint Layer と Image Layer が重なった状態を作る。
2. `Preview` を更新する。
3. `Export` を実行する。
4. 出力 PNG を Project ビューで選択し、内容を確認する。

期待結果:
- Preview と同じ見た目で PNG が出力される
- 元画像は変更されていない
