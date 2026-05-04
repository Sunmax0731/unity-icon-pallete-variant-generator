# レイヤー対応描画拡張 要件定義

## 1. 背景

`unity-icon-pallete-variant-generator` は色解析と色置換を中心にしたツールだったが、最終調整のために外部 DCC ツールへ戻る必要があった。Issue `#48` では、Unity Editor 内で完結する軽量な描画、レイヤー、直接編集、透過編集ワークフローを追加する。

## 2. 目的

- Unity Editor 拡張のデザインガイドラインに沿って画面の責務を整理する。
- Preview 上で読み込み画像またはレイヤーを直接編集できるようにする。
- ブラシ、消しゴム、塗りつぶし、ぼかし、スムース、ノイズ除去を追加する。
- 複数画像や描画結果を重ねられるレイヤー機能を追加する。
- JPEG 由来画像でも内部 RGBA バッファにより透過編集できるようにする。
- Preview / Export / Session に編集結果を一貫して反映する。

## 3. 対象範囲

### 対象

- メイン画面の UI 再設計
- Tool Settings の折りたたみ
- Preview キャンバスのリサイズ
- Paint Layer / Image Layer
- Layer visible / lock / opacity / order / duplicate / delete
- `編集対象 = 読み込み画像` / `アクティブレイヤー`
- Brush / Eraser / Fill / Blur / Smooth / NoiseRemoval
- JPEG の内部 RGBA 化
- 出力 PNG の alpha 保持と `Alpha Is Transparency` 自動設定
- Session JSON の保存 / 読み込み
- 自動検証と手動確認ドキュメント

### 非対象

- SpriteAtlas の直接編集
- ベクターレイヤー
- Photoshop 相当の高度なブレンドモード
- アニメーションタイムライン
- Runtime 機能

## 4. 機能要件

### FR-UI-001 画面設計

- Toolbar / Settings / Preview Workspace / Inspector / Report の責務を分離する。
- 日本語モードでは主要 UI を日本語表示にする。
- Tool Settings は折りたたみ可能にする。
- Preview キャンバスはドラッグで高さを変更できる。

### FR-LYR-001 レイヤー管理

- セッションは複数レイヤーを保持できる。
- レイヤーは `Paint` と `Image` を持つ。
- 各レイヤーは `id`、`displayName`、`visible`、`locked`、`opacity`、`blendMode`、`offsetX`、`offsetY`、`sourceAssetPath`、`pixelData` を持つ。
- レイヤーの追加、複製、削除、並び替え、表示切替、ロック切替、不透明度変更を行える。
- `visible = false` のレイヤーは Preview / Export へ合成しない。
- `locked = true` のレイヤーは描画編集できない。

### FR-DRW-001 編集対象

- `読み込み画像` を選ぶと、セッション内の `sourcePixelData` を編集する。
- `アクティブレイヤー` を選ぶと、選択中レイヤーの `pixelData` を編集する。
- 元画像アセットは直接上書きしない。
- 編集結果は Preview、Export、Session JSON に反映する。

### FR-DRW-002 ブラシ

- 指定した色、描画不透明度、ブラシサイズ、強さで対象を塗る。
- ドラッグ中にストローク補間し、線飛びを抑える。
- 編集中も Preview にリアルタイムで反映する。

### FR-DRW-003 消しゴム

- 対象の alpha を減算する。
- 読み込み画像とアクティブレイヤーの両方に適用できる。
- JPEG 由来の読み込み画像にも内部 RGBA バッファ経由で透明化できる。

### FR-DRW-004 塗りつぶし

- クリックしたピクセルと同じ RGBA の上下左右連結領域だけを塗りつぶす。
- 斜め接続は連結とみなさない。
- 透明ピクセル同士の連結領域も対象にする。
- ドラッグではなく 1 回のクリックで 1 領域を確定する。

### FR-DRW-005 ぼかし / スムース

- ぼかしはブラシ範囲内を平均化する。
- スムースは近傍色へ寄せ、ぼかしより穏やかに境界をならす。

### FR-DRW-006 ノイズ除去

- 小さな孤立領域を周辺色で補正する。
- ツールとして局所適用できる。

### FR-EXP-001 書き出し

- Preview と Export は同一の合成経路を使う。
- 色置換、読み込み画像への直接編集、レイヤー合成を PNG に反映する。
- 出力先が `Assets/` 配下の場合、`Alpha Is Transparency` を ON にする。

### FR-SES-001 セッション

- レイヤー構成、アクティブレイヤー、描画ツール設定、読み込み画像の直接編集バッファを保存 / 復元する。
- 旧セッション読み込み時は既定値で補完する。

## 5. 受け入れ条件

- Unity `6000.4.0f1` でコンパイルエラーがない。
- Brush / Eraser / Fill / Blur / Smooth / NoiseRemoval が Preview 上で動作する。
- 読み込み画像とアクティブレイヤーの両方を編集できる。
- JPEG 読み込み画像に消しゴムを使い、透明化結果を PNG に出力できる。
- 複数レイヤーを重ねた結果が Preview / Export に反映される。
- Session JSON にレイヤーと直接編集結果が保存される。
- `ISSUE48_*_VALIDATION=PASS` marker が出力される。
