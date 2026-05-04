# Unity Icon Palette Variant Generator 要件定義

## 目的

Unity Editor 上で 1 枚のアイコン画像から複数の色違い PNG を効率よく生成する。色解析、近傍色グルーピング、色置換に加えて、v1.1.0 では Preview 上のレイヤー編集と読み込み画像への直接編集を提供する。

## 対象ユーザー

- Unity で 2D / UI / RPG / MMORPG などのアイコン素材を扱う開発者。
- 同一アイコンの属性違い、ランク違い、テーマ違いを大量に作成したい制作者。
- Photoshop などの DCC ツールを開かず、Unity Editor 内で軽微な補正や透過編集を完結したいユーザー。

## 対象範囲

- Unity Editor 拡張として提供する。
- ランタイム機能は提供しない。
- 元画像アセットは直接上書きしない。
- 出力は別 PNG ファイルとして保存する。
- セッション、プリセット、バリエーションは JSON または ScriptableObject で保存する。

## 機能要件

### 画像解析

- Texture2D / PNG / JPEG を `Source Image` として選択できる。
- 透明ピクセルは `Alpha Threshold` 以下を解析対象外にできる。
- `Quantize Step` により近似色をまとめられる。
- 出現数、出現率、代表色をパレットとして表示できる。

### 色グルーピング

- RGB / HSV / Lab の距離計算を選択できる。
- 指定したグループ数へ近傍色を自動分類できる。
- `Max Color Distance` により遠い色の過剰結合を抑制できる。

### 色置換

- グループ単位の一括置換を行える。
- 色別ルールを設定できる。
- `GroupUniform` / `PerColor` / `Hybrid` の優先順位を選べる。
- 置換式は `Lerp(original, target, blendRatio)` とする。
- Alpha は維持する。ただし明示的な透過編集や `Visible in Export` OFF は alpha を変更する。

### Preview

- Before / After / Split / Difference / Side By Side を表示できる。
- Preview 上で色選択、ドラッグパン、ズーム、選択色ハイライトを行える。
- 表示カンバスはドラッグで高さを調整できる。

### レイヤー

- 描画レイヤーと画像レイヤーを追加できる。
- レイヤーの表示、ロック、順序、名前、opacity を編集できる。
- レイヤーはソース画像と色置換結果の上に合成される。
- Export / Export All / Folder Batch Export にレイヤー合成を反映する。

### 描画ツール

- `編集対象` として `読み込み画像` または `アクティブレイヤー` を選べる。
- Brush / Eraser / Fill / Blur / Smooth / NoiseRemoval を提供する。
- Paint モード中はドラッグ結果をリアルタイムに Preview へ反映する。
- 高速ドラッグ時も線が飛ばないよう、ストローク間を補間する。
- Lock されたレイヤーには描画できない。

### 読み込み画像への直接編集

- `編集対象 = 読み込み画像` の場合、セッション内の RGBA バッファを編集する。
- 元アセットは上書きしない。
- Brush、Eraser、Fill、Blur、Smooth、NoiseRemoval を適用できる。
- JPEG は内部で RGBA バッファへ正規化し、消しゴムで alpha を 0 にできる。

### 塗りつぶし

- クリックしたピクセルと同じ RGBA の上下左右連結領域を対象にする。
- 斜め接続は対象外にする。
- 透明ピクセル同士の連結領域も対象にする。
- 1 クリックにつき 1 回だけ実行し、ドラッグ中に連続実行しない。

### Export

- 元画像と同じ幅・高さの PNG を出力する。
- 色置換、読み込み画像への直接編集、レイヤー合成、透明化を反映する。
- 出力後に `AssetDatabase.Refresh` を行う。
- 出力先が Unity Project の `Assets/` 配下の場合、`Alpha Is Transparency` を自動で ON にする。

### Session / Preset

- 画像パス、解析設定、グループ、置換ルール、バリエーション、レイヤー、描画設定、読み込み画像編集バッファを保存できる。
- 旧セッションに存在しないフィールドは安全な既定値で復元する。

## 非機能要件

- Unity `6000.4.0f1` でコンパイルできる。
- EditorWindow に重い画像処理を詰め込まず、Service に分離する。
- 毎フレームの全画像再解析を避ける。
- 一時 `Texture2D` は不要になった時点で破棄する。
- UI は Unity Editor 拡張として自然なパネル構成にする。
- 日本語モードでは主要 UI 文言を日本語で表示する。

## 受け入れ基準

- `tools\validation\run-editmode-tests.ps1` が成功する。
- 手動確認で Brush / Eraser / Fill / Blur / Smooth / NoiseRemoval が Preview 上で動作する。
- JPEG 読み込み画像を消しゴムで透明化し、PNG 出力で alpha が保持される。
- `Assets/` 配下の出力 PNG で `Alpha Is Transparency` が ON になる。
- ZIP と `.unitypackage` の両方を生成し、検証できる。
