# レイヤー対応描画拡張 手動確認

## 前提

- Unity `6000.4.0f1`
- メニュー: `Tools > Palette Variant Generator > メイン画面`
- 自動検証:

```powershell
powershell -ExecutionPolicy Bypass -File tools\validation\run-editmode-tests.ps1
```

## 事前準備

1. Unity で本プロジェクトを開く。
2. `Source Image` に PNG または JPEG を指定する。
3. `Analyze` を実行する。
4. `Auto Group` を実行する。
5. `Preview` を実行する。
6. Language を `Japanese` にする。

## TC-UI-01 画面構成

1. 上部 toolbar に Source / Analyze / Auto Group / Preview / Export / Session / Help / Language があることを確認する。
2. 左に解析設定、グループ設定、ツール設定、書き出し設定があることを確認する。
3. 中央に Preview と Palette があることを確認する。
4. 右に Layers、Variations、置換ルール、色別ルールがあることを確認する。

期待結果:

- 日本語モードでは主要 UI 文言が日本語で表示される。
- `ツール設定` を折りたためる。

## TC-UI-02 Preview サイズ変更

1. Preview 直下のリサイズハンドルをドラッグする。

期待結果:

- Preview 表示領域の高さが変わる。
- サイズ変更後も Preview のクリック、ドラッグ、ズームが動作する。

## TC-LYR-01 Paint Layer 追加

1. `描画レイヤー追加` を押す。

期待結果:

- Layers に新しい描画レイヤーが追加される。
- 新しいレイヤーが active になる。

## TC-LYR-02 Image Layer 追加

1. `画像レイヤー追加` を押す。
2. 追加する PNG を選択する。
3. レイヤー opacity を変更する。

期待結果:

- 画像レイヤーが一覧に追加される。
- Preview で opacity 付き合成が確認できる。

## TC-LYR-03 Visible / Lock

1. Layer の visible を OFF にする。
2. visible を ON に戻す。
3. lock を ON にして描画を試す。

期待結果:

- visible OFF で該当レイヤーが非表示になる。
- lock ON では描画できず、状態が壊れない。

## TC-DRW-00 読み込み画像へ直接編集

1. `ツール設定 > 編集対象` を `読み込み画像` にする。
2. `プレビューモード` を `描画` にする。
3. `ツール` を `ブラシ` にする。
4. Preview 上をドラッグする。

期待結果:

- レイヤーがなくても描画できる。
- 元画像ファイルは上書きされない。
- 描画結果は Preview と Export PNG に反映される。

## TC-DRW-01 Brush

1. `編集対象` を `アクティブレイヤー` にする。
2. `描画レイヤー追加` で active layer を作る。
3. `ツール` を `ブラシ` にする。
4. Brush Size、色、不透明度を設定する。
5. Preview 上をゆっくり / 速くドラッグする。

期待結果:

- ドラッグ中も白くならず、リアルタイムに描画が見える。
- 高速ドラッグしても線が大きく飛ばない。
- active layer に描画される。

## TC-DRW-02 Eraser

1. Brush で描画済みの状態にする。
2. `ツール` を `消しゴム` にする。
3. 描画箇所をドラッグする。

期待結果:

- 該当箇所の alpha が下がり、下の画像が見える。
- 読み込み画像対象でも透明化できる。
- JPEG 読み込み画像の場合も Export PNG で透明化が保持される。

## TC-DRW-03 Blur

1. `ツール` を `ぼかし` にする。
2. 色境界付近をドラッグする。

期待結果:

- 境界がぼやける。
- Preview がリアルタイムに更新される。

## TC-DRW-04 Smooth

1. `ツール` を `スムース` にする。
2. ざらついた境界をドラッグする。

期待結果:

- 境界が弱く平滑化される。

## TC-DRW-05 Noise Removal

1. 小さな孤立ドットがある状態にする。
2. `ツール` を `ノイズ除去` にする。
3. Noise Threshold を調整し、ノイズ周辺をドラッグする。

期待結果:

- 孤立ピクセルが周辺色で埋まる。
- 大きな領域は不用意に消えない。

## TC-DRW-06 Fill

詳細は `docs/manual-test-fill-tool.md` を参照する。

期待結果:

- クリックしたピクセルと同じ RGBA の上下左右連結領域だけが塗りつぶされる。
- 透明領域も対象にできる。

## TC-SES-01 Session Save / Load

1. レイヤーと描画結果がある状態で `Save Session` を実行する。
2. `Load Session` で保存した JSON を読み込む。

期待結果:

- レイヤー構成、active layer、描画結果、ツール設定が復元される。
- 読み込み画像への直接編集内容も復元される。

## TC-EXP-01 Export

1. 描画、レイヤー、色置換を含む状態で `Export` を実行する。
2. 出力 PNG を Unity または外部ビューアで確認する。

期待結果:

- Preview と同じ見た目の PNG が出力される。
- 元画像ファイルは変更されていない。
- `Assets/` 配下に出力した場合、`Alpha Is Transparency` が ON になる。
