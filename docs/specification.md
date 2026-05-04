# Unity Icon Palette Variant Generator 仕様書

## メニュー

```text
Tools > Palette Variant Generator > メイン画面
Tools > Palette Variant Generator > ライセンス
Tools > Palette Variant Generator > バージョン情報
```

## 基本操作

1. `Source Image` に Texture2D / PNG / JPEG を指定する。
2. `Analyze` でパレットを抽出する。
3. `Auto Group` で近傍色をグルーピングする。
4. グループまたは色別に置換色とブレンド率を設定する。
5. 必要に応じて `Preview Mode = 描画` にし、読み込み画像またはレイヤーへ編集を加える。
6. `Preview` で結果を確認する。
7. `Export` または `Export All` で PNG を出力する。
8. `Save Session` で作業状態を保存する。

## 画面構成

- 上部: Source Image、Analyze、Auto Group、Preview、Export、Undo / Redo、Session、Help、Language。
- 左: Source 情報、解析設定、グループ設定、ツール設定、書き出し設定、プリセット。
- 中央: Preview、選択色情報、Palette。
- 右: Layers、Variations、置換ルール、色別ルール。

`Tool Settings` は折りたたみ可能とする。Preview キャンバスの高さはドラッグで変更できる。

## Preview Mode

| モード | 用途 |
|---|---|
| `選択` | Preview 上のピクセル色を選択する。 |
| `ブラシ選択` | ドラッグ範囲の複数パレット色を選択する。 |
| `描画` | 編集対象に対して Brush / Eraser / Fill などを適用する。 |

## 比較表示

- `分割比較`: 分割位置で Before / After を切り替える。
- `左右比較`: Before と After を横に並べる。
- `差分`: 変更箇所を強調する。
- Paint モード中は、比較表示より編集対象のリアルタイム反映を優先する。

## 描画対象

| 編集対象 | 仕様 |
|---|---|
| `読み込み画像` | セッション内の `sourcePixelData` を編集する。元画像は上書きしない。 |
| `アクティブレイヤー` | 選択中の描画レイヤーの `pixelData` を編集する。 |

画像レイヤー、非表示レイヤー、ロック中レイヤーは描画対象にしない。

## 描画ツール

| ツール | 仕様 |
|---|---|
| `ブラシ` | 指定色と不透明度で対象ピクセルを合成する。 |
| `消しゴム` | 対象ピクセルの alpha を減算する。読み込み画像にも適用できる。 |
| `塗りつぶし` | クリックした RGBA と同一の上下左右連結領域を一括で塗る。 |
| `ぼかし` | 指定半径内の平均色へ近づける。 |
| `スムース` | 境界を弱く平滑化する。 |
| `ノイズ除去` | 周辺色から孤立した小領域を近傍色で埋める。 |

Brush / Eraser / Blur / Smooth / NoiseRemoval はドラッグ中に補間点を生成する。Fill は 1 ストローク 1 回のみ実行する。

## JPEG 透過編集

JPEG は alpha を保持できないため、読み込み時に内部 RGBA バッファを作成する。消しゴムはそのバッファの alpha を 0 へ近づける。Export は PNG として行うため、透明化結果を保持できる。元 JPEG は変更しない。

## 色置換

置換式:

```text
output = Lerp(original, target, blendRatio)
```

優先順位:

1. `GroupUniform`: グループルールを適用する。
2. `PerColor`: 有効な色別ルールのみ適用する。
3. `Hybrid`: 色別ルールを優先し、未設定色はグループルールへフォールバックする。
4. 該当ルールなし: 変換しない。

## レイヤー合成

1. 読み込み画像編集バッファを基準にする。
2. 色置換結果を生成する。
3. 表示中の画像レイヤー / 描画レイヤーを上から順に opacity 付きで合成する。
4. `Visible in Export` OFF のパレット色は alpha 0 として扱う。

## Export

- PNG は元画像と同じ幅・高さで出力する。
- Export / Export All は現在のバリエーションとレイヤー状態を反映する。
- Folder Batch Export は同じ設定をフォルダ内画像へ適用する。
- 出力先が `Assets/` 配下の場合、出力後に TextureImporter の `Alpha Is Transparency` を ON にする。
- 出力後に `AssetDatabase.Refresh` を実行する。

## Session JSON

保存対象:

- source asset path
- analyze / group / export settings
- palette / groups / replacement rules
- variations
- layers
- active layer
- tool settings
- source edit buffer

保存形式は `JsonUtility` と相性の良い List 中心の構造とし、Dictionary や `UnityEngine.Object` 参照は保存しない。
